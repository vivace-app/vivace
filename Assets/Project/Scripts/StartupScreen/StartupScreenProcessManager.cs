using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Project.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupScreenProcessManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip sound;
    public GameObject panel = null;
    public Text showVersion;
    public Text messageTitle;
    public Text messageText;
    public Image messageButtonImage;
    public Sprite messageButtonSpriteInfo;
    public Sprite messageButtonSpriteError;
    public Text messageButtonLabel;
    public GameObject registerPanel = null;
    public RectTransform Background;
    public Text registerTitle;
    public Text registerText;
    public Text registerPlaceholder;
    public Text registerBottom;
    public Text showConnecting;
    public Text inputUserName;
    bool _touchableFlag = false;
    bool _playableFlag = false;
    bool _isRecoveryMode = false;
    bool _continueButtonFlag = true; //trueのとき，Continueボタンを使用可能

    // ------------------------------------------------------------------------------------

    private const string ThisVersion = EnvDataStore.ThisVersion;
    private const string LicenceApiUri = EnvDataStore.ApiUri + "/license";
    private const string RegisterApiUri = EnvDataStore.ApiUri + "/register";
    private const string RecoveryApiUri = EnvDataStore.ApiUri + "/recovery";
    static readonly bool ignoreNetworkProcess = false; // Allow setting to true only on emulator.

    // ------------------------------------------------------------------------------------

    [Serializable]
    public class licenseResponse
    {
        public bool success;
        public List<versionList> version;
    }

    [Serializable]
    public class versionList
    {
        public string version;
        public string expirationDate;
    }

    [Serializable]
    public class registerResponse
    {
        public bool success;
        public string msg;
        public string name;
        public string token;
    }

    [Serializable]
    public class recoveryResponse
    {
        public bool success;
        public string msg;
        public string name;
        public string token;
    }

    async void Start()
    {
        // PlayerPrefs.DeleteAll(); //ユーザ情報を初期化したい場合にコメントアウトを解除
        ScreenResponsive();
        audioSource = GetComponent<AudioSource>();
        this.showVersion.text = "Ver." + ThisVersion;
        await Task.Delay(1000);
        _touchableFlag = true;
    }

    void Update()
    {
        if (_touchableFlag && Input.GetMouseButtonUp(0))
        {
            _touchableFlag = false;
            audioSource.PlayOneShot(sound);
            StartCoroutine(NetworkProcess());
        }

        if (_touchableFlag && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                _touchableFlag = false;
                audioSource.PlayOneShot(sound);
                StartCoroutine(NetworkProcess());
            }
        }
    }

    private void ScreenResponsive()
    {
        float scale = 1f;
        if (Screen.width < 1920)
            scale = 1.5f;
        if (Screen.width < Screen.height)
            scale = (Screen.height * 16) / (Screen.width * 9);
        Background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    private IEnumerator NetworkProcess()
    {
        if (ignoreNetworkProcess) UserCheck();
        else
        {
            showConnecting.text = "Connecting Server ...";
            var request = UnityWebRequest.Get(LicenceApiUri);
            yield return request.SendWebRequest();
            
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("リクエスト成功");
                    LicenseCheck(request.downloadHandler.text);
                    break;

                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log("リクエスト失敗");
                    ShowDialog(1, 0);
                    break;

                case UnityWebRequest.Result.InProgress:
                    Debug.Log("リクエスト中");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void LicenseCheck(string data)
    {
        DateTime dtLocal = new DateTime();
        DateTime dtServer = new DateTime();
        string latestVersion = String.Empty;

        licenseResponse jsnData = JsonUtility.FromJson<licenseResponse>(data);

        if (jsnData.success)
        {
            foreach (versionList x in jsnData.version)
                latestVersion = x.version;

            foreach (versionList x in jsnData.version)
            {
                if (ThisVersion == latestVersion)
                {
                    this.showConnecting.text = "";
                    UserCheck();
                    break;
                }
                else if (ThisVersion == x.version)
                {
                    dtLocal = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                    dtServer = DateTime.Parse(x.expirationDate);
                    if (dtServer - dtLocal < new TimeSpan(0, 0, 0))
                    {
                        ShowDialog(3, 0);
                        break;
                    }
                    else
                    {
                        _playableFlag = true;
                        ShowDialog(0, (dtServer - dtLocal).Days);
                        break;
                    }
                }
            }
        }
        else
        {
            this.showConnecting.text = "";
            ShowDialog(1, 0);
        }
    }

    public async void ScreenTransition()
    {
        await Task.Delay(1000);
        SceneManager.LoadScene("DownloadScene");
    }

    public void ButtonTappedController()
    {
        if (_playableFlag)
            UserCheck();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UserCheck()
    {
        if (PlayerPrefs.HasKey("name") && PlayerPrefs.HasKey("jwt"))
            ScreenTransition();
        else
            ShowRegisterDialog(0);
    }

    private void ShowDialog(int arg, int day)
    {
        // Argument:
        //      0 -> Not the Latest Version
        //      1 -> Network Error
        //      2 -> Timezone Error
        //      3 -> End of Support

        switch (arg)
        {
            case 0:
                this.messageTitle.text = "NOTE";
                this.messageTitle.color = new Color(1f / 255f, 164f / 255f, 255f / 255f);
                this.messageText.text = "The latest version has been released.\nThis version will expire in " +
                                        day + " days.";
                this.messageButtonLabel.text = "Continue";
                this.messageButtonImage.sprite = messageButtonSpriteInfo;
                this.showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 1:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text = "Cannot connect to the API server.\nPlease check your network.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 2:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text = "The device time is not set correctly.\nThe time zone is JST only.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 3:
                this.messageTitle.text = "CAUTION !";
                this.messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                this.messageText.text =
                    "Support for this version has ended.\nFor details, see our official website.";
                this.messageButtonImage.sprite = messageButtonSpriteError;
                this.messageButtonLabel.text = "Restart";
                this.showConnecting.text = "";
                panel.SetActive(true);
                break;
        }
    }

    public void ContinueButtonTappedController()
    {
        if (_continueButtonFlag)
        {
            _continueButtonFlag = false; //Continueボタンの連打を禁止
            if (_isRecoveryMode)
                StartCoroutine(RecoveryNetworkProcess());
            else
                StartCoroutine(RegisterNetworkProcess());
        }
    }

    public void BottomButtonTappedController()
    {
        if (_isRecoveryMode)
        {
            _isRecoveryMode = false;
            ShowRegisterDialog(0);
        }
        else
        {
            _isRecoveryMode = true;
            ShowRegisterDialog(1);
        }
    }

    IEnumerator RegisterNetworkProcess()
    {
        if (this.inputUserName.text.Length >= 3 && this.inputUserName.text.Length <= 15)
        {
            this.showConnecting.text = "Connecting Server ...";
            WWWForm form = new WWWForm();
            form.AddField("name", this.inputUserName.text);
            UnityWebRequest www = UnityWebRequest.Post(RegisterApiUri, form);
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                ShowDialog(1, 0);
                _continueButtonFlag = true; //Continueボタンを再度有効化
            }
            else
            {
                UserRegistCheck(www.downloadHandler.text);
            }
        }
        else
        {
            this.registerText.text = "Please enter between 3 and 15 characters.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    async private void UserRegistCheck(string data)
    {
        registerResponse jsnData = JsonUtility.FromJson<registerResponse>(data);

        if (jsnData.success)
        {
            this.showConnecting.text = "";
            this.registerText.text = "";
            PlayerPrefs.SetString("name", jsnData.name);
            PlayerPrefs.SetString("jwt", jsnData.token);
            await Task.Delay(1000);
            ScreenTransition();
        }
        else if (!jsnData.success)
        {
            this.registerText.text = "This name is already in use.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    IEnumerator RecoveryNetworkProcess()
    {
        if (this.inputUserName.text.Length == 8)
        {
            this.showConnecting.text = "Connecting Server ...";
            WWWForm form = new WWWForm();
            form.AddField("code", this.inputUserName.text);
            UnityWebRequest www = UnityWebRequest.Post(RecoveryApiUri, form);
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                ShowDialog(1, 0);
                _continueButtonFlag = true; //Continueボタンを再度有効化
            }
            else
            {
                RecoveryCodeCheck(www.downloadHandler.text);
            }
        }
        else
        {
            this.registerText.text = "Please enter 8 characters.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    async private void RecoveryCodeCheck(string data)
    {
        recoveryResponse jsnData = JsonUtility.FromJson<recoveryResponse>(data);

        if (jsnData.success)
        {
            this.showConnecting.text = "";
            this.registerText.text = "";
            PlayerPrefs.SetString("name", jsnData.name);
            PlayerPrefs.SetString("jwt", jsnData.token);
            await Task.Delay(1000);
            ScreenTransition();
        }
        else if (!jsnData.success)
        {
            this.showConnecting.text = "";
            this.registerText.text = "This code is invalid.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    private void ShowRegisterDialog(int arg)
    {
        // Argument:
        //      0 -> Create
        //      1 -> Restore

        switch (arg)
        {
            case 0:
                this.registerTitle.text = "ENTER  YOUR  NAME";
                this.registerText.text = "";
                this.registerPlaceholder.text = "Half-Width Alphanumeric Only...";
                this.registerBottom.text = "Do you have Recovery Code ?";
                this.showConnecting.text = "";
                registerPanel.SetActive(true);
                break;
            case 1:
                this.registerTitle.text = "ENTER  ISSUE  CODE";
                this.registerText.text = "";
                this.registerPlaceholder.text = "Your Code...";
                this.registerBottom.text = "↩ Back to Create Account";
                this.showConnecting.text = "";
                registerPanel.SetActive(true);
                break;
        }
    }
}