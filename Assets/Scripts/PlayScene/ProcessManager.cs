using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Model;
using Tools.AssetBundle;
using Tools.PlayStatus;
using UnityEngine;

namespace PlayScene
{
    public class ProcessManager : MonoBehaviour
    {
        // static List<List<Note>> _notes = new List<List<Note>>(); // 2次元リスト
        static float laneWidth = 0.3f; //レーンの太さ( = ノーツの太さ )
        float _offset = 1.5f * NotesFallUpdater.Speed;
        public static bool isPose { get; private set; } = true;

        static float musicTime;

        private static Music music;

        // [SerializeField] LongNotesGenerator lng;
        [SerializeField] AudioSource bgm;

        // private Music _notes = new();
        private static List<ConvertedNotes>[] _queueNotes;
        private static List<ConvertedNotes>[] _generatedNotes;

        private void Awake() => Application.targetFrameRate = 60;

        private void Start()
        {
            Debug.Log(PlayStatusHandler.GetSelectedLevel());
            Debug.Log(PlayStatusHandler.GetSelectedMusic());

            var index = PlayStatusHandler.GetSelectedMusic();
            var level = PlayStatusHandler.GetSelectedLevel();
            var assetBundle = AssetBundleHandler.GetAssetBundle(index);
            var musicName = assetBundle.name;
            View.Instance.BgmAudioClip = assetBundle.LoadAsset<AudioClip>(musicName);

            // Debug.Log(musicName + "_" + level.ToString().ToLower());
            // var jsonFile = assetBundle.LoadAsset<TextAsset>(musicName + "_" + level.ToString().ToLower());
            var jsonFile = Resources.Load("maiden_voyage") as TextAsset;
            if (!jsonFile) throw new Exception("譜面データが無効です");
            var inputString = jsonFile.ToString();
            music = JsonUtility.FromJson<Music>(inputString);

            /*
             * _queueNotes[0] には、0番目のレーンのノーツ生成情報が リスト（ConvertedNotes）型 で格納されている。
             * _queueNotes[0] 〜 _queueNotes[(レーン数)] まで存在する。
             */
            _queueNotes = new List<ConvertedNotes>[music.maxBlock];
            for (var i = 0; i < _queueNotes.Length; i++) _queueNotes[i] = new List<ConvertedNotes>();
            _generatedNotes = new List<ConvertedNotes>[music.maxBlock];
            for (var i = 0; i < _generatedNotes.Length; i++) _generatedNotes[i] = new List<ConvertedNotes>();

            for (var i = 0; i < music.notes.Length; i++)
            {
                /* ==================================================== */

                /* 通常・フリックノーツ単体、またはロングノーツの始点 */
                /* ReSharper disable once JoinDeclarationAndInitializer */
                ConvertedNotes headNote;

                /* ロングノーツの場合の、最後尾ノーツ */
                /* ReSharper disable once JoinDeclarationAndInitializer */
                ConvertedNotes tailNote;

                /* ==================================================== */


                /*
                 * 1. ロングノーツの場合、tailNote に 最後尾ノーツを代入。
                 *    そうでない場合は、tailNote に null を代入。
                 */
                var childNotesLength = music.notes[i].notes.Length;
                tailNote = childNotesLength > 0
                    ? new ConvertedNotes(music.notes[i].notes[childNotesLength - 1], music.BPM)
                    : null;

                /* （リストに追加） */
                if (tailNote != null)
                {
                    var tailLaneNumber = music.notes[i].notes[childNotesLength - 1].block;
                    _queueNotes[tailLaneNumber].Add(tailNote);
                }


                /* 2. headNote に、先頭ノーツを代入 */
                headNote = new ConvertedNotes(music.notes[i], music.BPM, tailNote);

                /* （リストに追加） */
                var laneNumber = music.notes[i].block;
                _queueNotes[laneNumber].Add(headNote);
            }

            for (var i = 0; i < _queueNotes.Length; i++)
                _queueNotes[i] = _queueNotes[i].OrderBy(item => item.timing).ToList();

            NoteObjectInitializer();
            BaseScoreCalculation();

            musicTime = -2f;
            isPose = false;
            // LoadNotes(music);
            //Invoke("NotesStart", 1);
            //InvokeRepeating("Metro", 1, 60f / MusicData.BPM);
            Invoke(nameof(BGMStart), 2); // ノーツ再生から3秒待たなければならない
        }

        private void BGMStart() => View.Instance.BgmAudioSource.Play();

        [SerializeField] private GameObject noteObject;
        [SerializeField] private GameObject preNoteObject;
        [SerializeField] private GameObject noteObjectF;

        private void Update()
        {
            if (!isPose)
                musicTime += Time.deltaTime;

            // foreach (var generateNote in _notes)
            // {
            //     var filteredConvertedNotes = generateNote
            //     .Where(filteredGenerateNote => filteredGenerateNote.timing <= musicTime)
            //     .ToList();
            //     
            //     foreach (var filteredGenerateNote in filteredConvertedNotes.Where(filteredGenerateNote =>
            //                  !filteredGenerateNote.isGenerated))
            //     {
            //         SoundManager.instance.PlaySuccess();
            //         filteredGenerateNote.setIsGenerated();
            //     }
            // }

            for (var i = 0; i < _queueNotes.Length; i++)
                // foreach (var queueNote in _queueNotes)
            {
                /* ${2} 秒以内に判定ラインに到達させるべきノーツを抽出する */
                // TODO: LinQ -> for / foreach に変更
                var generateNotes = _queueNotes[i]
                    .Where(generateNote => generateNote.timing <= musicTime + 1.25)
                    .ToList();

                // _generatedNotes[i] = _generatedNotes[i].Union(generateNotes).ToList(); 間違い
                _generatedNotes[i].AddRange(generateNotes);

                foreach (var generateNote in generateNotes) _queueNotes[i].Remove(generateNote);

                // foreach (var generateNote in generateNotes.Where(filteredGenerateNote =>
                //              !filteredGenerateNote.isGenerated))
                foreach (var generateNote in generateNotes)
                {
                    switch (generateNote.type)
                    {
                        /* 通常ノーツ */
                        case 1:
                            NoteGenerator(generateNote.block, 1, generateNote);
                            // Instantiate(preNoteObject,
                            //     new Vector3(-0.9f + laneWidth * filteredGenerateNote.block,
                            //         // NotesFallUpdater.speed * generateNote.timing + _offset, -0.005f), // TODO
                            //         6.4f, -0.005f),
                            //     new Quaternion(0, 0, 0, 0));
                            break;

                        /* ロングノーツ */
                        case 2:
                            if (generateNote.tailNote != null)
                            {
                                LongNotesGenerator.instance.Create(generateNote.block, generateNote.tailNote.block,
                                    generateNote.timing,
                                    generateNote.tailNote.timing);
                            }

                            break;

                        /* フリックノーツ */
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            NoteGenerator(generateNote.block, 3, generateNote);
                            // Instantiate(noteObjectF,
                            //     new Vector3(-0.9f + laneWidth * generateNote.block,
                            //         // NotesFallUpdater.speed * generateNote.timing + _offset, -0.005f), // TODO
                            //         generateNote.timing + 2f * 6f, -0.005f),
                            //     new Quaternion(0, 0, 0, 0));
                            break;
                    }
                }
            }
        }

        private List<List<GameObject>> _normalNotesGameObjects = new();

        private List<List<GameObject>> _flickNotesGameObjects = new();
        // private GameObject[][] _flickNotesGameObjects;

        private const int PreGenerateNormalNotesPerLane = 6;
        private const int PreGenerateFlickNotesPerLane = 4;

        private void NoteObjectInitializer()
        {
            for (var i = 0; i < 7; i++)
            {
                var tmp = new List<GameObject>();
                for (var j = 0; j < PreGenerateNormalNotesPerLane; j++)
                {
                    tmp.Add(Instantiate(preNoteObject,
                        new Vector3(-0.9f + laneWidth * i, 6.4f, -0.005f),
                        new Quaternion(0, 0, 0, 0)));
                }

                _normalNotesGameObjects.Add(tmp);
            }

            Debug.Log(_normalNotesGameObjects);

            for (var i = 0; i < 7; i++)
            {
                var tmp = new List<GameObject>();
                for (var j = 0; j < PreGenerateFlickNotesPerLane; j++)
                {
                    tmp.Add(Instantiate(noteObjectF,
                        new Vector3(-0.9f + laneWidth * i, 6.4f, -0.005f),
                        new Quaternion(0, 0, 0, 0)));
                }

                _flickNotesGameObjects.Add(tmp);
            }

            Debug.Log(_flickNotesGameObjects);

            // for (var i = 0; i < 5; i++)
            // for (var j = 0; j < 2; j++)
            //     _flickNotesGameObjects[i][j] = Instantiate(noteObjectF,
            //         new Vector3(-0.9f + laneWidth * i, 6.4f, -0.005f),
            //         new Quaternion(0, 0, 0, 0));
        }

        private void NoteGenerator(int lane, int type, ConvertedNotes convertedNote)
        {
            switch (type)
            {
                case 1:
                {
                    for (var i = 0; i < PreGenerateNormalNotesPerLane; i++)
                        if (!_normalNotesGameObjects[lane][i].activeSelf)
                        {
                            _normalNotesGameObjects[lane][i].transform.position =
                                new Vector3(-0.9f + laneWidth * lane, 6.4f, -0.005f);
                            _normalNotesGameObjects[lane][i].SetActive(true);
                            convertedNote.LinkGameObject(_normalNotesGameObjects[lane][i]);

                            return;
                        }

                    Debug.LogError("メモリ不足");

                    break;
                }
                case 3:
                {
                    for (var i = 0; i < PreGenerateFlickNotesPerLane; i++)
                        if (!_flickNotesGameObjects[lane][i].activeSelf)
                        {
                            _flickNotesGameObjects[lane][i].transform.position =
                                new Vector3(-0.9f + laneWidth * lane, 6.4f, -0.005f);
                            _flickNotesGameObjects[lane][i].SetActive(true);
                            convertedNote.LinkGameObject(_flickNotesGameObjects[lane][i]);
                        }
                        else
                        {
                            Debug.LogError("メモリ不足");
                        }

                    break;
                }
            }
        }

        public static void JudgeTiming(int lineNum, int type, bool second = false)
        {
            if (type == 3)
            {
                var fnote = _generatedNotes[lineNum]
                    .Find(n => Mathf.Abs(n.timing - (musicTime - 0.015f)) <= 0.12f && n.type == type);
                if (fnote != null)
                {
                    SoundManager.instance.PlayFlick();
                    fnote.Destroy();
                    _generatedNotes[lineNum].Remove(fnote);
                    AddScore(1);
                    return;
                }
            }
            else if (type == 2)
            {
                var lnote = _generatedNotes[lineNum]
                    .Find(n => Mathf.Abs(n.timing - (musicTime - 0.015f)) <= 0.12f && n.type == type);
                if (lnote != null)
                {
                    SoundManager.instance.PlayPerfect();
                    lnote.Destroy();
                    _generatedNotes[lineNum].Remove(lnote);
                    AddScore(1);
                    return;
                }
            }
            else
            {
                var note1 = _generatedNotes[lineNum]
                    .Find(n => Mathf.Abs(n.timing - (musicTime - 0.015f)) <= 0.07f && n.type == type);
                if (note1 != null)
                {
                    SoundManager.instance.PlayPerfect();
                    note1.Destroy();
                    _generatedNotes[lineNum].Remove(note1);
                    AddScore(0);
                    return;
                }

                var note2 = _generatedNotes[lineNum]
                    .Find(n => Mathf.Abs(n.timing - (musicTime - 0.015f)) <= 0.12f && n.type == type);
                if (note2 != null)
                {
                    SoundManager.instance.PlayGreat();
                    note2.Destroy();
                    _generatedNotes[lineNum].Remove(note2);
                    AddScore(1);
                    return;
                }

                var note3 = _generatedNotes[lineNum]
                    .Find(n => Mathf.Abs(n.timing - (musicTime - 0.015f)) <= 0.15f && n.type == type);
                if (note3 == null) return;
                SoundManager.instance.PlayGood();
                note3.Destroy();
                _generatedNotes[lineNum].Remove(note3);
                AddScore(2);
            }

            if (second) return;
            if (lineNum - 1 >= 0) JudgeTiming(lineNum - 1, type, true);
            if (lineNum + 1 <= 6) JudgeTiming(lineNum + 1, type, true);
        }

        private static int _combo;
        private static float _score;
        private static int _perfect;
        private static int _great;
        private static int _good;
        private static int _miss;
        private static float _baseScore;
        private static float[] _logSq;

        private const int SepPoint = 50;


        /// <summary>
        /// 区分求積法によるスコア計算の値を用意しておきます．
        /// </summary>
        private void BaseScoreCalculation()
        {
            _logSq = new float[SepPoint];

            var logSqSum = 0f;

            var denominator = music.notes.Length >= SepPoint ? SepPoint : music.notes.Length;

            for (var i = 0; i < denominator; i++)
            {
                _logSq[i] = (float) Math.Log10(1 + (9 * ((float) i + 1) / denominator));
                logSqSum += _logSq[i];
            }

            _baseScore = 1000000 / (logSqSum + music.notes.Length - denominator);
        }

        /// <summary>
        /// スコアを加算します．
        /// </summary>
        public static async void AddScore(int num) // 0：Perfect，1：Great，2：Good，3：Miss
        {
            float magni = 0, scoreTemp = 0;
            // float jTextTemp = ++_jTextCounter;
            var vl = new Vector3(1.5f, 1.5f, 1.5f);
            var vo = Vector3.one;

            // アニメーションの初期化
            // _jTextFade?.Kill();
            // _jTextReduce?.Kill();
            // _aTextFade?.Kill();
            // _aTextReduce?.Kill();

            switch (num)
            {
                case 0:
                    magni = 1;
                    _combo++;
                    _perfect++;
                    // judgeText.transform.localScale = vl;
                    // _jTextReduce = judgeText.transform.DOScale(vo, 0.2f);
                    // judgeText.color = _perfectC;
                    // judgeText.text = "Perfect!";
                    break;
                case 1:
                    magni = 0.75f;
                    _combo++;
                    _great++;
                    // judgeText.transform.localScale = vl;
                    // _jTextReduce = judgeText.transform.DOScale(vo, 0.2f);
                    // judgeText.color = _greatC;
                    // judgeText.text = "Great!";
                    break;
                case 2:
                    magni = 0.25f;
                    _combo = 0;
                    _good++;
                    // judgeText.transform.localScale = vo;
                    // judgeText.color = _goodC;
                    // judgeText.text = "Good!";
                    break;
                case 3:
                    magni = 0;
                    _combo = 0;
                    _miss++;
                    // judgeText.transform.localScale = vo;
                    // judgeText.color = _goodC;
                    // judgeText.text = "Good!";
                    break;
            }

            // コンボ数を更新
            View.Instance.ComboText = _combo;

            // 区分求積法による加算スコアの計算
            scoreTemp = (_combo <= SepPoint) switch
            {
                true when _combo > 0 => _baseScore * _logSq[_combo - 1] * magni,
                true => _baseScore * _logSq[0] * magni,
                _ => _baseScore * magni
            };

            if (_perfect == music.notes.Length) scoreTemp = 1000000 - _score; // floatへの変更でAP時に100万点にならないので強制代入

            // 加算スコア表示の文字色を変更
            // addText.color = _scoreC;
            // 加算スコア表示の値を更新
            // addText.text = "+" + ((int) Math.Round(scoreTemp, 0, MidpointRounding.AwayFromZero)).ToString("D");
            // アニメーション
            // addText.transform.localScale = vl;
            // _aTextReduce = addText.transform.DOScale(vo, 0.2f);

            // 軽量化設定によってはスコアのパラパラ表示を省略
            // if (!_isLowGraphicsMode)
            // {
            //     for (var i = 0; i < 15; i++)
            //     {
            //         _score += scoreTemp / 15;
            //         scoreText.text = ((int) Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
            //         await Task.Delay(33);
            //     }
            // }
            // else
            // {
            _score += scoreTemp;
            View.Instance.ScoreText = _score;
            // }

            //  ↓なんで!=じゃないんだっけ
            // if (_jTextCounter == jTextTemp) return;

            // 250ms経過しても次のAddScoreが発生していなければフェードアウト処理
            // await Task.Delay(250);
            // if (_jTextCounter != jTextTemp) return;
            // _jTextFade = DOTween.ToAlpha(() => judgeText.color, cchanger => judgeText.color = cchanger, 0.0f, 0.2f);
            // _aTextFade = DOTween.ToAlpha(() => addText.color, cchanger => addText.color = cchanger, 0.0f, 0.2f);
        }
    }
}