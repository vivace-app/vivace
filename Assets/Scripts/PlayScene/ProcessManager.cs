using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Project.Scripts.Model;
using Tools.AssetBundle;
using Tools.Authentication;
using Tools.Firestore;
using Tools.PlayStatus;
using Tools.Score;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayScene
{
    public class ProcessManager : MonoBehaviour
    {
        private readonly AuthenticationHandler _auth = new();

        private const int LaneCount = 7; // レーンの数
        public const float LaneWidth = 0.3f; // レーンの太さ( = ノーツの太さ )
        public const float Speed = 5f;

        public static bool isPose = true;
        private static float _currentTime;
        private static float _endTime;

        private bool _hasStarted;
        private bool _hasPosted;

        private static Music _music;
        private Level _level;
        private string _musicName;

        [SerializeField] private GameObject normalNoteGameObject;
        [SerializeField] private GameObject flickNoteGameObject;

        private static List<QueuedNote>[] _queueNotes;
        private static List<QueuedNote>[] _generatedNotes;

        private readonly List<List<GameObject>> _preGenerateNormalNotes = new();
        private readonly List<List<GameObject>> _preGenerateFlickNotes = new();

        private const int PreGenerateNormalNotesPerLane = 10;
        private const int PreGenerateFlickNotesPerLane = 6;

        private void Awake() => Application.targetFrameRate = 60;

        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            _auth.Start(null);

            var index = PlayStatusHandler.GetSelectedMusic();
            _level = PlayStatusHandler.GetSelectedLevel();
            var assetBundle = AssetBundleHandler.GetAssetBundle(index);
            _musicName = assetBundle.name;
            var audioClip = assetBundle.LoadAsset<AudioClip>(_musicName);
            View.instance.BgmAudioClip = audioClip;
            _endTime = audioClip.length;
            View.instance.setOnClickPauseCustomButtonAction = () =>
            {
                Pause();
                View.instance.setPauseModalVisible = true;
            };
            View.instance.setOnClickCancelCustomButtonAction = () =>
            {
                Play();
                View.instance.setPauseModalVisible = false;
            };
            View.instance.setOnClickGiveUpCustomButtonAction = () => SceneManager.LoadScene("SelectScene");

            var jsonFile = assetBundle.LoadAsset<TextAsset>(_musicName + "_" + _level.ToString().ToLower());
            // TODO: var jsonFile = Resources.Load("maiden_voyage_master_proto") as TextAsset;
            if (!jsonFile) throw new Exception("譜面データが無効です");
            var inputString = jsonFile.ToString();
            _music = JsonUtility.FromJson<Music>(inputString);

            ScoreHandler.Initialize(_music.notes.Length);
            ScoreHandler.OnComboChanged += combo => View.instance.ComboText = combo;
            ScoreHandler.OnScoreChanged += score => View.instance.ScoreText = score;

            /*
             * _queueNotes[0] には、0番目のレーンのノーツ生成情報が リスト（QueuedNote）型 で格納されている。
             * _queueNotes[0] 〜 _queueNotes[(レーン数)] まで存在する。
             */
            _queueNotes = new List<QueuedNote>[_music.maxBlock];
            for (var i = 0; i < _queueNotes.Length; i++) _queueNotes[i] = new List<QueuedNote>();

            /*
             * _generatedNotes[0] には、0番目のレーンに生成済みのノーツ情報が リスト（QueuedNote）型 で格納されている。
             * _generatedNotes[0] 〜 _generatedNotes[(レーン数)] まで存在する。
             */
            _generatedNotes = new List<QueuedNote>[_music.maxBlock];
            for (var i = 0; i < _generatedNotes.Length; i++) _generatedNotes[i] = new List<QueuedNote>();

            foreach (var note in _music.notes)
            {
                /*
                 * 1. ロングノーツの場合、tailNote に 最後尾ノーツを代入。
                 *    そうでない場合は、tailNote に null を代入。
                 */
                var childNotesLength = note.notes.Length;

                var tailNote = childNotesLength > 0
                    ? new QueuedNote(note.notes[childNotesLength - 1], _music.BPM)
                    : null;

                /* （リストに追加） */
                if (tailNote != null)
                {
                    var lane = note.notes[childNotesLength - 1].block;
                    _queueNotes[lane].Add(tailNote);
                }

                /* 2. headNote に、先頭ノーツを代入（通常・フリックノーツ単体、またはロングノーツの始点） */
                var headNote = new QueuedNote(note, _music.BPM, tailNote);

                /* （リストに追加） */
                _queueNotes[note.block].Add(headNote);
            }

            for (var i = 0; i < _queueNotes.Length; i++)
                _queueNotes[i] = _queueNotes[i].OrderBy(item => item.timing).ToList();

            PreGenerateNotes();

            _currentTime = -2f;
            _hasStarted = true;
            isPose = false;
            Invoke(nameof(Play), 1.75f);
        }

        private void Update()
        {
            if (!isPose) _currentTime += Time.deltaTime;
            if (_hasStarted && !_hasPosted && _currentTime > _endTime + 3f) StartCoroutine(nameof(SceneMove));

            for (var i = 0; i < _queueNotes.Length; i++)
            {
                /* 1.25秒以内に判定ラインに到達させるべきノーツを抽出する */
                var queuedNotes = _queueNotes[i]
                    .Where(generateNote => generateNote.timing <= _currentTime + 1.25)
                    .ToList();

                _generatedNotes[i].AddRange(queuedNotes);

                foreach (var queuedNote in queuedNotes)
                {
                    _queueNotes[i].Remove(queuedNote);

                    switch (queuedNote.noteType)
                    {
                        /* 通常ノーツ */
                        case QueuedNote.NoteType.Normal:
                            GenerateNote(queuedNote);
                            break;

                        /* ロングノーツ */
                        case QueuedNote.NoteType.Long:
                            if (queuedNote.TailNote != null)
                            {
                                var notes = LongNotesGenerator.instance.Create(queuedNote.lane,
                                    queuedNote.TailNote.lane,
                                    queuedNote.timing,
                                    queuedNote.TailNote.timing);
                                queuedNote.TailNote.LinkGameObject(notes);
                            }

                            break;

                        /* フリックノーツ */
                        case QueuedNote.NoteType.Flick:
                            GenerateNote(queuedNote);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void Play()
        {
            isPose = false;
            View.instance.BgmAudioSource.Play();
        }

        private void Pause()
        {
            isPose = true;
            View.instance.BgmAudioSource.Pause();
        }

        private IEnumerator SceneMove()
        {
            _hasPosted = true;

            var fs = new FirestoreHandler();
            fs.OnErrorOccured += error =>
            {
                // TODO: エラーをユーザに伝える
                Debug.Log(error);
            };

            var user = _auth.GetUser();
            var totalScore = ScoreHandler.GetTotalScore();
            var iEnumerator = fs.AddScore(user, _musicName, totalScore, _level);
            yield return iEnumerator;


            Achieve achieve;
            var good = ScoreHandler.GetGood();
            var miss = ScoreHandler.GetMiss();

            if (totalScore == 1000000)
                achieve = Achieve.AllPerfect;
            else if (good == 0 && miss == 0)
                achieve = Achieve.FullCombo;
            else
                achieve = Achieve.Clear;

            iEnumerator = fs.AddAchieve(user, _musicName, _level, achieve);
            yield return iEnumerator;


            SceneManager.LoadScene("ResultScene");
        }

        /// <summary>
        /// 各レーンごとのGameObjectインスタンスをあらかじめ生成しておきます
        /// </summary>
        private void PreGenerateNotes()
        {
            for (var i = 0; i < LaneCount; i++)
            {
                var tmp = new List<GameObject>();
                for (var j = 0; j < PreGenerateNormalNotesPerLane; j++)
                    tmp.Add(Instantiate(normalNoteGameObject,
                        new Vector3(-0.9f + LaneWidth * i, 6.4f, -0.005f),
                        new Quaternion(0, 0, 0, 0)));

                _preGenerateNormalNotes.Add(tmp);
            }

            for (var i = 0; i < LaneCount; i++)
            {
                var tmp = new List<GameObject>();
                for (var j = 0; j < PreGenerateFlickNotesPerLane; j++)
                    tmp.Add(Instantiate(flickNoteGameObject,
                        new Vector3(-0.9f + LaneWidth * i, 6.4f, -0.005f),
                        new Quaternion(0, 0, 0, 0)));

                _preGenerateFlickNotes.Add(tmp);
            }
        }

        /// <summary>
        /// 生成済みのノーツから非アクティブのものを使用してノーツを生成します
        /// </summary>
        private void GenerateNote(QueuedNote queuedNote)
        {
            switch (queuedNote.noteType)
            {
                case QueuedNote.NoteType.Normal:
                {
                    for (var i = 0; i < PreGenerateNormalNotesPerLane; i++)
                        if (!_preGenerateNormalNotes[queuedNote.lane][i].activeSelf)
                        {
                            _preGenerateNormalNotes[queuedNote.lane][i].transform.position =
                                new Vector3(-0.9f + LaneWidth * queuedNote.lane, 6.4f, -0.005f);
                            _preGenerateNormalNotes[queuedNote.lane][i].SetActive(true);
                            queuedNote.LinkGameObject(
                                new List<GameObject> {_preGenerateNormalNotes[queuedNote.lane][i]});

                            return;
                        }

                    Debug.LogError("Normal: メモリ不足");

                    break;
                }
                case QueuedNote.NoteType.Flick:
                {
                    for (var i = 0; i < PreGenerateFlickNotesPerLane; i++)
                        if (!_preGenerateFlickNotes[queuedNote.lane][i].activeSelf)
                        {
                            _preGenerateFlickNotes[queuedNote.lane][i].transform.position =
                                new Vector3(-0.9f + LaneWidth * queuedNote.lane, 6.4f, -0.005f);
                            _preGenerateFlickNotes[queuedNote.lane][i].SetActive(true);
                            queuedNote.LinkGameObject(new List<GameObject>
                                {_preGenerateFlickNotes[queuedNote.lane][i]});

                            return;
                        }

                    Debug.LogError("Flick: メモリ不足");

                    break;
                }
                case QueuedNote.NoteType.Long:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(queuedNote.noteType), queuedNote.noteType, null);
            }
        }

        /// <summary>
        /// タッチした際の判定を行います
        /// </summary>
        public static bool JudgeTiming(int lane, QueuedNote.NoteType noteType, bool isAdjacentLane = false)
        {
            switch (noteType)
            {
                case QueuedNote.NoteType.Normal:
                {
                    var perfectNote = _generatedNotes[lane]
                        .Find(n => Math.Abs(n.timing - (_currentTime - 0.015f)) <= 0.06f && n.noteType == noteType);
                    if (perfectNote != null)
                    {
                        SoundManager.instance.PlayPerfect();
                        perfectNote.Destroy();
                        _generatedNotes[lane].Remove(perfectNote);
                        ScoreHandler.AddScore(ScoreHandler.Judge.Perfect);
                        return true;
                    }

                    var greatNote = _generatedNotes[lane]
                        .Find(n => Math.Abs(n.timing - (_currentTime - 0.015f)) <= 0.1f && n.noteType == noteType);
                    if (greatNote != null)
                    {
                        SoundManager.instance.PlayGreat();
                        greatNote.Destroy();
                        _generatedNotes[lane].Remove(greatNote);
                        ScoreHandler.AddScore(ScoreHandler.Judge.Great);
                        return true;
                    }

                    var goodNote = _generatedNotes[lane]
                        .Find(n => Math.Abs(n.timing - (_currentTime - 0.015f)) <= 0.12f && n.noteType == noteType);
                    if (goodNote != null)
                    {
                        SoundManager.instance.PlayGood();
                        goodNote.Destroy();
                        _generatedNotes[lane].Remove(goodNote);
                        ScoreHandler.AddScore(ScoreHandler.Judge.Good);
                        return true;
                    }

                    break;
                }
                case QueuedNote.NoteType.Long:
                {
                    var longNote = _generatedNotes[lane]
                        .Find(n => Math.Abs(n.timing - (_currentTime - 0.015f)) <= 0.12f && n.noteType == noteType);
                    if (longNote != null)
                    {
                        SoundManager.instance.PlayPerfect();
                        if (longNote.gameObjects != null)
                            foreach (var gameObject in longNote.gameObjects)
                                Destroy(gameObject);
                        _generatedNotes[lane].Remove(longNote);
                        ScoreHandler.AddScore(ScoreHandler.Judge.Perfect);
                        return true;
                    }

                    break;
                }
                case QueuedNote.NoteType.Flick:
                {
                    var flickNote = _generatedNotes[lane]
                        .Find(n => Math.Abs(n.timing - (_currentTime - 0.02f)) <= 0.12f && n.noteType == noteType);
                    if (flickNote != null)
                    {
                        SoundManager.instance.PlayFlick();
                        flickNote.Destroy();
                        _generatedNotes[lane].Remove(flickNote);
                        ScoreHandler.AddScore(ScoreHandler.Judge.Perfect);
                        return true;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(noteType), noteType, null);
            }

            /* 隣接レーンの確認 */
            if (isAdjacentLane) return false;
            if (lane - 1 >= 0) return JudgeTiming(lane - 1, noteType, true);
            if (lane + 1 <= 6) return JudgeTiming(lane + 1, noteType, true);

            return false;
        }
    }
}