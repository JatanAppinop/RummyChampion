using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Playables;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class HeroBannerController : MonoBehaviour
{
    [SerializeField] Image child;
    [SerializeField] List<Image> childrens;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] float bannerDisplayTime = 10f;

    [SerializeField] List<RectTransform> childrenRects;

    private bool isShown = false;

    private void OnEnable()
    {
        if (isShown)
        {
            if (childrenRects.Count > 1)
            {
                StartCoroutine(AnimateBannerView());
            }
            else if (childrenRects.Count <= 0)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
    async void Start()
    {

        var responce = await APIServices.Instance.GetAsync<Banners>(APIEndpoints.getBanner, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            List<BannerItem> activeBanners = responce.data.FindAll(banner => banner.isActive).FindAll(banner => banner.bannerType == "offerBanner");
            await DownloadAndDisplayBanners(activeBanners);
        }
        else
        {
            UnityNativeToastsHelper.ShowShortText("Something went Wrong.");
        }


        childrenRects = new List<RectTransform>();


        foreach (var child in childrens)
        {
            childrenRects.Add(child.GetComponent<RectTransform>());
        }

        if (childrenRects.Count > 1)
        {
            StartCoroutine(AnimateBannerView());
        }
        else if (childrenRects.Count <= 0)
        {
            this.gameObject.SetActive(false);
        }
        isShown = true;
    }

    private async Task DownloadAndDisplayBanners(List<BannerItem> banners)
    {
        foreach (var banner in banners)
        {
            string imageUrl = new Uri(new Uri(APIServices.Instance.GetBaseUrl), banner.image.Replace("\\", "/")).ToString();
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    Sprite bannerSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                    Image newChild = Instantiate(child, scrollRect.content);
                    newChild.sprite = bannerSprite;
                    newChild.gameObject.SetActive(true);

                    childrens.Add(newChild);
                }
                else
                {
                    Debug.LogError($"Failed to download image: {request.error}");
                    Debug.LogError($"UR: {imageUrl}");
                }
            }
        }
    }

    IEnumerator AnimateBannerView()
    {
        yield return new WaitForEndOfFrame();
        int currentSlide = 0;
        while (true)
        {
            if (currentSlide >= childrenRects.Count) currentSlide = 0;
            scrollRect.content.DOLocalMove(GetSnapToPositionToBringChildIntoView(scrollRect, childrenRects[currentSlide]), 0.1f);
            yield return new WaitForSeconds(bannerDisplayTime);
            currentSlide++;
        }
    }
    public Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }

}
