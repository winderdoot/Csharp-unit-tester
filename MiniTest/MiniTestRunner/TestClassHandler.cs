using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MiniTest;

namespace MiniTestRunner;
using Color = ConsoleColor;

/// <summary>
/// This class acquires test methods provided by a test class and handles their execution.
/// </summary>
/// <remarks>
/// Test methods are only considered if:
/// <list type="bullet">
///    <item>
///         their return type is <see cref="void"></see>
///    </item>
///    <item>
///         they have the attribute <see cref="TestMethodAttribute"></see>
///    </item>
///</list>
///</remarks>
public class TestClassHandler
{
    public struct TestErrorInfo
    { 
        public string Message { get; set; }
        public MethodInfo? Method { get; set; }
        public Exception? CaughtException { get; set; }
        public TestErrorInfo(string message, MethodInfo? method = null, Exception? caughtException = null)
        {
            Message = message;
            Method = method;
            CaughtException = caughtException;
        }
    }

    public Type TestClass {  get; }

    private MethodInfo[] _testsNoParams;
    private MethodInfo[] _testsWithParams;
    private MethodInfo[] _prepMethods;
    private MethodInfo[] _postMethods;

    private Action[] _prepDelegates = [];
    private Action[] _postDelegates = [];
    private object? _myTestClassInstance;
    private List<TestErrorInfo> _testErrors = [];

    /// <summary>
    /// Gets a key that is used to compare methods.
    /// </summary>
    /// <param name="meth"> <see cref="MethodInfo"/> object that represents a method's metadata</param>
    /// <returns>
    /// Key of type (<see cref="int"/>,<see cref="string"/>) that is used by <see cref="MethodComparer"/> to compare methods.
    /// </returns>
    private static (int, string) GetMethodKey(MethodInfo meth)
    {
        int? priority = (int?) meth
                .GetCustomAttribute(typeof(PriorityAttribute), false)
                ?.GetType()
                .GetProperty(nameof(PriorityAttribute.Priority))
                ?.GetValue(meth.GetCustomAttribute(typeof(PriorityAttribute), false));
        return priority switch
        {
            int => (priority.Value, meth.Name),
            null => (0, meth.Name)
        };
    }

    private void CheckTestForErrors(MethodInfo meth, bool hasArguments, bool isTest = true)
    {
        if (meth.ReturnType != typeof(void))
        {
            _testErrors.Add(new TestErrorInfo($"Invalid return type - expected: void, found: {meth.ReturnType}", meth));
        }
        if (hasArguments && meth.GetParameters().Length < 1)
        {
            _testErrors.Add(new TestErrorInfo($"Invalid argument count - expected: > 0, found: {meth.GetParameters().Length}", meth));
        }
        else if (!hasArguments && meth.GetParameters().Length != 0)
        {
            _testErrors.Add(new TestErrorInfo($"Invalid argument count - expected: 0, found: {meth.GetParameters().Length}", meth));
        }
        if (!isTest && meth.GetCustomAttributes(typeof(DataRowAttribute), false) != null)
        {
            _testErrors.Add(new TestErrorInfo($"Invalid attribute - {typeof(DataRowAttribute)} cannot be set on prep/post methods", meth));
        }
    }

    public TestClassHandler(Type testClass)
    {
        TestClass = testClass;
        _testsNoParams = TestClass
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(typeof(TestMethodAttribute), false) != null)
            .Where(m => m.GetCustomAttributes(typeof(DataRowAttribute), false).Length == 0)
            .Select(m => { CheckTestForErrors(m, hasArguments: false); return m; })
            .Where(m => m.ReturnType == typeof(void))
            .Where(m => m.GetParameters().Length == 0)
            .OrderBy(GetMethodKey, new MethodComparer())
            .ToArray();

        _testsWithParams = TestClass
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(typeof(TestMethodAttribute), false) != null)
            .Where(m => m.GetCustomAttributes(typeof(DataRowAttribute), false).Length > 0)
            .Select(m => { CheckTestForErrors(m, hasArguments: true); return m; })
            .Where(m => m.ReturnType == typeof(void))
            .OrderBy(GetMethodKey, new MethodComparer())
            .ToArray();

        _prepMethods = TestClass
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(typeof(BeforeEachAttribute), false) != null)
            .Where(m => m.GetParameters().Length == 0)
            .OrderBy(GetMethodKey, new MethodComparer())
            .ToArray();

        _postMethods = TestClass
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(typeof(AfterEachAttribute), false) != null)
            .Where(m => m.GetParameters().Length == 0)
            .OrderBy(GetMethodKey, new MethodComparer())
            .ToArray();
        
        BindMyDelegates();
    }
    private void BindMyDelegates()
    {
        try
        {
            _myTestClassInstance = Activator.CreateInstance(TestClass);
        }
        catch (Exception e)
        {
            _testErrors.Add(new TestErrorInfo($"", null, e));
            _myTestClassInstance = null;
            return;
        }
        _prepDelegates = _prepMethods.Select(m => (Action)Action.CreateDelegate(typeof(Action), _myTestClassInstance, m)).ToArray();
        _postDelegates = _postMethods.Select(m => (Action)Action.CreateDelegate(typeof(Action), _myTestClassInstance, m)).ToArray();
    }

    public void PrintClassHeader()
    {
        ConsoleFormat formatter = new ConsoleFormat(fieldWidth: TestClass.FullName!.Length, textNormal: Color.Gray, textPassed: Color.DarkGreen,
                                                  textFailed: Color.DarkRed, textException: Color.Red, textDescription: Color.DarkYellow,
                                                  textAccent: Color.Cyan, textSummary: Color.Yellow);
        formatter.PrintClassHeader(TestClass);
    }

    /// <summary>
    /// This method is meant to be run before <see cref="RunAllTests"/> in order to inform the user of any test-related errors that occured while acquiring the test methods.
    /// </summary>
    public void PrintSetupErrors()
    {
        if (_testErrors.Count() < 1)
            return;
        ConsoleFormat formatter = new ConsoleFormat(fieldWidth: _testErrors.MaxBy(e => e.Method?.Name.Length).Method?.Name.Length ?? 30,
                                                    textException: Color.Red);
        formatter.PrintErrors(_testErrors);
    }
    public void RunAllTests()
    {
        if (_myTestClassInstance is null)
        {
            throw new TestHandlerException("TestHandlerException: no parameterless constructor found for class", TestClass, null);
        }    
        int numTests = _testsNoParams.Length;
        int numPassed = 0;
        MethodInfo[] allMeths = _testsNoParams.Concat(_testsWithParams).ToArray();
        int? maxMethodName = allMeths
            ?.Select(m => m.Name.Length)
            ?.Max();
        if (maxMethodName is null)
            return;

        ConsoleFormat formatter = new ConsoleFormat(fieldWidth: (int)maxMethodName, textNormal: Color.Gray, textPassed: Color.DarkGreen,
                                                  textFailed: Color.DarkRed, textException: Color.Red, textDescription: Color.DarkYellow,
                                                  textAccent: Color.Cyan, textSummary: Color.Yellow);

        // Parameterless test methods
        foreach (var method in _testsNoParams)
        {
            RunPrepMethods(method);
            if (SingleTestPassed(method, dataRow: null, formatter))
            {
                numPassed++;
            }
            formatter.PrintDescription(method);
            RunPostMethods(method);
        }

        // Test methods with parameters(DataRowAttribute must be present)
        foreach (var method in _testsWithParams)
        {
            formatter.PrintMethodHeader(method);
            DataRowAttribute[] dataRows = method
                .GetCustomAttributes(typeof(DataRowAttribute), false)
                .Select(a => (DataRowAttribute)a)
                .Select(a => { ProcessInvalidDataRows(a, method, formatter); return a; })
                .Where(a => ParameterInfoMatchesDataRow(method.GetParameters(), a.Data))
                .ToArray();

            numTests += dataRows.Length;
            foreach (var dataRow in dataRows)
            {
                RunPrepMethods(method);
                if(SingleTestPassed(method, dataRow: dataRow, formatter))
                {
                    numPassed++;
                }
                RunPostMethods(method);
            }
            formatter.PrintDescription(method);
        }

        formatter.PrintClassSummary(numTests, numPassed);
    }

    private static void ProcessInvalidDataRows(DataRowAttribute a, MethodInfo method, ConsoleFormat formatter)
    {
        if (!ParameterInfoMatchesDataRow(method.GetParameters(), a.Data))
        {
            formatter.PrintInvalidDataRow(method, a);
        }
    }

    private bool SingleTestPassed(MethodInfo method, DataRowAttribute? dataRow, ConsoleFormat formatter)
    {
        try
        {
            method.Invoke(_myTestClassInstance, dataRow?.Data);
            formatter.PrintPassed(method, dataRow);
            return true;
        }
        catch (TargetInvocationException e)
        {
            formatter.PrintFailed(method, dataRow, e.InnerException!);
            return false;
        }
    }

    private void RunPrepMethods(MethodInfo method)
    {
        try
        {
            foreach (var prepMethod in _prepDelegates)
                prepMethod();
        }
        catch (Exception e)
        {
            throw new TestHandlerException($"TestHandlerException: - a prep method threw an exception: {e.Message}", TestClass, method);
        }
    }

    private void RunPostMethods(MethodInfo method)
    {
        try
        {
            foreach (var postMethod in _postDelegates)
                postMethod();
        }
        catch (Exception e)
        {
            throw new TestHandlerException($"TestHandlerException: - a post method threw an exception: {e.Message}", TestClass, method);
        }
    }
    private static bool ParameterInfoMatchesDataRow(ParameterInfo[] parameters, object?[] data)
    {
        if (parameters.Length != data.Length)
            return false;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (data[i] is null)
            {
                if (parameters[i].ParameterType.IsValueType)
                    return false;
            }
            else if (parameters[i].ParameterType.IsValueType)
            {
                if (!parameters[i].ParameterType.IsAssignableFrom(data[i]?.GetType()))
                    return false;
            }
            else if (parameters[i].ParameterType != data[i]?.GetType())
                return false;
        }
        return true;
    }

    // Unused - debugging purpose only
    public void PrintMethods()
    {
        Console.WriteLine(TestClass.FullName);
        foreach (var TestMethod in _testsNoParams)
        {
            Console.WriteLine(TestMethod?.Name ?? "null");
        }
        Console.WriteLine();
    }
}
