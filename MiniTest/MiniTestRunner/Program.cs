using MiniTest;
using System.Reflection;
using System.Runtime.Loader;

namespace MiniTestRunner;

/// <remarks>
/// The author of this project really hopes that his pretty formatting with wrapping and proper indentation can score him some extra points, beacuse it was <i>Hell</i> <see href="https://en.wikipedia.org/wiki/Hell"/>.
/// For the program to work, you have to supply a correct path to an .dll file that was compiled by VisualStudio and is in it's original place.
/// I am giving the input arguments in DebugProperties - <b>attempting to pass them through the console will not work, unless the debug properties are changed!</b>
/// </remarks>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("MiniTestRunner:");

        foreach (string path in args)
        {
            Console.WriteLine($"Running test: {path}...");
            try
            {
                MyContextLoader context = new MyContextLoader(Path.GetFullPath(path));
                Assembly asm = context.LoadFromAssemblyPath(Path.GetFullPath(path));
                Type[] TestClasses = asm
                    .GetTypes()
                    .Where(t => t.GetCustomAttribute(typeof(TestClassAttribute), false) != null)
                    .ToArray();
                foreach (var TestClass in TestClasses)
                {
                    Console.WriteLine();
                    TestClassHandler TestHandler = new(TestClass);
                    TestHandler.PrintClassHeader();
                    TestHandler.PrintSetupErrors();
                    try
                    {
                        TestHandler.RunAllTests();
                    }
                    catch (TestHandlerException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Failed to load {path} assembly");
            }
        }

    }
}