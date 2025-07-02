using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditProfilePhotoButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image photo;
    [SerializeField] Image backdrop;
    [SerializeField] GameObject tickmark;
    [SerializeField] bool showPhoto = true;

    [HideInInspector]
    public UnityEvent<EditProfilePhotoButton> onSelect;

    private void Awake()
    {
        button.onClick.AddListener(Select);
        photo.gameObject.SetActive(showPhoto);

        // Deselect();
    }

    public void MarkSelect()
    {
        tickmark.SetActive(true);
    }
    public void Select()
    {
        tickmark.SetActive(true);
        onSelect.Invoke(this);
    }

    public void Deselect()
    {
        tickmark.SetActive(false);
    }

    public void UpdateSprite(Sprite sprite)
    {

        photo.sprite = sprite;
    }
    public void UpdateBackdrop(Color color)
    {
        backdrop.color = color;

    }


}
