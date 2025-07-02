using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SafeAreaResizer : MonoBehaviour
{
    private void Awake()
    {
        ResizeToSafeArea();

    }
    private void ResizeToSafeArea()
    {
        RectTransform View = transform as RectTransform;
        if (View == null)
        {
            Debug.LogError("Gameobject is not inside a Canvas", this.gameObject);
            return;
        }

        List<Canvas> canvases = GetComponentsInParent<Canvas>().ToList();
        Canvas canvas = canvases.Find(c => c.CompareTag("MainCanvas"));
        if (canvas == null)
        {
            if (canvases.Count > 0)
                Debug.LogError("Canvas with \"MainCanvas\" Tag Not Found", this.gameObject);
            else
                Debug.LogError("Canvas Not Found in The Parent", this.gameObject);
            return;
        }

        RectTransform canvasRectTransform = canvas.transform as RectTransform;

        var safeArea = Screen.safeArea;

        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        View.anchorMin = anchorMin;
        View.anchorMax = anchorMax;
    }
}