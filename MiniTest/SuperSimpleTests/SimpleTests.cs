using MiniTest;

namespace SuperSimpleTests;

[TestClass]
public class SimpleTests
{
    [TestMethod]
    [Description("This is a boilerplate test that will always fail")]
    public void TestThatWillAlwaysFail()
    {
        Assert.Fail();
    }
}

[TestClass]
public class TestWithNoConstructor
{
    private TestWithNoConstructor()
    { }

    [TestMethod]
    public void ShouldNotEvenRun()
    {  
        Assert.Fail(); 
    }
}

[TestClass]
public class TestClassCase1
{
    [TestMethod]
    public bool BadTestWithNonVoidReturn()
    {
        return false;
    }

    [TestMethod]
    [Priority(10)]
    [DataRow(1, Description = "Integer instead of string")]
    public void TestWithInvalidDataRow(string x)
    {
        Assert.IsTrue(x == "Funny string");
    }

    [TestMethod]
    [DataRow(2137, Description = "Random number that has no meaning")]
    public void TestWithParamsShouldPass(int x)
    {
        Assert.IsTrue(x.GetType() == typeof(int));
    }

    [TestMethod]
    public void TestThatHasArgumentDespiteNoDataRow(int x)
    {
        Assert.Fail();
    }

    [TestMethod]
    [Description("This just always fails but is there to show that other tests will run normally despite some setup errors")]
    public void NormalTestThatIsOkButFails()
    {
        Assert.Fail();
    }
}