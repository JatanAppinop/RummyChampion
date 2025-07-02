using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appinop;
using LottiePlugin.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransactionsPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;
    [SerializeField] AnimatedImage loader;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] TransactionCell cellPrefab;

    [SerializeField] List<TransactionCell> cells;
    [SerializeField] TextMeshProUGUI nothingtoShow;


    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
    }

    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
            ResetView();
        });
    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            loader.gameObject.SetActive(true);
            nothingtoShow.gameObject.SetActive(false);
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        UpdateList();

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);
    }

    private async Task<List<TransactionData>> FetchData()
    {

        var response = await APIServices.Instance.GetAsync<Transactions>(APIEndpoints.getTransactions);
        if (response != null && response.success)
        {
            return response.data;
        }
        else
        {
            return new List<TransactionData>();
        }
    }

    private async void UpdateList()
    {

        var data = await FetchData();

        var filteredData = data.Where(i => i.amount != 0).ToList();
        UpdateUI(filteredData);
    }

    private void UpdateUI(List<TransactionData> data)
    {
        loader.Stop();
        loader.gameObject.SetActive(false);

        if (data.Count > 0)
        {


            int requiredCells = data.Count;

            if (cells.Count > requiredCells)
            {
                while (cells.Count > requiredCells)
                {
                    TransactionCell cell = cells.Last();
                    cells.Remove(cell);
                    DestroyImmediate(cell.gameObject);
                }
            }
            else if (cells.Count == data.Count)
            {
                return;
            }
            else
            {
                while (cells.Count < requiredCells)
                {
                    TransactionCell cell = Instantiate(cellPrefab, scrollRect.content);
                    cells.Add(cell);
                }
            }

            for (int i = 0; i < requiredCells; i++)
            {
                cells[i].UpdateData(data[i]);
            }
        }
        else
        {
            nothingtoShow.gameObject.SetActive(true);
        }

        loader.gameObject.SetActive(false);
    }
    private void ResetView()
    {
        scrollRect.content.anchoredPosition = Vector2.zero;
    }
    public override void OnFocus(bool dataUpdated = false)
    {

    }

}
