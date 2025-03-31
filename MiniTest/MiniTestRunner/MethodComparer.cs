using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTest;

namespace MiniTestRunner;

/// <summary>
/// This class implements the <see cref="IComparer{(int, string)}"/> interface, and allows comparisons
/// between test methods. 
/// </summary>
/// <remarks>
/// The comparison is based on:
/// <list type="bullet">
///    <item>
///         firstly the <see cref="PriorityAttribute"/> attribute (if a method doesn't have it priority 0 is assumed)
///    </item>
///    <item>
///         secondly the name of the test method (alphabetical order)
///    </item>
///</list>
///</remarks>
public class MethodComparer : IComparer<(int, string)>
{
    public int Compare((int, string) x, (int, string) y)
    {
        if (x.Item1 != y.Item1)
            return x.Item1.CompareTo(y.Item1);
        return x.Item2.CompareTo(y.Item2);
    }
}
