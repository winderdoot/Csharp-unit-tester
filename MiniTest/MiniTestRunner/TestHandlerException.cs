using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner;
/// <summary>
/// This class defines a custom exception that is thrown when a prep method executed by <see cref="TestClassHandler"/> throws an exception. This exception stops execution of all
/// </summary>
public class TestHandlerException : Exception
{
    public override string Message { get; }
    public TestHandlerException(string message, Type testClass, MethodInfo? method) : base(message)
    {
        Message = $"CRITICAL ==> " + base.Message + $"\n\tFaulty Test Class: {testClass.Name}\n\tFaulty Test: {method?.Name ?? "<none>"}";
    }
}
