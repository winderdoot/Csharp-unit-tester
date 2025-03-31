namespace MiniTest;

[AttributeUsage(AttributeTargets.Class)]
public class TestClassAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
public class TestMethodAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
public class BeforeEachAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
public class AfterEachAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
public class PriorityAttribute : Attribute
{
    public int Priority { get; set; }
    public PriorityAttribute(int priority)
    {
        Priority = priority;
    }
}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DataRowAttribute : Attribute
{
    public object?[] Data { get; set; }
    public string? Description { get; set; }
    public DataRowAttribute(params object?[] testData)
    {
        Data = testData;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; set;  }
    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}