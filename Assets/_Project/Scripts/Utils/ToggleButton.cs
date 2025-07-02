using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    public bool isON = false;

    private void Awake()
    {
        if (onSprite == null) Debug.LogError("On Sprite Not Found.", this.gameObject);
        if (offSprite == null) Debug.LogError("Off Sprite Not Found.", this.gameObject);

        updateImage();
    }
    void Start()
    {
        button.onClick.AddListener(onButtonPressed);
    }

    public void updateButton(bool b)
    {
        isON = b;
        updateImage();

    }
    void onButtonPressed()
    {
        isON = !isON;
        updateImage();

    }

    void updateImage()
    {
        image.sprite = isON ? onSprite : offSprite;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(onButtonPressed);
    }
}
