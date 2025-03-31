using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner;

public static class MyStringExtensions
{
    /// <summary>
    /// This function extends the capablity of a standard <see cref="String.PadRight(int)"/> method, by allowing the function to break the input string
    /// by inserting the <paramref name="breakWith"/> string in between sections of <paramref name="str"/> that are no more than <paramref name="totalWidth"/> characters long.
    /// The sections are chosen so as to not split words into two.
    /// </summary>
    /// <param name="str">Input string</param>
    /// <param name="totalWidth"></param>
    /// <param name="breakWith"></param>
    /// <returns>The string <paramref name="str"/> delimited by <paramref name="breakWith"/> so that all sections fit inside <paramref name="totalWidth"/> characters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="totalWidth"/> is less than zero
    /// </exception>
    /// <exception cref="System.ArgumentException">Thrown when age is set to be less than 18 years.</exception>
    public static string PadRightWithBreaking(this string str, int totalWidth, string breakWith = "\n")
    {
        if (totalWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWidth));
        if (str.Length <=  totalWidth)
            return str.PadRight(totalWidth);
        string[] words = str.Replace('\n', ' ').Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        // We can't do anything about this, better format badly than crash the program.
        if ((words.MaxBy(x => x.Length)?.Length ?? 0) > totalWidth)
            return str;
        StringBuilder sb = new StringBuilder();
        int currentSize = 0;
        int wordInd = 0;
        while (wordInd < words.Length)
        {
            while (wordInd < words.Length && currentSize + words[wordInd].Length  <= totalWidth)
            {
                sb.Append(words[wordInd]);
                sb.Append(' ');
                currentSize += words[wordInd].Length + 1;
                wordInd++;
            }
            if (wordInd < words.Length)
            {
                currentSize = 0;
                sb.Append(breakWith);
            }
        }
        sb.Append(new string(' ', totalWidth - currentSize));
        return sb.ToString();
    }
}
