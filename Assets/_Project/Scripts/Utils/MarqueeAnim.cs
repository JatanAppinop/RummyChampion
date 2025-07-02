using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarqueeAnim : MonoBehaviour
{
    public TMP_Text label;
    public float speed = 50f; // Pixels per second
    private RectTransform parentRectTransform;

    private RectTransform labelRectTransform;
    private float textWidth;

    void Start()
    {
        parentRectTransform = this.GetComponent<RectTransform>();
        labelRectTransform = label.GetComponent<RectTransform>();

        // Initial position: Right outside the parent's right edge
        labelRectTransform.anchoredPosition = new Vector2(0, 0);
    }

    void Update()
    {
        float newX = labelRectTransform.anchoredPosition.x - speed * Time.deltaTime;

        // Check if the label has moved completely off the left edge
        if (newX < (-labelRectTransform.rect.width))
        {
            newX = parentRectTransform.rect.width; // Reset to the right edge
        }

        labelRectTransform.anchoredPosition = new Vector2(newX, 0);
    }
}
