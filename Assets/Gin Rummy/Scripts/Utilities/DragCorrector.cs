using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(EventSystem))]
    [AddComponentMenu("UI/Extensions/DragCorrector")]
    public class DragCorrector : MonoBehaviour
    {
        public int basePPI = 210;
        int dragTH = 0;

        void Start()
        {
            EventSystem es = GetComponent<EventSystem>();
            if (es)
            {
                int defaultValue = es.pixelDragThreshold;
                dragTH = Mathf.Max(
                             defaultValue,
                             (int)(defaultValue * Screen.dpi / basePPI));
                es.pixelDragThreshold = dragTH;
            }
        }
    }
}