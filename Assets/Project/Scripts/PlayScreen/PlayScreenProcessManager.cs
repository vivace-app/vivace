using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class PlayScreenProcessManager : MonoBehaviour
{
    private AudioSource _audioSource;
    public GameObject[] Note;
    private int _notesTotal = 0;
    private int _notesCount = 0;
    private float _startTime = 0;
    public float[] _timing;
    public int[] _lineNum;
    private AudioSource[] _SoundEffects;
    public static bool _isPlaying = true;
    //public GameObject PauseButton;

    // -- Temporary Variable. -------------------------------------------------------------
    private string csvFilePass = "CSV/burningHeart";
    // ------------------------------------------------------------------------------------

    async void Start()
    {
        _timing = new float[1024];
        _lineNum = new int[1024];
        LoadCSV();
        _audioSource = GameObject.Find("Music").GetComponent<AudioSource>();
        _SoundEffects = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
        _startTime = Time.time;
        await Task.Delay(1950);
        _audioSource.Play();
    }

    void Update()
    {
        if (_isPlaying == true) CheckNextNotes();
    }

    void LoadCSV()
    {
        TextAsset csv = Resources.Load(csvFilePass) as TextAsset;
        StringReader reader = new StringReader(csv.text);
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            _timing[_notesTotal] = float.Parse(values[0]);
            _lineNum[_notesTotal++] = int.Parse(values[1]);
        }
    }

    void CheckNextNotes()
    {
        while (_timing[_notesCount] < (Time.time - _startTime) && _timing[_notesCount] != 0) SpawnNotes(_lineNum[_notesCount++]);
    }

    void SpawnNotes(int num)
    {
        Instantiate(Note[num], new Vector3(-0.676f + (0.338f * num), 8.4f, 4.5f), Quaternion.Euler(-30f, 0, 0));
    }

    public void SoundEffect(int num)
    {
        _SoundEffects[num].PlayOneShot(_SoundEffects[num].clip);
        //Debug.Log ("SoundEffect Played.");
    }

    public async void Pause()
    {
        int i = 0;
        if (_isPlaying == true)
        {
            Debug.Log("止まるドン！");
            _isPlaying = false;
        }
        else if (_isPlaying == false)
        {
            Debug.Log("さぁ，再開するドン！");
            for (i = 3; i > 0; i--)
            {
                await Task.Delay(1000);
                Debug.Log(i);
                SoundEffect(2);
            }
            await Task.Delay(1000);
            _isPlaying = true;
        }
    }
}