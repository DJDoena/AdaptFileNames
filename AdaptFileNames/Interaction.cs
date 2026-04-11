using System;

namespace DoenaSoft.AdaptFileNames;

internal sealed class Interaction : IInteraction
{
    public void WriteLine(string message = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    public string ReadLine()
        => Console.ReadLine();
}