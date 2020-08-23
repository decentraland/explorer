using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Categories = WearableLiterals.Categories;
using Rarity = WearableLiterals.ItemRarity;

public class AvatarEditorHUDAudioHandler : MonoBehaviour
{
    [SerializeField]
    AvatarEditorHUDView view;

    AudioContainer audioContainer;

    private void Start()
    {
        audioContainer = GetComponent<AudioContainer>();
        audioContainer.GetEvent("AvatarAppear").SetPitch(0.4f);

        int nPairs = view.wearableGridPairs.Length;
        for (int i = 0; i < nPairs; i++)
        {
            view.wearableGridPairs[i].selector.OnItemClicked += OnWearableClicked;
        }

        view.skinColorSelector.OnColorChanged += OnSkinColorClicked;
        view.eyeColorSelector.OnColorChanged += OnEyeColorClicked;
        view.hairColorSelector.OnColorChanged += OnHairColorClicked;
    }

    void OnWearableClicked(string wearableId)
    {
        var wearable = CatalogController.wearableCatalog.Get(wearableId);
        if (wearable == null) return;

        Debug.Log(wearable.rarity + " - " + wearable.type + " - " + wearable.category);

        switch(wearable.rarity)
        {
            case Rarity.RARE:
                break;
            case Rarity.EPIC:
                break;
            case Rarity.LEGENDARY:
                break;
            case Rarity.MYTHIC:
                break;
            case Rarity.UNIQUE:
                break;
            default:
                break;
        }

        switch (wearable.category)
        {
            case Categories.EYEBROWS:
                Play("Hair");
                break;
            case Categories.FACIAL:
                Play("Hair");
                break;
            case Categories.FEET:
                Play("Footwear");
                break;
            case Categories.HAIR:
                Play("Hair");
                break;
            case Categories.LOWER_BODY:
                Play("Clothing");
                break;
            case Categories.UPPER_BODY:
                Play("Clothing");
                break;
            case "eyewear":
                Play("Eyewear");
                break;
            case "tiara":
                Play("Jewelry");
                break;
            case "earring":
                Play("Jewelry");
                break;
            case "hat":
                Play("Hat");
                break;
            case "top_head":
                Play("Footwear");
                break;
            case "helmet":
                Play("Footwear");
                break;
            case "mask":
                Play("Hat");
                break;
            default:
                break;
        }
    }

    void OnSkinColorClicked(Color color)
    {

    }

    void OnEyeColorClicked(Color color)
    {

    }

    void OnHairColorClicked(Color color)
    {

    }

    void Play(string eventName)
    {
        audioContainer.GetEvent(eventName).Play(true);
    }
}
