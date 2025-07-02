using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TermsLinkOpener : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI text;

    public void OnPointerClick(PointerEventData eventData)
    {
        // First, get the index of the link clicked. Each of the links in the text has its own index.
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, Camera.main);


        if (linkIndex >= 0)
        {
            var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

            var url = linkId switch
            {
                "tnc" => Appinop.Constants.TermsConditionURL,
                "privacy" => Appinop.Constants.PrivacyPolicyURL,
                _ => Appinop.Constants.SiteURL
            };


            Application.OpenURL(url);
        }
    }
}
