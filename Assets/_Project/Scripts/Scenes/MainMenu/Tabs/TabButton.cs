using System.Collections;
using System.Collections.Generic;
using LottiePlugin.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TabGroup tabBar;

    [Header("Animated Objects")]
    [SerializeField] Image unselected;
    // [SerializeField] AnimatedImage selected;
    private bool isSelected;

    private void Awake()
    {
        // selected.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabBar.onTabSelected(this);
    }

    public void Select(bool animate = true)
    {
        if (!isSelected)
        {
            isSelected = true;
            // if (animate)
            // {
            //     StartCoroutine(selectButton());
            // }
            // else
            // {
            //     unselected.gameObject.SetActive(false);
            //     selected.gameObject.SetActive(true);
            // }
        }
    }

    // IEnumerator selectButton()
    // {

    //     unselected.gameObject.SetActive(false);
    //     selected.gameObject.SetActive(true);
    //     yield return null;
    //     selected.Play();
    // }
    public void DeSelect()
    {
        if (isSelected)
        {
            isSelected = false;
            // StartCoroutine(DeSelectButton());
        }
    }

    // IEnumerator DeSelectButton()
    // {
    //     selected.gameObject.SetActive(false);
    //     selected.Stop();
    //     unselected.gameObject.SetActive(true);
    //     yield return null;
    // }


}
