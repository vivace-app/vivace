using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System;
using System.IO;
// using System.Text;

public class PlayScreenProcessManager : MonoBehaviour {
    // public string csvFilePass = "Assets/CSV/burningHeart.csv";
    public GameObject[] Note;
    private AudioSource _audioSource;

    private int notesTotal = 0;
    private int _notesCount = 0;
    private float _startTime = 0;
    public float[] _timing;
    public int[] _lineNum;

    // ---------------------
    private int timeOffset = 0;
    // ---------------------

    async void Start () {
        _timing = new float[1024];
        _lineNum = new int[1024];
        LoadCSV ();
        _audioSource = GameObject.Find ("Music").GetComponent<AudioSource> ();
        _startTime = Time.time;
        await Task.Delay (1950);
        _audioSource.Play ();
    }

    void Update () {
        CheckNextNotes ();
    }

    void LoadCSV () {
        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset> ("Assets/Project/CSV/burningHeart.csv") as TextAsset; //ファイル読み込み
        StringReader reader = new StringReader (csv.text);
        while (reader.Peek () > -1) {
            string line = reader.ReadLine ();
            string[] values = line.Split (',');
            _timing[notesTotal] = float.Parse (values[0]);
            _lineNum[notesTotal++] = int.Parse (values[1]);
        }
    }

    void CheckNextNotes () {
        while (_timing[_notesCount] + timeOffset < (Time.time - _startTime) && _timing[_notesCount] != 0) {
            SpawnNotes (_lineNum[_notesCount]);
            _notesCount++;
        }
    }

    void SpawnNotes (int num) {
        Instantiate (Note[num],
            // new Vector3 (-0.7f + (0.35f * num), 8.4f, 4.5f),
            new Vector3 (-0.676f + (0.338f * num), 8.4f, 4.5f),
            Quaternion.Euler (-30f, 0, 0));
    }
}