using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardShadow : MonoBehaviour {

    Shadow cardShadow;
    float minShadowDistance = 7f;
    float maxShadowDistance = 30f;
    float minShadowAlpha = 0.15f;
    float maxShadowAlpha = 0.5f;
    float minZDistance = -80f;
    float maxZDistance = 0f;
    bool cardOnMove;

    private void Awake()
    {
        cardShadow = gameObject.AddComponent<Shadow>();
        minZDistance = Constants.DRAG_CARD_Z_DISTANCE;
    }

    private void Update()
    {
        if (cardOnMove)
        {
            float currZ = Mathf.Clamp(transform.localPosition.z, minZDistance, maxZDistance);
            float distanceFactor = (currZ - minZDistance) / (maxZDistance - minZDistance);
            float currDistanceValue = Mathf.Lerp(maxShadowDistance, minShadowDistance, distanceFactor);
            float currShadowAlpha = Mathf.Lerp(minShadowAlpha, maxShadowAlpha, distanceFactor);

            cardShadow.effectDistance = new Vector2(currDistanceValue, -currDistanceValue);
            cardShadow.effectColor = new Color(0, 0, 0, currShadowAlpha); 
        }
    }

    public void StartShadowAnimation()
    {
        cardOnMove = true;
    }

    public void StopShadowAnimation()
    {
        cardOnMove = false;
    }
}   
