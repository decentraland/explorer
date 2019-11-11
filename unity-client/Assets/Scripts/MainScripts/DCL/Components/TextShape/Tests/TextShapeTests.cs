using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TextShapeTests : TestsBase
    {
        [UnityTest]
        public IEnumerator TestCreate()
        {
            yield return InitScene();

            string entityId = "e1";

            TestHelpers.CreateSceneEntity(scene, entityId);

            yield return null;

            var textShapeModel = new TextShape.Model()
            {
                value = "Hello world!",

                color = Color.white,
                opacity = 0.5f,
                fontSize = 10,
                fontWeight = "",

                width = 20,
                height = 20,
                adaptHeight = false,
                adaptWidth = false,
                paddingTop = 10,
                paddingRight = 0,
                paddingBottom = 10,
                paddingLeft = 0,

                shadowBlur = 0,
                shadowOffsetX = 0,
                shadowOffsetY = 0,
                shadowColor = Color.white
            };

            TextShape textShape =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, scene.entities[entityId],
                    textShapeModel);

            yield return textShape.routine;

            TextMeshPro tmpro = textShape.GetComponentInChildren<TextMeshPro>();

            Assert.IsTrue(textShape != null, "Component creation fail!");
            Assert.IsTrue(tmpro != null, "TextMeshPro doesn't exists for TextShape!");
            Assert.IsTrue(textShape.text != null, "Unity Text component doesn't exists for TextShape!");

            yield return null;

            TMProConsistencyAsserts(tmpro, textShapeModel);

            textShapeModel.paddingLeft = 5;
            textShapeModel.paddingRight = 15;
            textShapeModel.value = "Hello world again!";

            TextShape textShape2 =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, scene.entities[entityId],
                    textShapeModel);

            TMProConsistencyAsserts(tmpro, textShapeModel);

            yield return null;
        }

        void TMProConsistencyAsserts(TextMeshPro tmpro, TextShape.Model model)
        {
            Assert.AreEqual(model.paddingLeft, tmpro.margin[0], 0.01,
                string.Format("Left margin must be {0}", model.paddingLeft));
            Assert.AreEqual(model.paddingTop, tmpro.margin[1], 0.01,
                string.Format("Top margin must be {0}", model.paddingTop));
            Assert.AreEqual(model.paddingRight, tmpro.margin[2], 0.01,
                string.Format("Right margin must be {0}", model.paddingRight));
            Assert.AreEqual(model.paddingBottom, tmpro.margin[3], 0.01,
                string.Format("Bottom margin must be {0}", model.paddingBottom));

            Assert.IsTrue(tmpro.text == model.value, "Text wasn't set correctly!");
        }

        [UnityTest]
        public IEnumerator TestTextAlignment()
        {
            Assert.AreEqual(TextAlignmentOptions.BottomLeft, TextShape.GetAlignment("bottom", "left"));
            Assert.AreEqual(TextAlignmentOptions.BottomRight, TextShape.GetAlignment("Bottom", "RIght"));
            Assert.AreEqual(TextAlignmentOptions.Center, TextShape.GetAlignment("Center", "center"));
            Assert.AreEqual(TextAlignmentOptions.TopLeft, TextShape.GetAlignment("top", "Left"));
            Assert.AreEqual(TextAlignmentOptions.TopRight, TextShape.GetAlignment("top", "right"));
            Assert.AreEqual(TextAlignmentOptions.Top, TextShape.GetAlignment("top", "center"));
            Assert.AreEqual(TextAlignmentOptions.Right, TextShape.GetAlignment("center", "right"));
            Assert.AreEqual(TextAlignmentOptions.Bottom, TextShape.GetAlignment("Bottom", "cEnter"));
            Assert.AreEqual(TextAlignmentOptions.Left, TextShape.GetAlignment("center", "left"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            TextShape.Model textShapeModel = new TextShape.Model
            {
                color = Color.green,
                width = 0.25f,
                lineCount = 3,
                fontAutoSize = true,
                shadowColor = Color.red
            };

            TextShape textShapeComponent =
                TestHelpers.EntityComponentCreate<TextShape, TextShape.Model>(scene, scene.entities[entityId],
                    textShapeModel);

            yield return textShapeComponent.routine;

            // 2. Check configured values
            Assert.AreEqual(Color.green, textShapeComponent.model.color);
            Assert.AreEqual(0.25f, textShapeComponent.model.width);
            Assert.AreEqual(3, textShapeComponent.model.lineCount);
            Assert.IsTrue(textShapeComponent.model.fontAutoSize);
            Assert.AreEqual(Color.red, textShapeComponent.model.shadowColor);

            // 3. Update component with missing values
            scene.EntityComponentUpdate(scene.entities[entityId], CLASS_ID_COMPONENT.TEXT_SHAPE,
                JsonUtility.ToJson(new TextShape.Model { }));

            yield return textShapeComponent.routine;

            // 4. Check defaulted values
            Assert.AreEqual(Color.white, textShapeComponent.model.color);
            Assert.AreEqual(1f, textShapeComponent.model.width);
            Assert.AreEqual(0, textShapeComponent.model.lineCount);
            Assert.IsFalse(textShapeComponent.model.fontAutoSize);
            Assert.AreEqual(new Color(1, 1, 1), textShapeComponent.model.shadowColor);
        }
    }
}