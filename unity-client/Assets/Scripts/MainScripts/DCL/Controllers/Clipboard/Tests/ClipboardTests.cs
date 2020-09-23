using System.Collections;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ClipboardTests : TestsBase
{
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield break;
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        yield break;
    }

    [UnityTest]
    public IEnumerator ReadClipboardPromiseShouldBehaveCorrectly()
    {
        ClipboardHandler_Mock mockClipboardHandler = new ClipboardHandler_Mock();
        Clipboard clipboard = new Clipboard(mockClipboardHandler);

        const string firstText = "sometext";
        mockClipboardHandler.MockReadTextRequestResult(0.5f,firstText,false);

        var promise = clipboard.ReadText();
        yield return promise;

        Assert.IsNull(promise.error);
        Assert.IsNotNull(promise.value);
        Assert.IsTrue(promise.value == firstText);

        const string errorText = "errortext";
        mockClipboardHandler.MockReadTextRequestResult(0,errorText,true);
        promise = clipboard.ReadText();

        Assert.IsNull(promise.value);
        Assert.IsNotNull(promise.error);
        Assert.IsTrue(promise.error == errorText);
    }
}