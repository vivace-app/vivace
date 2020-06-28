using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupScreenProcessManager : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip sound;
    public GameObject panel = null;
    public Text showVersion;
    public Text messageTitle;
    public Text messageText;
    public Image messageButtonImage;
    public Sprite messageButtonSpriteInfo;
    public Sprite messageButtonSpriteError;
    public Text messageButtonLabel;
    public Text showConnecting;
    bool _touchableFlag = true;
    bool _playableFlag = false;

    // ------------------------------------------------------------------------------------

    static readonly string thisVersion = EnvDataStore.thisVersion;
    static readonly string licenceApiUrl = EnvDataStore.licenceApiUrl;
    static readonly bool ignoreNetworkProcess = false; // Allow setting to true only on emulator.

    // ------------------------------------------------------------------------------------

    [Serializable]
    public class jsonResponse {
        public string currentVersion;
        public string currentTime;
        public List<versionList> supportedVersions;
    }

    [Serializable]
    public class versionList {
        public string version;
        public string expirationDate;
    }

    void Start () {
        audioSource = GetComponent<AudioSource> ();
        this.showVersion.text = "Ver." + thisVersion;
    }

    void Update () {
        if (_touchableFlag && Input.GetMouseButtonUp (0)) {
            _touchableFlag = false;
            audioSource.PlayOneShot (sound);
            StartCoroutine (NetworkProcess ());
        }

        if (_touchableFlag && Input.touchCount > 0) {
            Touch touch = Input.GetTouch (0);

            if (touch.phase == TouchPhase.Ended) {
                _touchableFlag = false;
                audioSource.PlayOneShot (sound);
                StartCoroutine (NetworkProcess ());
            }
        }
    }

    IEnumerator NetworkProcess () {
        if (ignoreNetworkProcess) {
            ScreenTransition ();
        } else {
            this.showConnecting.text = "Connecting Server ...";
            UnityWebRequest www = UnityWebRequest.Get (licenceApiUrl);
            yield return www.SendWebRequest ();
            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError ("ネットワークに接続できません．(" + www.error + ")");
                ShowDialog (1, 0);
            } else {
                JsonCheck (www.downloadHandler.text);
            }
        }
    }

    private void JsonCheck (string data) {
        jsonResponse jsnData = JsonUtility.FromJson<jsonResponse> (data);
        DateTime dtLocal = DateTime.Parse (DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss"));
        DateTime dtServer = DateTime.Parse (jsnData.currentTime);
        TimeSpan ts = dtLocal - dtServer;

        if (ts.Duration () > new TimeSpan (0, 5, 0)) {
            Debug.LogError ("現在時刻が正常に設定されていません");
            ShowDialog (2, 0);
        } else if (thisVersion != jsnData.currentVersion) {
            foreach (versionList x in jsnData.supportedVersions) {
                if (thisVersion == x.version) {
                    dtLocal = DateTime.Parse (DateTime.Now.ToString ("yyyy-MM-dd"));
                    dtServer = DateTime.Parse (x.expirationDate);
                    if (dtServer - dtLocal >= new TimeSpan (0, 0, 0)) {
                        Debug.LogError ("最新版が更新されています．このバージョンの有効期限はあと" + (dtServer - dtLocal).Days + "日です．");
                        _playableFlag = true;
                        ShowDialog (0, (dtServer - dtLocal).Days);
                        break;
                    } else {
                        Debug.LogError ("このバージョンのサポートは終了しています．");
                        ShowDialog (3, 0);
                        break;
                    }
                }
            }
        } else {
            this.showConnecting.text = "";
            ScreenTransition ();
        }
    }

    private async void ScreenTransition () {
        Debug.Log ("遊べるドン！");
        await Task.Delay (1000);
        // SceneManager.LoadScene ("GameScene");
    }

    public void ButtonTappedController () {
        if (_playableFlag) {
            ScreenTransition ();
        } else {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        }
    }

    private void ShowDialog (int arg, int day) {
        // Argument:
        //      0 -> Not the Latest Version
        //      1 -> Network Error
        //      2 -> Timezone Error
        //      3 -> End of Support

        switch (arg) {
            case 0:
                this.messageTitle.text = "NOTE";
                this.messageTitle.color = new Color (1f / 255f, 164f / 255f, 255f / 255f);
                this.messageText.text = "The latest version has been released.\nThis version will expire in " + day + " days.";
                this.messageButtonLabel.text = "Continue";
                this.messageButtonImage.sprite = messageButtonSpriteInfo;
                this.showConnecting.text = "";
                panel.SetActive (true);
                break;
            case 1:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color (255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text = "Cannot connect to the API server.\nPlease check your network.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive (true);
                break;
            case 2:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color (255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text = "The device time is not set correctly.\nThe time zone is JST only.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive (true);
                break;
            case 3:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color (255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text = "Support for this version has ended.\nFor details, see our official website.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive (true);
                break;
        }
    }
}