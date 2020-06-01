﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCL;
using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Linq;
using DCL.Interface;

public class NFTPromptHUDView : MonoBehaviour
{
    [SerializeField] internal GameObject content;

    [SerializeField] RawImage imageNft;
    [SerializeField] Image imageNftBackground;
    [SerializeField] TextMeshProUGUI textNftName;
    [SerializeField] TextMeshProUGUI textOwner;

    [Header("Last Sale")]
    [SerializeField] TextMeshProUGUI textLastSaleSymbol;
    [SerializeField] TextMeshProUGUI textLastSalePrice;
    [SerializeField] TextMeshProUGUI textLastSaleNeverSold;

    [Header("Price")]
    [SerializeField] TextMeshProUGUI textPriceSymbol;
    [SerializeField] TextMeshProUGUI textPrice;
    [SerializeField] TextMeshProUGUI textPriceNotForSale;

    [Header("Description & Comment")]
    [SerializeField] TextMeshProUGUI textDescription;
    [SerializeField] TextMeshProUGUI textComment;
    [SerializeField] GameObject containerDescription;
    [SerializeField] GameObject containerComment;

    [Header("Spinners")]
    [SerializeField] GameObject spinnerGeneral;
    [SerializeField] GameObject spinnerNftImage;

    [Header("Buttons")]
    [SerializeField] Button buttonClose;
    [SerializeField] Button buttonCancel;
    [SerializeField] Button buttonOpenMarket;
    [SerializeField] TextMeshProUGUI textOpenMarketButton;

    Coroutine fetchNFTRoutine = null;
    Coroutine fetchNFTImageRoutine = null;
    IWrappedTextureAsset imageAsset = null;

    bool backgroundColorSet = false;
    string marketUrl = null;

    private void Awake()
    {
        buttonClose.onClick.AddListener(Hide);
        buttonCancel.onClick.AddListener(Hide);
        buttonOpenMarket.onClick.AddListener(OpenMarketUrl);

        //ShowNFT("0xf64dc33a192e056bb5f0e5049356a0498b502d50", "2481", "let's add some comment");
        //ShowNFT("0xb932a70a57673d89f4acffbe830e8ed7f75fb9e0", "6674", "let's add some comment");
        //ShowNFT("0x57f1887a8bf19b14fc0df6fd9b2acc9af147ea85", "108024385621239244618096740331803135526962319540778177318110146440918862054900", "let's add some comment");
        //ShowNFT("0x1e1b3525388e8a63988f8455638aee87f68eeaa7", "7", "let's add some comment");
        //ShowNFT("0x989e1fb123b67afd66e10574c8b409bc6e812d9a", "3182", "let's add some comment");
        ShowNFT("0xd35147be6401dcb20811f2104c33de8e97ed6818", "671", "let's add some comment");
    }

    internal void ShowNFT(string assetContractAddress, string tokenId, string comment)
    {
        content.SetActive(true);
        Utils.UnlockCursor();

        if (fetchNFTRoutine != null) StopCoroutine(fetchNFTRoutine);
        if (fetchNFTImageRoutine != null) StopCoroutine(fetchNFTImageRoutine);

        SetLoading();

        fetchNFTRoutine = StartCoroutine(NFTHelper.FetchNFTInfo(assetContractAddress, tokenId,
            (nftInfo) => SetNFTInfo(nftInfo, comment),
            (error) => OnError(error)
        ));
    }

    internal void Hide()
    {
        content.SetActive(false);

        if (imageAsset != null) imageAsset.Dispose();
        if (fetchNFTRoutine != null) StopCoroutine(fetchNFTRoutine);
        if (fetchNFTImageRoutine != null) StopCoroutine(fetchNFTImageRoutine);

        fetchNFTRoutine = null;
        fetchNFTImageRoutine = null;
    }

    private void SetLoading()
    {
        imageNftBackground.color = Color.white;

        imageNft.gameObject.SetActive(false);
        textNftName.gameObject.SetActive(false);
        textOwner.gameObject.SetActive(false);
        textLastSaleSymbol.gameObject.SetActive(false);
        textLastSalePrice.gameObject.SetActive(false);
        textLastSaleNeverSold.gameObject.SetActive(false);
        textPriceSymbol.gameObject.SetActive(false);
        textPrice.gameObject.SetActive(false);
        textPriceNotForSale.gameObject.SetActive(false);
        containerDescription.SetActive(false);
        containerComment.SetActive(false);
        buttonCancel.gameObject.SetActive(false);
        buttonOpenMarket.gameObject.SetActive(false);

        spinnerGeneral.SetActive(true);
        spinnerNftImage.SetActive(false);
    }

    private void SetNFTInfo(NFTInfo info, string comment)
    {
        spinnerGeneral.SetActive(false);

        imageNftBackground.color = Color.white;
        backgroundColorSet = info.backgroundColor != null;
        if (backgroundColorSet)
        {
            imageNftBackground.color = info.backgroundColor.Value;
        }

        textNftName.text = info.name;
        textNftName.gameObject.SetActive(true);

        textOwner.text = info.owner;
        textOwner.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(info.lastSaleAmount))
        {
            textLastSalePrice.text = ShortDecimals(info.lastSaleAmount, 4);
            textLastSalePrice.gameObject.SetActive(true);
        }
        else
        {
            textLastSaleNeverSold.gameObject.SetActive(true);
        }

        if (!string.IsNullOrEmpty(info.currentPrice))
        {
            textPrice.text = ShortDecimals(info.currentPrice, 4);
            textPrice.gameObject.SetActive(true);

            if (info.currentPriceToken != null)
            {
                SetTokenSymbol(textPriceSymbol, info.currentPriceToken.Value.symbol);
            }
        }
        else
        {
            textPriceNotForSale.gameObject.SetActive(true);
        }

        if (info.lastSaleToken != null)
        {
            SetTokenSymbol(textLastSaleSymbol, info.lastSaleToken.Value.symbol);
        }

        if (!string.IsNullOrEmpty(info.description))
        {
            textDescription.text = info.description;
            containerDescription.SetActive(true);
        }

        if (!string.IsNullOrEmpty(comment))
        {
            textComment.text = comment;
            containerComment.SetActive(true);
        }

        textOpenMarketButton.text = "VIEW";
        if (info.marketInfo != null)
        {
            textOpenMarketButton.text = $"{textOpenMarketButton.text} IN {info.marketInfo.Value.name.ToUpper()}";
        }

        marketUrl = null;
        if (!string.IsNullOrEmpty(info.marketLink))
        {
            marketUrl = info.marketLink;
        }
        else if (!string.IsNullOrEmpty(info.assetLink))
        {
            marketUrl = info.assetLink;
        }

        buttonCancel.gameObject.SetActive(true);
        buttonOpenMarket.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(info.thumbnailUrl))
        {
            spinnerNftImage.SetActive(true);
            fetchNFTImageRoutine = StartCoroutine(Utils.FetchWrappedTextureAsset(info.thumbnailUrl, (asset) =>
            {
                imageAsset = asset;
                imageNft.texture = asset.texture;

                var gifAsset = asset as WrappedGif;
                if (gifAsset != null)
                {
                    gifAsset.SetUpdateTextureCallback((texture) =>
                    {
                        imageNft.texture = texture;
                    });
                }
                SetNFTImageSize(asset.texture);
                if (!backgroundColorSet) SetSmartBackgroundColor(asset.texture);

                imageNft.gameObject.SetActive(true);
                spinnerNftImage.SetActive(false);
            }));
        }
    }

    private void SetNFTImageSize(Texture2D texture)
    {
        RectTransform rt = (RectTransform)imageNft.transform.parent;
        float h = rt.rect.height;
        float w = h * (texture.width / (float)texture.height);
        imageNft.rectTransform.sizeDelta = new Vector2(w, h);
    }

    private string ShortDecimals(string value, int decimalCount)
    {
        int pointPosition = value.IndexOf('.');
        if (pointPosition <= 0) return value;

        string ret = value.Substring(0, pointPosition + Mathf.Min(value.Length - pointPosition, decimalCount + 1));

        for (int i = ret.Length - 1; i >= 0; i--)
        {
            if (ret[i] == '.')
            {
                return ret.Substring(0, i);
            }
            if (ret[i] != '0')
            {
                return ret.Substring(0, i + 1);
            }
        }

        return ret;
    }

    private void SetSmartBackgroundColor(Texture2D texture)
    {
        imageNftBackground.color = texture.GetPixel(0, 0);
    }

    private void SetTokenSymbol(TextMeshProUGUI textToken, string symbol)
    {
        textToken.text = symbol;
        textToken.gameObject.SetActive(true);
    }

    private void OpenMarketUrl()
    {
        if (!string.IsNullOrEmpty(marketUrl))
        {
            WebInterface.OpenURL(marketUrl);
        }
        else
        {
            Hide();
        }
    }

    private void OnError(string error)
    {
        Debug.LogError(error);
        Hide();
        Utils.LockCursor();
    }

    private void OnDestroy()
    {
        if (imageAsset != null)
        {
            imageAsset.Dispose();
        }
    }
}
