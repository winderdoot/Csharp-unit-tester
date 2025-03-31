using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTest;
/// <summary>
/// A custom exception class thrown upon encountering an assertion error in a unit test.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
