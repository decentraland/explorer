using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Categories = WearableLiterals.Categories;
using Rarity = WearableLiterals.ItemRarity;

public class AvatarEditorHUDAudioHandler : MonoBehaviour
{
    [SerializeField]
    AvatarEditorHUDView view;
    [SerializeField]
    ItemSelector nftItemSelector; // (Mordi) This guy handles wearables in the "collectibles" tab
    [SerializeField]
    Button randomizeButton;
    [SerializeField]
    AudioEvent eventRarity, eventAvatarAppear, eventReactionMale, eventReactionFemale, eventWearableClothing, eventWearableEyewear, eventWearableJewelry,
        eventWearableFootwear, eventWearableHair, eventWearableHatMask, eventWearableRarity;

    WearableItem lastClickedWearable = null;

    private void Start()
    {
        int nPairs = view.wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            view.wearableGridPairs[i].selector.OnItemClicked += OnWearableClicked;
        }

        view.eyeColorSelector.OnColorChanged += OnEyeColorChanged;
        view.skinColorSelector.OnColorChanged += OnSkinColorChanged;
        view.OnAvatarAppear += OnAvatarAppear;

        nftItemSelector.OnItemClicked += OnWearableClicked;

        if (randomizeButton != null)
            randomizeButton.onClick.AddListener(ResetLastClickedWearable);
    }

    void OnWearableClicked(string wearableId)
    {
        var wearable = CatalogController.wearableCatalog.Get(wearableId);
        lastClickedWearable = wearable;
        if (wearable == null) return;

        switch (wearable.category)
        {
            case Categories.EYEBROWS:
                eventWearableHair.Play(true);
                break;
            case "facial_hair":
                eventWearableHair.Play(true);
                break;
            case Categories.FEET:
                eventWearableFootwear.Play(true);
                break;
            case Categories.HAIR:
                eventWearableHair.Play(true);
                break;
            case Categories.LOWER_BODY:
                eventWearableClothing.Play(true);
                break;
            case Categories.UPPER_BODY:
                eventWearableClothing.Play(true);
                break;
            case "eyewear":
                eventWearableEyewear.Play(true);
                break;
            case "tiara":
                eventWearableJewelry.Play(true);
                break;
            case "earring":
                eventWearableJewelry.Play(true);
                break;
            case "hat":
                eventWearableHatMask.Play(true);
                break;
            case "top_head":
                eventWearableFootwear.Play(true);
                break;
            case "helmet":
                eventWearableFootwear.Play(true);
                break;
            case "mask":
                eventWearableHatMask.Play(true);
                break;
            default:
                break;
        }
    }

    void OnEyeColorChanged(Color color)
    {
        ResetLastClickedWearable();
    }

    void OnSkinColorChanged(Color color)
    {
        ResetLastClickedWearable();
    }

    void ResetLastClickedWearable()
    {
        lastClickedWearable = null;
    }

    void OnAvatarAppear(AvatarModel model)
    {
        if (!view.isOpen) return;

        eventAvatarAppear.Play(true);
        PlayRarity();
        PlayVoiceReaction(model.bodyShape);
    }

    void PlayRarity()
    {
        if (lastClickedWearable == null)
            return;

        if (lastClickedWearable.rarity == null)
            return;

        /*switch (lastClickedWearable.rarity)
         {
             case Rarity.RARE:
                 eventRarity.SetIndex(0);
                 break;
             case Rarity.EPIC:
                 eventRarity.SetIndex(1);
                 break;
             case Rarity.LEGENDARY:
                 eventRarity.SetIndex(2);
                 break;
             case Rarity.MYTHIC:
                 eventRarity.SetIndex(3);
                 break;
             case Rarity.UNIQUE:
                 eventRarity.SetIndex(4);
                 break;
             default:
                 eventRarity.SetIndex(0);
                 break;
         }*/

        if (lastClickedWearable.rarity == Rarity.UNIQUE)
            eventRarity.SetIndex(1);
        else
            eventRarity.SetIndex(0);

        eventRarity.Play(true);
    }

    void PlayVoiceReaction(string bodyShape)
    {
        float chanceToPlay = 0.7f;

        AudioEvent eventReaction = null;

        if (bodyShape.Contains("Female"))
            eventReaction = eventReactionFemale;
        else
            eventReaction = eventReactionMale;

        if (lastClickedWearable != null)
        {
            if (lastClickedWearable.rarity != null)
            {
                eventReaction.RandomizeIndex(5, 9);

                if (lastClickedWearable.rarity == Rarity.UNIQUE)
                {
                    chanceToPlay = 1f;
                }
            }
            else
            {
                eventReaction.RandomizeIndex(0, 5);
            }
        }

        if (eventReaction != null && Random.Range(0f, 1f) <= chanceToPlay)
        {
            if (!eventReaction.source.isPlaying)
            {
                eventReaction.PlayScheduled(0.6f);
            }
        }
    }
}
