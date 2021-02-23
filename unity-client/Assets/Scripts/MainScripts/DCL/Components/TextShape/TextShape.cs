using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class TextShape : BaseComponent
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public bool billboard;

            [Header("Font Properties")] public string value = "";

            public bool visible = true;

            public Color color = Color.white;
            public float opacity = 1f;
            public float fontSize = 100f;
            public bool fontAutoSize = false;
            public string fontWeight = "normal";
            public string font;

            [Header("Text box properties")] public string hTextAlign = "bottom";

            public string vTextAlign = "left";
            public float width = 1f;
            public float height = 0.2f;
            public bool adaptWidth = false;
            public bool adaptHeight = false;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public float lineSpacing = 0f;
            public int lineCount = 0;
            public bool textWrapping = false;

            [Header("Text shadow properties")] public float shadowBlur = 0f;

            public float shadowOffsetX = 0f;
            public float shadowOffsetY = 0f;
            public Color shadowColor = new Color(1, 1, 1);

            [Header("Text outline properties")] public float outlineWidth = 0f;

            public Color outlineColor = Color.white;

            public override bool Equals(object obj)
            {
                return obj is Model model &&
                       billboard == model.billboard &&
                       value == model.value &&
                       visible == model.visible &&
                       color.Equals(model.color) &&
                       opacity == model.opacity &&
                       fontSize == model.fontSize &&
                       fontAutoSize == model.fontAutoSize &&
                       fontWeight == model.fontWeight &&
                       font == model.font &&
                       hTextAlign == model.hTextAlign &&
                       vTextAlign == model.vTextAlign &&
                       width == model.width &&
                       height == model.height &&
                       adaptWidth == model.adaptWidth &&
                       adaptHeight == model.adaptHeight &&
                       paddingTop == model.paddingTop &&
                       paddingRight == model.paddingRight &&
                       paddingBottom == model.paddingBottom &&
                       paddingLeft == model.paddingLeft &&
                       lineSpacing == model.lineSpacing &&
                       lineCount == model.lineCount &&
                       textWrapping == model.textWrapping &&
                       shadowBlur == model.shadowBlur &&
                       shadowOffsetX == model.shadowOffsetX &&
                       shadowOffsetY == model.shadowOffsetY &&
                       shadowColor.Equals(model.shadowColor) &&
                       outlineWidth == model.outlineWidth &&
                       outlineColor.Equals(model.outlineColor);
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = -852556059;
                hashCode = hashCode * -1521134295 + billboard.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(value);
                hashCode = hashCode * -1521134295 + visible.GetHashCode();
                hashCode = hashCode * -1521134295 + color.GetHashCode();
                hashCode = hashCode * -1521134295 + opacity.GetHashCode();
                hashCode = hashCode * -1521134295 + fontSize.GetHashCode();
                hashCode = hashCode * -1521134295 + fontAutoSize.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(fontWeight);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(font);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(hTextAlign);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(vTextAlign);
                hashCode = hashCode * -1521134295 + width.GetHashCode();
                hashCode = hashCode * -1521134295 + height.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptWidth.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptHeight.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingTop.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingRight.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingBottom.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingLeft.GetHashCode();
                hashCode = hashCode * -1521134295 + lineSpacing.GetHashCode();
                hashCode = hashCode * -1521134295 + lineCount.GetHashCode();
                hashCode = hashCode * -1521134295 + textWrapping.GetHashCode();
                hashCode = hashCode * -1521134295 + shadowBlur.GetHashCode();
                hashCode = hashCode * -1521134295 + shadowOffsetX.GetHashCode();
                hashCode = hashCode * -1521134295 + shadowOffsetY.GetHashCode();
                hashCode = hashCode * -1521134295 + shadowColor.GetHashCode();
                hashCode = hashCode * -1521134295 + outlineWidth.GetHashCode();
                hashCode = hashCode * -1521134295 + outlineColor.GetHashCode();
                return hashCode;
            }
        }

        public TextMeshPro text;
        public RectTransform rectTransform;
        public Model cachedModel;

        public void Update()
        {
            if (cachedModel.billboard && Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (rectTransform == null) yield break;

            Model model = (Model) newModel;
            cachedModel = model;
            PrepareRectTransform();

            yield return ApplyModelChanges(scene, text, model);
        }

        public static IEnumerator ApplyModelChanges(ParcelScene scene, TMP_Text text, Model model)
        {
            if (!string.IsNullOrEmpty(model.font))
            {
                yield return DCLFont.SetFontFromComponent(scene, model.font, text);
            }

            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.visible ? model.opacity : 0);
            text.fontSize = (int) model.fontSize;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableAutoSizing = model.fontAutoSize;

            text.margin =
                new Vector4
                (
                    (int) model.paddingLeft,
                    (int) model.paddingTop,
                    (int) model.paddingRight,
                    (int) model.paddingBottom
                );

            text.alignment = GetAlignment(model.vTextAlign, model.hTextAlign);
            text.lineSpacing = model.lineSpacing;

            if (model.lineCount != 0)
            {
                text.maxVisibleLines = Mathf.Max(model.lineCount, 1);
            }
            else
            {
                text.maxVisibleLines = int.MaxValue;
            }

            text.enableWordWrapping = model.textWrapping && !text.enableAutoSizing;

            if (model.shadowOffsetX != 0 || model.shadowOffsetY != 0)
            {
                text.fontMaterial.EnableKeyword("UNDERLAY_ON");
                text.fontMaterial.SetColor("_UnderlayColor", model.shadowColor);
                text.fontMaterial.SetFloat("_UnderlaySoftness", model.shadowBlur);
            }
            else if (text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"))
            {
                text.fontMaterial.DisableKeyword("UNDERLAY_ON");
            }

            if (model.outlineWidth > 0f)
            {
                text.fontMaterial.EnableKeyword("OUTLINE_ON");
                text.outlineWidth = model.outlineWidth;
                text.outlineColor = model.outlineColor;
            }
            else if (text.fontMaterial.IsKeywordEnabled("OUTLINE_ON"))
            {
                text.fontMaterial.DisableKeyword("OUTLINE_ON");
            }
        }

        public static TextAlignmentOptions GetAlignment(string vTextAlign, string hTextAlign)
        {
            vTextAlign = vTextAlign.ToLower();
            hTextAlign = hTextAlign.ToLower();

            switch (vTextAlign)
            {
                case "top":
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.TopLeft;
                        case "right":
                            return TextAlignmentOptions.TopRight;
                        default:
                            return TextAlignmentOptions.Top;
                    }

                case "bottom":
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.BottomLeft;
                        case "right":
                            return TextAlignmentOptions.BottomRight;
                        default:
                            return TextAlignmentOptions.Bottom;
                    }

                default: // center
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.Left;
                        case "right":
                            return TextAlignmentOptions.Right;
                        default:
                            return TextAlignmentOptions.Center;
                    }
            }
        }

        private void ApplyCurrentModel()
        {
            ApplyModelChanges(scene, text, cachedModel);
        }

        private void PrepareRectTransform()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // NOTE: previously width and height weren't working (setting sizeDelta before anchors and offset result in
            // sizeDelta being reset to 0,0)
            // to fix textWrapping and avoid backwards compatibility issues as result of the size being properly set (like text alignment)
            // we only set it if textWrapping is enabled.
            if (cachedModel.textWrapping)
            {
                rectTransform.sizeDelta = new Vector2(cachedModel.width, cachedModel.height);
            }
            else
            {
                rectTransform.sizeDelta = Vector2.zero;
            }
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_TEXT_SHAPE;
        }
    }
}