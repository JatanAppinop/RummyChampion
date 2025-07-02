using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rummy
{


    public class RummyModesPopoverView : PopoverView
    {
        [SerializeField] private bool isLoaded = false;
        private RectTransform rectTransform;

        [Header("References")]
        [SerializeField] ScrollRect scrollView;
        [SerializeField] TextMeshProUGUI OnlinePlayersLabel;


        private void Awake()
        {
            this.rectTransform = this.transform as RectTransform;


        }
        private void Start()
        {
            Debug.Log("Rummy Mode");
        }
        public override void Hide()
        {
            rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom, Animate: true, onComplete: () =>
            {
                ResetView();
                this.gameObject.SetActive(false);
            });
        }

        public override void Show()
        {
            if (!isLoaded)
            {
                isLoaded = true;
                OnlinePlayersContext.Instance.OnDataUpdated.AddListener(OnOnlinePlayersDataUpdated);
            }

            this.gameObject.SetActive(true);
            rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom);
            this.gameObject.SetActive(true);
            rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

            DataContext.Instance.RefreshData();
            UserDataContext.Instance.RefreshData();
        }

        private void OnOnlinePlayersDataUpdated()
        {
            UpdateOnlineUsersData();
        }

        private void UpdateOnlineUsersData()
        {
            OnlinePlayersLabel.SetText(OnlinePlayersContext.Instance.GetOnlinePlayersCountForLudo().ToString());
        }
        public void OnButtonClicked(string mode)
        {
            PopoverViewController.Instance.Show(PopoverViewController.Instance.ludoContestPopover,
            new KeyValuePair<string, object>(Appinop.Constants.KModeSelect, mode)
            );
        }


        private void ResetView()
        {
            //Reset Scroll View Position
            scrollView.content.anchoredPosition = Vector2.zero;
        }


        public override void OnFocus(bool dataUpdated = false)
        {
        }
    }
}
