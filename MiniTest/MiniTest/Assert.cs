using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTest;
/// <summary>
/// Provides a set of assertion methods that throw an AssertionException, if they fail. Otherwise, nothing happens.
/// </summary>
public static class Assert
{
    public static void ThrowsException<TException>(Action action, string message = "") where TException : Exception
    {
        try 
        { 
            action();
            throw new AssertionException($"Expected exception type: <{typeof(TException)}> but no exception was thrown. {message}");
        }
        catch (TException)
        { 
            return;
        }
        catch (Exception e)
        {
            throw new AssertionException($"Expected exception type: <{typeof(TException)}>. Actual exception type <{e.GetType()}>. {message}"); 
        }
            
    }

    public static void AreEqual<T>(T? expected, T? actual, string message = "") where T : IEquatable<T>
    {
        if (expected is null)
        {
            if (actual is not null)
                throw new AssertionException($"Excpected: <{expected?.ToString() ?? "null"}>. Actual: <{actual?.ToString() ?? "null"}>. {message}");
            return;
        }
        if (actual is null)
            throw new AssertionException($"Excpected: <{expected?.ToString() ?? "null"}>. Actual: <{actual?.ToString() ?? "null"}>. {message}");
        if (!expected.Equals(actual))
            throw new AssertionException($"Excpected: <{expected?.ToString() ?? "null"}>. Actual: <{actual?.ToString() ?? "null"}>. {message}");
    }

    public static void AreNotEqual<T>(T? notExcpected, T? actual, string message = "") where T : IEquatable<T>
    {
        if (notExcpected is null)
        {
            if (actual is null)
                throw new AssertionException($"Excpected any value expect: <{notExcpected?.ToString() ?? "null"}>. Actual: <{actual?.ToString() ?? "null"}>. {message}");
            return;
        }
        if (actual is null)
            return;
        if (notExcpected.Equals(actual))
            throw new AssertionException($"Excpected any value expect: <{notExcpected?.ToString() ?? "null"}>. Actual: <{actual?.ToString() ?? "null"}>. {message}");
    }
    public static void IsTrue(bool condition, string message = "")
    {
        if (!condition)
            throw new AssertionException($"Condintion expected to be true. Found to be false. {message}");
    }
    public static void IsFalse(bool condition, string message = "")
    {
        if (condition)
            throw new AssertionException($"Condintion expected to be false. Found to be true. {message}");
    }
    public static void Fail(string message = "")
    {
        throw new AssertionException($"Unconditional assertion fail. {message}");
    }
}
