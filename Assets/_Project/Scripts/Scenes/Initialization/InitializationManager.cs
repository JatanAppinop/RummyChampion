using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Appinop;
using DG.Tweening;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class InitializationManager : MonoBehaviour
{
    [Header("Editor Testing")]
    [SerializeField] InitializationView loadingView;

    [SerializeField] GameObject AppUpdateView;
    [SerializeField] private TextMeshProUGUI versionLabel;


    private bool isConnected = false;
    private bool isServerLive = false;

    private NoInternetView noInternetView;
    private PopupControls noServerView;

    private string newAppVersion;


    private void Awake()
    {
        versionLabel.SetText("Version : " + Application.version);
        AppUpdateView.SetActive(false);


    }
    private async void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 120;

        loadingView.SetStatus(20, "Connecting");
        AudioManager.Instance.Initialize();

        isConnected = await InternetConnectivityChecker.CheckInternetAsync();

        if (!isConnected)
        {
            ShowNoInternetPopup();
            return;
        }

        APIServices.Instance.Initialize(Appinop.Constants.ServerURL, SecurePlayerPrefs.GetString(Appinop.Constants.KUserToken), Appinop.Constants.SocketURL);
        ServerReachablity serverReachablity = await CheckServerReachablity();
        isServerLive = serverReachablity.success;


        if (!isServerLive || serverReachablity.data.maintenance)
        {
            ShowServerUnavailableAlert();
            return;
        }

        //Check for New Version Update



        bool requireUpdate = false;

#if UNITY_ANDROID

        requireUpdate = Extentions.CheckNewVersion(Application.version, serverReachablity.data.appVersion);
#endif

#if UNITY_IOS
        //Check for iOS app Update
        requireUpdate = false;
#endif

        if (requireUpdate)
        {
            ShowUpdateView();
            newAppVersion = serverReachablity.data.appVersion;
            return;
        }


        string updatePath = SecurePlayerPrefs.GetString(Appinop.Constants.KUpdatePath);

        if (File.Exists(updatePath))
        {
            Debug.Log("File exists at path: " + updatePath);

            // Perform delete operation
            File.Delete(updatePath);
            Debug.Log("File deleted: " + updatePath);
        }


        if (isConnected && isServerLive && !serverReachablity.data.maintenance)
        {
            StartInitialization();
        }

    }


    private async Task<ServerReachablity> CheckServerReachablity()
    {
        var responce = await APIServices.Instance.GetAsync<ServerReachablity>("", includeAuthorization: false);

        if (responce != null)
        {
            return responce;
        }

        return default;
    }

    private async void StartInitialization()
    {
        loadingView.SetStatus(30, "Fetching");

        loadingView.SetStatus(50, "Validating");

        bool validatedUser = await ValidateUser();

        if (validatedUser)
        {
            loadingView.SetStatus(80, "Loading");

            await UserDataContext.Instance.Initialize();

            // Banned User
            if (!UserDataContext.Instance.UserData.status.ToLower().Equals("active"))
            {
                AlertSlider.Instance.Show($"Your Account is Banned.\nPlease Contect {Appinop.Constants.ContactEmail}", "OK")
                .OnPrimaryAction(() =>
                {
                    AlertSlider.Instance.Hide();
                    SecurePlayerPrefs.SetString(Appinop.Constants.KUserToken, "");
                    SceneManager.LoadScene((int)Scenes.Initialization);
                });
                return;
            }

            await GetGameData();


        }

        loadingView.SetStatus(100, "Loading");

        ChangeScene(validatedUser);


    }

    private async Task<bool> GetGameData()
    {
        await DataContext.Instance.FetchData();
        await WalletDataContext.Instance.Initialize();
        OnlinePlayersContext.Instance.Initialize();
        SettingsDataContext.Instance.Initialize();
        return true;
    }
    private void ShowNoInternetPopup()
    {
        if (noInternetView == null)
        {
            noInternetView = Instantiate(Resources.Load<NoInternetView>("NoInternetPopup"));
            noInternetView.RetryPressed.AddListener(RetryInternetBtnPressed);
        }
        noInternetView.EnableRetryButton();
    }
    private async void RetryInternetBtnPressed()
    {
        isConnected = await InternetConnectivityChecker.CheckInternetAsync();

        if (isConnected)
        {
            noInternetView.HidePopup();
            Start();
        }
        else
        {
            noInternetView.EnableRetryButton();
        }
    }
    private void ShowServerUnavailableAlert()
    {
        if (noServerView == null)
        {
            noServerView = Instantiate(Resources.Load<PopupControls>("NoServerPopup"));
            noServerView.RetryPressed.AddListener(RetryServerBtnPressed);
        }
    }
    private async void RetryServerBtnPressed()
    {
        ServerReachablity serverReachablity = await CheckServerReachablity();
        isServerLive = serverReachablity.success;

        if (isServerLive)
        {
            noServerView.HidePopup();
            Start();
        }
        else
        {
            noServerView.EnableRetryButton();
        }
    }

    private void ChangeScene(bool Login)
    {
        if (Login)
        {
            SceneManager.LoadScene((int)Scenes.MainMenu);
        }
        else
        {
            SceneManager.LoadScene((int)Scenes.Login);
        }
    }
    private async Task<bool> ValidateUser()
    {

        if (string.IsNullOrEmpty(SecurePlayerPrefs.GetString(Appinop.Constants.KUserToken)))
        {
            await Task.Delay(10);
            Debug.LogWarning("Token Not Found");
            return false;
        }
        else
        {
            // Debug.Log("Token : " + SecurePlayerPrefs.GetString(Appinop.Constants.KUserToken));
            var responce = await APIServices.Instance.GetAsync<SimpleBaseModel>(APIEndpoints.verifyToken);

            return responce != null ? responce.success : false;
        }
    }


    private void ShowUpdateView()
    {
        AppUpdateView.GetComponent<CanvasGroup>().alpha = 0;
        RectTransform popupTransform = AppUpdateView.transform.GetChild(0).transform as RectTransform;
        Vector3 movePosition = popupTransform.anchoredPosition;
        popupTransform.MoveOutOfScreen();


        AppUpdateView.gameObject.SetActive(true);
        AppUpdateView.GetComponent<CanvasGroup>().DOFade(1, 0.2f);

        popupTransform.MoveToPosition(movePosition);
    }

    private void HideUpdateView()
    {
        RectTransform popupTransform = AppUpdateView.transform.GetChild(0).transform as RectTransform;
        popupTransform.AnimateToBottom();
        AppUpdateView.GetComponent<CanvasGroup>().DOFade(1, 0.5f).OnComplete(() => AppUpdateView.gameObject.SetActive(false));


    }
    public void onUpdateAppButtonPressed()
    {
        HideUpdateView();
        StartCoroutine(DownloadAPKRoutine());
    }

    private IEnumerator DownloadAPKRoutine()
    {

        string filePath = Path.Combine(Application.persistentDataPath, $"{Appinop.Constants.GameName}_{newAppVersion}.apk");
        Debug.Log("File Path : " + filePath);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(APIServices.Instance.GetBaseUrl + APIEndpoints.downloadAPK))
        {
            webRequest.downloadHandler = new DownloadHandlerFile(filePath);

            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                loadingView.SetStatus(Mathf.RoundToInt(webRequest.downloadProgress * 100), $"Downloading - {Mathf.RoundToInt(webRequest.downloadProgress * 100)}%");
                yield return new WaitForSeconds(0.1f);
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {

                AlertSlider.Instance.Show("Something Went Wrong.\n" + webRequest.error, "Try Again Later").OnPrimaryAction(() => Start());
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("APK Downloaded: " + filePath);
                SecurePlayerPrefs.SetString(Appinop.Constants.KUpdatePath, filePath);
                InstallApk(filePath);
            }
        }
    }

    private void InstallApk(string filePath)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        currentActivity.Call("OpenNewVersion", filePath);
    }
}
