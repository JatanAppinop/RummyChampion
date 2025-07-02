using System.Collections;
using System.Collections.Generic;
using System.IO;
using Appinop;
using UnityEngine;
using UnityEngine.UI;

public class DLKYCPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    [SerializeField] ScrollRect scrollRect;

    [Space]
    [SerializeField] AdvancedInputField nameInput;
    [SerializeField] List<GameObject> buttons;
    [SerializeField] PrimaryButton submitButton;
    [SerializeField] Image backdropImage;

    private string imagePath;
    private string fileType;

    private RectTransform rectTransform;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
    }

    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
        });
        ResetView();
    }


    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
    }

    public override void OnFocus(bool dataUpdated = false)
    {

    }

    public async void onSubmitButtonPressed()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            nameInput.ShowError("Please Enter your name.");
            return;
        }

        if (imagePath == null || imagePath.Length < 1)
        {
            UnityNativeToastsHelper.ShowShortText("Please Select Your Driving License Photo.");
            Debug.LogError("Please Select Your Driving License Photo.");
            return;
        }

        submitButton.SetInteractable(false);

        // Prepare the data dictionary
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("documentType", "Driving License");
        data.Add("fullName", nameInput.text);

        // Prepare the files dictionary
        Dictionary<string, string> files = new Dictionary<string, string>();
        files.Add("documentImage", imagePath);

        //Call the PostForm function
        var responce = await APIServices.Instance.PostForm<SimpleBaseModel>(APIEndpoints.submitKYC, data, files);

        if (responce != null && responce.success)
        {
            AlertSlider.Instance.Show("KYC Submitted.\nYour KYC will be processed in 24 Hours.", "OK")
            .OnPrimaryAction(() =>
            {
                AlertSlider.Instance.Hide();
                UserDataContext.Instance.RefreshData();
                PopoverViewController.Instance.GoBack(true);
            });
        }
        else if (responce != null && !responce.success)
        {

            AlertSlider.Instance.Show($"KYC Submission Failed.\n{responce.message}", "OK")
            .OnPrimaryAction(() =>
            {
                submitButton.SetInteractable(true);
                AlertSlider.Instance.Hide();
            });
        }
        else
        {
            AlertSlider.Instance.Show($"KYC Submission Failed.\nTry Again later.", "OK")
            .OnPrimaryAction(() =>
            {
                submitButton.SetInteractable(true);
                AlertSlider.Instance.Hide();
            });

        }
    }

    public async void CameraPickerClicked()
    {

        NativeCamera.Permission per = await NativeCamera.RequestPermissionAsync(true);

        Debug.Log("Permission result: " + per);

        int maxSize = 2048;
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
    {
        if (path == null)
        {
            Debug.Log("Operation cancelled");
            Debug.LogError("> Canceled");
            UnityNativeToastsHelper.ShowShortText("Something Went Wrong");
        }
        else
        {
            Debug.Log("Picked file: " + path);
            imagePath = path;
            ApplyImage();
        }
    }, maxSize);

        Debug.Log("Permission result: " + permission);
    }

    public async void ImagePickerClicked()
    {

        NativeFilePicker.Permission per = await NativeFilePicker.RequestPermissionAsync(true);
        Debug.Log("Permission result: " + per);

        // imagePicker.Show("Select Image", "kyc-pancard", 2048);
        fileType = NativeFilePicker.ConvertExtensionToFileType("image");
        Debug.Log("pdf's MIME/UTI is: " + fileType);

        // Don't attempt to import/export files if the file picker is already open
        if (NativeFilePicker.IsFilePickerBusy())
            return;

#if UNITY_ANDROID
        // Use MIMEs on Android
        string[] fileTypes = new string[] { "image/*" };
#else
			// Use UTIs on iOS
			string[] fileTypes = new string[] { "public.image" };
#endif

        // Pick image(s) and/or video(s)
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
                Debug.LogError("> Canceled");

                UnityNativeToastsHelper.ShowShortText("Something Went Wrong");
            }
            else
            {
                Debug.Log("Picked file: " + path);
                imagePath = path;
                ApplyImage();
            }
        }, fileTypes);

        Debug.Log("Permission result: " + permission);


    }

    private void ApplyImage()
    {

        // Load the image as a Texture2D
        Texture2D texture = null;
        byte[] imageData = File.ReadAllBytes(imagePath);
        texture = new Texture2D(2, 2); // Set temporary dimensions
        texture.LoadImage(imageData);

        // Create a sprite from the Texture2D
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Assign the sprite to the Image component
        backdropImage.sprite = sprite;

        buttons.ForEach(g => g.SetActive(false));
        backdropImage.gameObject.SetActive(true);
    }

    public void ClearImageBtnPressed()
    {
        buttons.ForEach(g => g.SetActive(true));
        backdropImage.gameObject.SetActive(false);
        backdropImage.sprite = null;
        imagePath = "";
    }

    private void ResetView()
    {
        imagePath = "";
        fileType = "";
        backdropImage.gameObject.SetActive(false);
        buttons.ForEach(button => button.gameObject.SetActive(true));
        nameInput.HideError();
        nameInput.text = "";
        submitButton.SetInteractable(true);
        scrollRect.content.anchoredPosition = Vector3.zero;
    }
}
