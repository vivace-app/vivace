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
    private float _stoptime = 0;
    public float[] _timing;
    public int[] _lineNum;
    private static AudioSource[] _SoundEffects;
    public static bool _isPlaying = true;
    public static bool _autoPlay = false; //自動プレイ用

    // -- Temporary Variable. -------------------------------------------------------------
    private string csvFilePass = "CSV/burningHeart";
    // ------------------------------------------------------------------------------------

    async void Start()
    {
        _timing = new float[1024];
        _lineNum = new int[1024];
        await Task.Delay(1000);
        LoadCSV();
        _audioSource = GameObject.Find("Music").GetComponent<AudioSource>();
        _SoundEffects = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
        _startTime = Time.time;
        await Task.Delay(2550);
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

    public static void SoundEffect(int num)
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
            _stoptime = Time.time;
            _audioSource.Pause();
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
            _startTime = _startTime + (Time.time - _stoptime);
            _audioSource.UnPause();
        }
    }

    public static void PerfectTimingFunc(int num)
    {
        // Debug.Log ("Line:" + num + " Perfect!"); //ログ出力
        // Debug.Log (GetMusicTime ()); //ログ出力
        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        SoundEffect(0); //Perfectサウンド（引数0）を再生
        //_combo++; //コンボ数を1加算
        //AddScore(1); //スコア加算(倍率はPerfectなので1)
    }

    public static void GreatTimingFunc(int num)
    {
        SoundEffect(1); //Greatサウンド再生
        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        //_combo++; //コンボ数を1加算
        //AddScore(0.75f); //スコア加算(倍率はGreatなので0.75)
    }
}