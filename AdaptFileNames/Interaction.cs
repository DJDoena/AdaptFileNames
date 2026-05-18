using System;
using DoenaSoft.AbstractionLayer.UI.Contracts;

namespace DoenaSoft.AdaptBookFileNames;

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

    public void Write(string message)
        => Console.Write(message);

    public (char key, KeyModifiers modifiers) ReadKey(bool intercept)
    {
        var key = Console.ReadKey(intercept);

        return (key.KeyChar, (KeyModifiers)key.Modifiers);
    }
}