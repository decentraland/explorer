using NUnit.Framework;

public class ParseOptionShould
{
    readonly string[] args = new string[] { "-unityRandomOption", "-testOption", "arg1", "arg2", "-garbage", "-garbage2" };

    [Test]
    public void FailWhenNoOptionsAreFound()
    {
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, null, 0, out string[] test));
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, "blah", 0, out string[] test2));
        Assert.IsTrue(test == null);
        Assert.IsTrue(test2 == null);
    }

    [Test]
    public void FailWhenTooManyArgumentsAreGiven()
    {
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, "testOption", 5, out string[] test5));
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, null, 5, out string[] test6));
        Assert.IsTrue(test5 == null);
        Assert.IsTrue(test6 == null);
    }

    [Test]
    public void NotCrashWhenInvalidArgsAreGiven()
    {
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(null, null, -1, out string[] test5));
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(null, "asdasdsad", -1, out string[] test6));
        Assert.IsFalse(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(null, "asdasdsad", int.MaxValue, out string[] test7));
    }

    [Test]
    public void SucceedWhenOptionsAreFound()
    {
        Assert.IsTrue(DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, "testOption", 0, out string[] test2));
        Assert.IsTrue(test2 == null);
    }

    [Test]
    public void SucceedExtractingArguments()
    {
        if (DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, "testOption", 1, out string[] test3))
        {
            Assert.IsTrue(test3 != null);
            Assert.IsTrue(test3.Length == 1);
            Assert.IsTrue(test3[0] == "arg1");
        }

        if (DCL.AssetBundleBuilderUtils.ParseOptionExplicit(args, "testOption", 2, out string[] test4))
        {
            Assert.IsTrue(test4 != null);
            Assert.IsTrue(test4.Length == 2);
            Assert.IsTrue(test4[0] == "arg1");
            Assert.IsTrue(test4[1] == "arg2");
        }
    }

}
