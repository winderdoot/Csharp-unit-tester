using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using MiniTest;

namespace MiniTestRunner;

using static System.Runtime.InteropServices.JavaScript.JSType;
using Color = ConsoleColor;
/// <summary>
/// This class implements extremely overkill measures to provide an interface for console writing that is both needlesly general and yet very specific.
/// <list type="bullet">
///     <item>
///         It uses a custom <see langword="string"/> extension method <see cref="MyStringExtensions.PadRightWithBreaking(string, int, string)"/> for dynamic padding with custom indentation support.
///     </item>
///     <item>
///         It also uses the <see cref="Console.ForegroundColor"/> property for console coloring.
///     </item>
/// </list>
/// </summary>
public class ConsoleFormat
{
    private const string _defaultDescriptionMarker = "  =>  ";
    private const string _defaultDataRowMarker = " - ";

    private string _descriptionMarker;
    private string _dataRowMarker;

    public const Color TextDefault = Color.Gray;
    public int FieldWidth { get; set; }
    public Color TextNormal { get; set; }
    public Color TextPassed { get; set; }
    public Color TextFailed { get; set; }
    public Color TextException { get; set; }
    public Color TextDescription { get; set; }
    public Color TextSummary { get; set; }
    public Color TextAccent { get; set; }
    public ConsoleFormat(int fieldWidth, Color textNormal = TextDefault, Color textPassed = TextDefault,
                         Color textFailed = TextDefault, Color textDescription = TextDefault,
                         Color textSummary = TextDefault, Color textAccent = TextDefault, Color textException = TextDefault,
                         string descriptionMarker = _defaultDescriptionMarker, string dataRowMarker = _defaultDataRowMarker)
    {
        FieldWidth = fieldWidth;
        TextNormal = textNormal;
        TextPassed = textPassed;
        TextFailed = textFailed;
        TextDescription = textDescription;
        TextSummary = textSummary;
        TextAccent = textAccent;
        TextException = textException;
        _descriptionMarker = descriptionMarker;
        _dataRowMarker = dataRowMarker;
    }
    public void PrintClassSummary(int numTestsAll, int numTestsPassed)
    {
        Console.WriteLine();
        Console.ForegroundColor = TextSummary;
        string div1 = $"{numTestsPassed} / {numTestsAll}";
        string div2 = $"{numTestsAll - numTestsPassed} / {numTestsAll}";
        int maxWidth = Math.Max(div1.Length, div2.Length) + 5;
        string cap1 = "Tests passed:";
        string cap2 = "Tests failed:";
        string line1 = $" {cap1}{div1.PadLeft(maxWidth)}    ";
        string line2 = $" {cap2}{div2.PadLeft(maxWidth)}    ";
        Console.ForegroundColor = TextSummary;
        Console.WriteLine(new string('*', line1.Length + 2));
        Console.Write("*");
        Console.ForegroundColor = TextPassed;
        Console.Write(line1);
        Console.ForegroundColor = TextSummary;
        Console.WriteLine("*");
        Console.Write("*");
        Console.ForegroundColor = TextFailed;
        Console.Write(line2);
        Console.ForegroundColor = TextSummary;
        Console.WriteLine("*");
        Console.WriteLine(new string('*', line2.Length + 2));
        Console.ForegroundColor = TextDefault;
    }

    public void PrintClassHeader(Type testClass)
    {
        Console.ForegroundColor = TextAccent;
        Console.WriteLine(new string('=', $"Running tests from {testClass.FullName} class:".Length));
        Console.WriteLine($"Running tests from {testClass.FullName} class:");
        Console.WriteLine(new string('=', $"Running tests from {testClass.FullName} class:".Length));
        Console.ForegroundColor = TextDefault;
    }
    public void PrintFailed(MethodInfo method, DataRowAttribute? dataRow, Exception exception)
    {
        string label;
        if (dataRow?.Description is not null)
        {
            label = $"{_dataRowMarker}" +
                $"{dataRow.Description.PadRightWithBreaking
                        (
                            FieldWidth - _dataRowMarker.Length,
                            $"\n\t{new string(' ', _dataRowMarker.Length)}"
                        )}";
        }
        else
        {
            label = method.Name.PadRight(FieldWidth);
        }
        Console.ForegroundColor = TextFailed;
        Console.WriteLine($"\t{label}: FAILED");
        Console.ForegroundColor = TextException;
        string exceptionMessage = $"{_descriptionMarker}" +
                $"{exception.Message.PadRightWithBreaking
                        (
                            FieldWidth - _descriptionMarker.Length,
                            $"\n\t{new string(' ', _descriptionMarker.Length)}"
                        )}";
        Console.WriteLine($"\t{exceptionMessage}");
        Console.ForegroundColor = TextDefault;
    }
    public void PrintPassed(MethodInfo method, DataRowAttribute? dataRow)
    {
        string label;
        if (dataRow?.Description is not null)
        {
            label = $"{_dataRowMarker}" +
                $"{dataRow.Description.PadRightWithBreaking
                        (
                            FieldWidth - _dataRowMarker.Length,
                            $"\n\t{new string(' ', _dataRowMarker.Length)}"
                        )}";
        }
        else
        {
            label = method.Name.PadRight(FieldWidth);
        }
        Console.ForegroundColor = TextPassed;
        Console.WriteLine($"\t{label}: PASSED");
        Console.ForegroundColor = TextDefault;
    }
    public void PrintMethodHeader(MethodInfo method)
    {
        Console.ForegroundColor = TextDefault;
        Console.WriteLine($"\t{method.Name.PadRight(FieldWidth)}");
        Console.ForegroundColor = TextDefault;
    }
    public void PrintDescription(MethodInfo method)
    {
        MiniTest.DescriptionAttribute? desc = (MiniTest.DescriptionAttribute?)method.GetCustomAttribute(typeof(MiniTest.DescriptionAttribute), false);
        if (desc is null)
            return;
        Console.ForegroundColor = TextDescription;
        Console.WriteLine($"\t{_descriptionMarker}{desc.Description.PadRightWithBreaking(FieldWidth - _descriptionMarker.Length, $"\n\t{new string(' ', _descriptionMarker.Length)}")}");
        Console.ForegroundColor = TextDefault;
    }

    public static void TestColors()
    {
        Console.WriteLine("Test Colors:");
        foreach (Color color in Enum.GetValues(typeof(Color)))
        {
            Console.ForegroundColor = color;
            Console.WriteLine(color.ToString());
            Console.ForegroundColor = Color.Gray;
        }
    }

    public void PrintErrors(List<TestClassHandler.TestErrorInfo> errors)
    {
        Console.ForegroundColor = TextException;
        foreach (var error in errors)
        {
            Console.WriteLine($"Setup Error: {error.Message}" +
                $"{(error.Method is null ? ("") : ($"\nFrom test: {error.Method.Name}"))}" +
                $"{(error.CaughtException is null ? ("") : ($"\nException message: {error.CaughtException.Message}"))}");
        }
        Console.ForegroundColor = TextDefault;
        Console.WriteLine();
    }

    public void PrintInvalidDataRow(MethodInfo method, DataRowAttribute dataRow)
    {
        string label;
        if (dataRow?.Description is not null)
        {
            label = $"{_dataRowMarker}" +
                $"{dataRow.Description.PadRightWithBreaking
                        (
                            FieldWidth - _dataRowMarker.Length,
                            $"\n\t{new string(' ', _dataRowMarker.Length)}"
                        )}";
        }
        else
        {
            label = method.Name.PadRight(FieldWidth);
        }
        Console.ForegroundColor = TextException;
        Console.WriteLine($"\t{label}: INVALID DATA ROW");
        Console.ForegroundColor = TextDefault;
    }
}
