using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DoenaSoft.AdaptBookFileNames;

namespace DoenaSoft.AudioBookCDsToSingleFolder;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine($"v{Assembly.GetExecutingAssembly().GetName().Version}");

        if (Directory.Exists(args?.FirstOrDefault()))
        {
            TrySortRename(args[0]);
        }
        else
        {
            while (true)
            {
                Console.WriteLine("Enter CD path:");

                var path = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(path))
                {
                    TrySortRename(path);
                }
                else
                {
                    return;
                }
            }
        }
    }

    private static void TrySortRename(string root)
    {
        try
        {
            SortRename(root);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void SortRename(string root)
    {
        var mp3Count = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories).Length;

        var subFolders = Directory.GetDirectories(root, "*.*", SearchOption.TopDirectoryOnly).OrderBy(f => f);

        var fileIndex = 0;

        foreach (var subFolder in subFolders)
        {
            var mp3Files = Directory.GetFiles(subFolder, "*.mp3", SearchOption.TopDirectoryOnly).OrderBy(f => f);

            foreach (var mp3File in mp3Files)
            {
                var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, mp3Count);

                fileIndex++;

                File.Move(mp3File, Path.Combine(root, $"{fileNumber}.mp3"));
            }
        }
    }
}