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

    private async void Start()
    {
        // PlayerPrefs.DeleteAll(); //ユーザ情報を初期化したい場合にコメントアウトを解除
        ScreenResponsive();
        audioSource = GetComponent<AudioSource>();
        showVersion.text = "Ver." + ThisVersion;
        await Task.Delay(1000);
        _touchableFlag = true;
    }

    public void Update()
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
                    showConnecting.text = "";
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
            showConnecting.text = "";
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
                messageTitle.text = "NOTE";
                messageTitle.color = new Color(1f / 255f, 164f / 255f, 255f / 255f);
                messageText.text = "The latest version has been released.\nThis version will expire in " +
                                        day + " days.";
                messageButtonLabel.text = "Continue";
                messageButtonImage.sprite = messageButtonSpriteInfo;
                showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 1:
                messageTitle.text = "CAUTION !";
                messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                messageText.text = "Cannot connect to the API server.\nPlease check your network.";
                messageButtonImage.sprite = messageButtonSpriteError;
                messageButtonLabel.text = "Restart";
                showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 2:
                messageTitle.text = "CAUTION !";
                messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                messageText.text = "The device time is not set correctly.\nThe time zone is JST only.";
                messageButtonImage.sprite = messageButtonSpriteError;
                messageButtonLabel.text = "Restart";
                showConnecting.text = "";
                panel.SetActive(true);
                break;
            case 3:
                messageTitle.text = "CAUTION !";
                messageTitle.color = new Color(255f / 255f, 92f / 255f, 1f / 255f);
                messageText.text =
                    "Support for this version has ended.\nFor details, see our official website.";
                messageButtonImage.sprite = messageButtonSpriteError;
                messageButtonLabel.text = "Restart";
                showConnecting.text = "";
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
        if (inputUserName.text.Length >= 3 && inputUserName.text.Length <= 15)
        {
            showConnecting.text = "Connecting Server ...";
            WWWForm form = new WWWForm();
            form.AddField("name", inputUserName.text);
            UnityWebRequest www = UnityWebRequest.Post(RegisterApiUri, form);
            yield return www.SendWebRequest();
            switch (www.result)
            {
                case UnityWebRequest.Result.Success:
                    UserRegistCheck(www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    ShowDialog(1, 0);
                    _continueButtonFlag = true; //Continueボタンを再度有効化
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            registerText.text = "Please enter between 3 and 15 characters.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    private async void UserRegistCheck(string data)
    {
        registerResponse jsnData = JsonUtility.FromJson<registerResponse>(data);

        if (jsnData.success)
        {
            showConnecting.text = "";
            registerText.text = "";
            PlayerPrefs.SetString("name", jsnData.name);
            PlayerPrefs.SetString("jwt", jsnData.token);
            await Task.Delay(1000);
            ScreenTransition();
        }
        else if (!jsnData.success)
        {
            registerText.text = "This name is already in use.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    private IEnumerator RecoveryNetworkProcess()
    {
        if (inputUserName.text.Length == 8)
        {
            showConnecting.text = "Connecting Server ...";
            WWWForm form = new WWWForm();
            form.AddField("code", inputUserName.text);
            UnityWebRequest www = UnityWebRequest.Post(RecoveryApiUri, form);
            yield return www.SendWebRequest();
            switch (www.result)
            {
                case UnityWebRequest.Result.Success:
                    RecoveryCodeCheck(www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    ShowDialog(1, 0);
                    _continueButtonFlag = true; //Continueボタンを再度有効化
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            registerText.text = "Please enter 8 characters.";
            _continueButtonFlag = true; //Continueボタンを再度有効化
        }
    }

    private async void RecoveryCodeCheck(string data)
    {
        recoveryResponse jsnData = JsonUtility.FromJson<recoveryResponse>(data);

        if (jsnData.success)
        {
            showConnecting.text = "";
            registerText.text = "";
            PlayerPrefs.SetString("name", jsnData.name);
            PlayerPrefs.SetString("jwt", jsnData.token);
            await Task.Delay(1000);
            ScreenTransition();
        }
        else if (!jsnData.success)
        {
            showConnecting.text = "";
            registerText.text = "This code is invalid.";
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
                registerTitle.text = "ENTER  YOUR  NAME";
                registerText.text = "";
                registerPlaceholder.text = "Half-Width Alphanumeric Only...";
                registerBottom.text = "Do you have Recovery Code ?";
                showConnecting.text = "";
                registerPanel.SetActive(true);
                break;
            case 1:
                registerTitle.text = "ENTER  ISSUE  CODE";
                registerText.text = "";
                registerPlaceholder.text = "Your Code...";
                registerBottom.text = "↩ Back to Create Account";
                showConnecting.text = "";
                registerPanel.SetActive(true);
                break;
        }
    }
}