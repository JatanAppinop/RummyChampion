using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Appinop.GetPhoneNumber;
using System.Text.RegularExpressions;
using SecPlayerPrefs;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginScreenController : MonoBehaviour
{
    [SerializeField] private List<AnimatedView> animatedViews;

    [Header("Login Page")]
    [SerializeField] AdvancedInputField phoneNumberInput;
    [SerializeField] PrimaryButton GetStartedButton;
    [Header("Verification Page")]
    [SerializeField] RectTransform spinner;

    [Header("OTP Page")]
    [SerializeField] MultiTextInput otpInput;
    [SerializeField] PrimaryButton VerifyButton;
    [SerializeField] GameObject ResendOTPView;
    [SerializeField] TextMeshProUGUI ResendOTPTimerLabel;
    [SerializeField] TextMeshProUGUI phNoLabel;

    [Header("Editor OTP Input")]
    [SerializeField] string debugOTP;
    private ModalTransitionManager transitionManager;


    IEnumerator Start()
    {
        transitionManager = new ModalTransitionManager(animatedViews);
        yield return new WaitForEndOfFrame();

#if UNITY_ANDROID && !UNITY_EDITOR
        GetPhoneNumberHelper.Instance.GetNumber()
            .OnSuccess((number) =>
            {
                phoneNumberInput.text = number.Substring(number.Length - 10);
            })
            .OnError((e) => phoneNumberInput.Focus());
#endif
        spinner.gameObject.SetActive(false);

        AudioManager.Instance.StopMusic();
    }



    public void onGetStartedPresses()
    {

        if (validPhoneNumber())
        {
            SendOTP();
            GetStartedButton.SetInteractable(false);
        }
    }

    private bool validPhoneNumber()
    {

        string pattern = @"^\d{10}$";
        bool isValid = Regex.IsMatch(phoneNumberInput.text, pattern);

        if (phoneNumberInput.text.Length != 10 || !isValid)
        {
            phoneNumberInput.ShowError("Enter a Valid Phone Number");
        }
        else
        {
            phoneNumberInput.HideError();
        }

        if (phoneNumberInput.text.Length == 10 && isValid)
        {
            return true;
        }


        return false;
    }

    private async void SendOTP()
    {
        string requestBodyJson = "{\"mobileNumber\":\"" + phoneNumberInput.text + "\"}";
        var responce = await APIServices.Instance.PostAsync<SimpleBaseModel>(APIEndpoints.sentOtp, requestBodyJson, false);
        if (responce != null && responce.success)
        {
            GetStartedButton.SetInteractable(true);
            phNoLabel.SetText(phoneNumberInput.text);

#if UNITY_ANDROID && !UNITY_EDITOR
            transitionManager.MoveToView("Verification").OnComplete(() =>
            {
                StartCoroutine(GetAutoOTP());
            });
#endif  
#if UNITY_EDITOR || UNITY_IOS

            transitionManager.MoveToView("OTP").OnComplete(() =>
            {
                StartCoroutine(ResendOTPTimer());
                StartCoroutine(ReadOTPandCloseKB());
            });
#endif 

        }
        else
        {
            GetStartedButton.SetInteractable(true);
            phoneNumberInput.ShowError(responce.message);
        }

    }

    public void OnVerifyPressed()
    {
#if UNITY_EDITOR
        otpInput.text = debugOTP;
#endif
        if (validOTP())
        {
            SubmitOTP();
            VerifyButton.SetInteractable(false);
        }

    }

    private bool validOTP()
    {
        if (otpInput.text.Length == 6)
        {
            int otp;
            if (int.TryParse(otpInput.text, out otp))
            {
                otpInput.HideError();
                return true;
            }
        }

        otpInput.ShowError("Invalid OTP");
        return false;
    }
    private async void SubmitOTP()
    {

        string requestBodyJson = "{\"mobileNumber\":\"" + phoneNumberInput.text + "\" ,\"otp\":" + int.Parse(otpInput.text) + "}";
        var responce = await APIServices.Instance.PostAsync<UserVerification>(APIEndpoints.loginAndSignup, requestBodyJson, false);

        if (responce != null && responce.success)
        {
            Debug.Log("OTP Verification : " + responce.message);
            SecurePlayerPrefs.SetString(Appinop.Constants.KUserToken, responce.data.accessToken);
            APIServices.Instance.UpdateToken(responce.data.accessToken);

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
            await DataContext.Instance.FetchData();
            await WalletDataContext.Instance.Initialize();
            OnlinePlayersContext.Instance.Initialize();
            SettingsDataContext.Instance.Initialize();

            ShowSuccessScreen();
        }
        else
        {
            VerifyButton.SetInteractable(true);
            otpInput.ShowError("Enter a Valid OTP");
        }
    }

    private void ShowSuccessScreen()
    {
        transitionManager.MoveToView("Done");
        StartCoroutine(ChangeToMainMenu());
    }

    public void OnChangePhoneNumberPressed()
    {
        AlertSlider.Instance.Show("Do you want to change the Phone number ?", "Change Number", "Cancel")
        .OnPrimaryAction(() =>
        {
            transitionManager.MoveToView("Login");

        });
    }
    public async void OnResendOTPPressed()
    {
        VerifyButton.SetInteractable(false);
        string requestBodyJson = "{\"mobileNumber\":\"" + phoneNumberInput.text + "\" ,\"resend\":" + "true" + "}";
        var responce = await APIServices.Instance.PostAsync<SimpleBaseModel>(APIEndpoints.sentOtp, requestBodyJson, false);
        if (responce != null && responce.success)
        {
            VerifyButton.SetInteractable(true);
            StartCoroutine(ResendOTPTimer());

        }
        else
        {
            VerifyButton.SetInteractable(true);
            StartCoroutine(ResendOTPTimer());
        }
    }
    IEnumerator ChangeToMainMenu()
    {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }

    IEnumerator GetAutoOTP()
    {
        Debug.LogWarning("Started Auto Fetch OTP");
        yield return new WaitForSeconds(0.1f);
        //Fetch OTP
        GetAutoOTPFunc();
    }

    IEnumerator ReadOTPandCloseKB()
    {

        while (otpInput.text.Length < 6)
        {
            yield return new WaitForSeconds(0.1f);
        }
        otpInput.CloseKeyboard();
        Debug.LogError($"OTP is now 6 Digit : " + otpInput.text);
    }

    IEnumerator ResendOTPTimer()
    {

        int timer = 15;
        ResendOTPTimerLabel.gameObject.SetActive(true);
        ResendOTPView.SetActive(false);
        yield return new WaitForEndOfFrame();

        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            ResendOTPTimerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        ResendOTPTimerLabel.gameObject.SetActive(false);
        ResendOTPView.SetActive(true);
    }
    private void GetAutoOTPFunc()
    {

        GetPhoneNumberHelper.Instance.GetOTPMessage()
                .OnSuccess((message) =>
                {
                    spinner.gameObject.SetActive(true);
                    string otp = FilterOTP(message);
                    otpInput.SetText(otp);
                    OnVerifyPressed();

                }).OnError((err) =>
                {
                    transitionManager.MoveToView("OTP").OnComplete(() =>
            {
                StartCoroutine(ResendOTPTimer());
                StartCoroutine(ReadOTPandCloseKB());

            });
                });
    }



    private string FilterOTP(string input)
    {

        string pattern = @"\d{6}";

        var match = Regex.Match(input, pattern);

        if (match.Success)
        {
            string otp = match.Value;
            Debug.Log("Extracted OTP: " + otp);
            return otp;
        }
        else
        {
            Debug.LogError("OTP not found in the message.");
            return "";
        }
    }

    private void Update()
    {
        spinner.Rotate(Vector3.back * Time.deltaTime * 100f);
    }

}