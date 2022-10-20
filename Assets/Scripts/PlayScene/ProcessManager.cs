using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlayScene;
using Project.Scripts.Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.PlayScene
{
    public class PlaySceneProcessManager : MonoBehaviour
    {
        // static List<List<Note>> _notes = new List<List<Note>>(); // 2次元リスト
        static float laneWidth = 0.3f; //レーンの太さ( = ノーツの太さ )
        float _offset = 1.5f * NotesFallUpdater.Speed;
        public static bool isPose { get; private set; } = true;
        static float musicTime;
        // [SerializeField] LongNotesGenerator lng;
        [SerializeField] AudioSource bgm;

        // private Music _notes = new();
        private static List<ConvertedNotes>[] _queueNotes;
        private static List<ConvertedNotes>[] _generatedNotes;

        private void Awake() => Application.targetFrameRate = 60;

        private void Start()
        {
            var jsonFile = Resources.Load("burning_heart_hard") as TextAsset;
            if (!jsonFile) throw new Exception("譜面データが無効です");
            var inputString = jsonFile.ToString();
            var music = JsonUtility.FromJson<Music>(inputString);

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

            musicTime = -2f;
            isPose = false;
            // LoadNotes(music);
            //Invoke("NotesStart", 1);
            //InvokeRepeating("Metro", 1, 60f / MusicData.BPM);
            Invoke(nameof(BGMStart), 2); // ノーツ再生から3秒待たなければならない
        }

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
                    .Where(generateNote => generateNote.timing <= musicTime + 0.95)
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
                            NoteGenerator(generateNote.block, 1);
                            // Instantiate(preNoteObject,
                            //     new Vector3(-0.9f + laneWidth * filteredGenerateNote.block,
                            //         // NotesFallUpdater.speed * generateNote.timing + _offset, -0.005f), // TODO
                            //         6.4f, -0.005f),
                            //     new Quaternion(0, 0, 0, 0));
                            break;

                        /* ロングノーツ */
                        case 2:
                            // if (generateNote.tailNote != null)
                            // {
                            //     lng.Create(generateNote.block, generateNote.tailNote.block,
                            //         generateNote.timing,
                            //         generateNote.tailNote.timing);
                            // }
                            break;

                        /* フリックノーツ */
                        case 3:
                            NoteGenerator(generateNote.block, 3);
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

        private void NoteGenerator(int lane, int type)
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
                        }
                        else
                        {
                            Debug.LogError("メモリ不足");
                        }

                    break;
                }
            }
        }

        // static void Metro()
        // {
        // }

        static void Success(int type)
        {
            // metro.Stop();
            //success.Stop();
            SoundManager.instance.PlaySuccess();
            // else SoundManager.instance.PlayFlick();
        }

        void Bad()
        {
            //metro.Stop();
            SoundManager.instance.PlayBad();
        }

        // void LoadNotes(MusicJson music)
        // {
        //     Object preNoteObject = Resources.Load("Notes");
        //     Object noteObjectF = Resources.Load("FrickNotes");
        //
        //     for (int i = 0; i < music.maxBlock; i++)
        //     {
        //         _notes.Add(new List<Note>());
        //     }
        //
        //     for (int i = 0; i < music.notes.Length; i++)
        //     {
        //         Note firstNote = new Note();
        //         Note nextNote = new Note();
        //         firstNote.lpb = music.notes[i].LPB;
        //         firstNote.num = music.notes[i].num;
        //         firstNote.block = music.notes[i].block;
        //         firstNote.type = music.notes[i].type;
        //         //Debug.Log(firstNote.num);
        //         float a = 60f * firstNote.num;
        //         float b = Music.bpm * firstNote.lpb;
        //         firstNote.timing = a / b;
        //         //Debug.Log(i + " " + a);
        //         //Debug.Log(i + " " + b);
        //         //Debug.Log(i + " " + firstNote.block + " " + firstNote.timing);
        //         firstNote.noteObjects = new List<GameObject>();
        //         _notes[firstNote.block].Add(firstNote);
        //         if (music.notes[i].notes.Length == 1)
        //         {
        //             nextNote.lpb = music.notes[i].notes[0].LPB;
        //             nextNote.num = music.notes[i].notes[0].num;
        //             nextNote.block = music.notes[i].notes[0].block;
        //             nextNote.type = music.notes[i].notes[0].type;
        //             a = 60f * nextNote.num;
        //             b = Music.bpm * nextNote.lpb;
        //             nextNote.timing = a / b;
        //             _notes[nextNote.block].Add(nextNote);
        //         }
        //
        //         if (firstNote.type == 1) // 生成は別のところで
        //         {
        //             firstNote.noteObjects.Add((GameObject) Instantiate(preNoteObject,
        //                 new Vector3(-0.9f + laneWidth * firstNote.block,
        //                     NotesFallUpdater.speed * firstNote.timing + _offset, -0.005f), new Quaternion(0, 0, 0, 0)));
        //         }
        //         else if (firstNote.type == 2)
        //         {
        //             firstNote.noteObjects = lng.Create(firstNote.block, nextNote.block, firstNote.timing, nextNote.timing);
        //         }
        //         else if (firstNote.type == 5)
        //         {
        //             firstNote.noteObjects.Add((GameObject) Instantiate(noteObjectF,
        //                 new Vector3(-0.9f + laneWidth * firstNote.block,
        //                     NotesFallUpdater.speed * firstNote.timing + _offset, -0.005f), new Quaternion(0, 0, 0, 0)));
        //         }
        //     }
        //
        //     for (int i = 0; i < _notes.Count; i++)
        //         _notes[i].OrderBy(item => item.timing);
        //
        //     //Debug.Log(_notes[0][0].block + _notes[0][0].timing);
        //     //Debug.Log(_notes[0][1].block + _notes[0][1].timing);
        // }

        void BGMStart()
        {
            bgm.Play();
            //isPose = false;
        }

        public static void JudgeTiming(int lineNum, int type)
        {
            // Debug.Log("LANE: " + lineNum);
            var note = _generatedNotes[lineNum].Find(n => Mathf.Abs(n.timing - musicTime) <= 0.5f && n.type == type);
            if (note != null)
            {
                //Debug.Log("OK!: " + musicTime + " " + lineNum);
                Success(note.type);
                // if (note.noteObjects != null && note.noteObjects[0] != null && note.type != 2) Destroy(note.noteObjects[0]);
                _generatedNotes[lineNum].Remove(note);
                return;
            }
            else
            {
                //Debug.Log("Failed.: " + musicTime + " " + lineNum);
                //Bad();
            }

            if (type == 5)
                JudgeTiming(lineNum, 2);

            if (type == 1)
                JudgeTiming(lineNum, 2);
            // 5のときは判定して見つからなければ2を考える
        }
    }
}