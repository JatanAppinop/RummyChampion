using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;


public class PlayerSection : MonoBehaviour
{
    // Attributes --------------------
    [Header("Index of Position in Board")]
    public int Index;

    [Header("Occupation with Player")]
    public bool occupied = false;
    [HideInInspector]
    public BoardColorsUtils.BoardColors sectionColor;

    [Header("Token Position Placeholders")]
    public List<RectTransform> tokenPositions;

    [Header("Path Checkpoint that token will travel")]
    public List<RectTransform> playerPath;

    [Header("Colored items that will change colors.")]
    [SerializeField] List<Image> coloredItems;

    [Header("TimerMode Ref")]
    [SerializeField] GameObject timerModeBG;
    [SerializeField] TextMeshProUGUI timerPoints;
    // Private Attributs
    private Sequence highlightSeq;

    private void Awake()
    {
        //UpdateSectionColor(Color.white);
        this.timerPoints.text = "0";
    }

    public void UpdateSectionColor(Color color)
    {
        foreach (Image img in coloredItems)
        {
            img.DOColor(color, Appinop.Constants.TSectionColorChange);
        }
    }

    public void HighlightSection()
    {
        Image img = this.gameObject.GetComponent<Image>();
        highlightSeq = DOTween.Sequence();
        highlightSeq.Append(img.DOFade(0.6f, 0.3f));
        highlightSeq.Append(img.DOFade(1.0f, 0.3f).SetDelay(0.2f));
        highlightSeq.SetLoops(50);
    }

    public void StopHighlight()
    {
        highlightSeq.Kill(true);

        //this.gameObject.GetComponent<Image>().DOFade(1, 0.01f);
    }

    public void HidePlayerPlaceholder()
    {
        foreach (var place in tokenPositions)
        {
            place.DOScale(Vector2.zero, 0.2f);
        }
    }

    public void SetGameMode(Appinop.Constants.GameMode gameMode)
    {
        if (gameMode == Appinop.Constants.GameMode.Timer || gameMode == Appinop.Constants.GameMode.Turbo)
        {
            this.timerModeBG.SetActive(true);
            this.tokenPositions[0].parent.gameObject.SetActive(false);
        }
        else
        {
            this.timerModeBG.SetActive(false);
            this.tokenPositions[0].parent.gameObject.SetActive(true);

        }
    }

    public void UpdatePoints(int points, int count)
    {


        this.timerPoints.text = points.ToString();

        GameObject parent = GameObject.Find("Effect Container");

        AddPointAnimation effect = Instantiate(Resources.Load<AddPointAnimation>("PointAnimation"), parent.transform, false);
        RectTransform effRect = effect.GetComponent<RectTransform>();

        RectTransform targetRect = timerPoints.GetComponent<RectTransform>();
        var toDestinationInLocalSpace = effRect.InverseTransformVector(targetRect.position);

        effRect.localPosition = toDestinationInLocalSpace;

        effect.Show(count);

    }
    public void UpdatePointsLabels(int points)
    {
        this.timerPoints.text = points.ToString();
    }

}
