using System;
using System.IO;
using System.Linq;
using DoenaSoft.DownloadRenamer;

namespace DoenaSoft.AdaptFileNames;

internal static class Program
{
    private static FileType _fileType;

    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Invalid arg count: " + args.Length);
            Console.WriteLine("Expected:");
            Console.WriteLine("[0] = mp3/epub");
            Console.WriteLine("[1] = folder path");
            Console.ReadLine();

            return -1;
        }
        else if (args[0] != "mp3" && args[0] != "epub")
        {
            Console.WriteLine("Invalid file type: " + args[0]);
            Console.ReadLine();

            return -2;
        }
        else if (!Directory.Exists(args[1]))
        {
            Console.WriteLine("Folder does not exist: " + args[1]);
            Console.ReadLine();

            return -3;
        }

        if (args[0] == "mp3")
        {
            _fileType = FileType.AudioBooks;
        }
        if (args[0] == "epub")
        {
            _fileType = FileType.EBooks;
        }

        try
        {
            RenameQueue.StartRename();

            ProcessFolder(new DirectoryInfo(args[1]));

            RenameQueue.FinishRename();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        Console.WriteLine("Press <Enter> to exit.");
        Console.ReadLine();

        return 0;
    }

    private static void ProcessFolder(DirectoryInfo folder)
    {
        var subFolders = folder.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

        foreach (var subFolder in subFolders)
        {
            ProcessFolder(subFolder);
        }

        if (_fileType == FileType.EBooks)
        {
            ProcessEBook(folder);
        }
        else if (_fileType == FileType.AudioBooks)
        {
            ProcessAudioBook(folder);
        }
    }

    private static void ProcessEBook(DirectoryInfo folder)
    {
        var files = folder.GetFiles("*.epub", SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mobi", SearchOption.TopDirectoryOnly))
            .ToList();

        if (files.Count != 0 && files.Count != 2)
        {
            Console.WriteLine($"Check folder {folder.Name}");
        }

        foreach (var file in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(file.Name);

            var newName = folder.Name;

            if (oldName != newName)
            {
                RenameQueue.TryAdd(file, Path.Combine(file.DirectoryName, $"{newName}{file.Extension}"));
            }
        }

        RenameCover(folder);
    }

    private static void RenameCover(DirectoryInfo folder)
    {
        var coverfiles = folder.GetFiles("*.jpg", SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.jpeg", SearchOption.TopDirectoryOnly));

        foreach (var file in coverfiles)
        {
            if (file.Name != "cover.jpg")
            {
                RenameQueue.TryAdd(file, Path.Combine(file.DirectoryName, "cover.jpg"));
            }
        }
    }

    private static void ProcessAudioBook(DirectoryInfo folder)
    {
        var files = folder.GetFiles("*.mp3", SearchOption.TopDirectoryOnly)
            .OrderBy(fn => fn.Name)
            .ToList();

        if (files.Count == 1)
        {
            RenameMp3File(folder.Name, files[0]);
        }
        else
        {
            for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, files.Count);

                var newName = $"{fileNumber} {folder.Name}";

                RenameMp3File(newName, files[fileIndex]);
            }
        }

        RenameCover(folder);

        RenameXmlFile(folder);
    }

    private static void RenameMp3File(string newName, FileInfo file)
    {
        var oldName = Path.GetFileNameWithoutExtension(file.Name);

        if (oldName != newName)
        {
            RenameQueue.TryAdd(file, Path.Combine(file.DirectoryName, $"{newName}{file.Extension}"));
        }
    }

    private static void RenameXmlFile(DirectoryInfo folder)
    {
        var files = folder.GetFiles("*.xml", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var newFileName = $"{folder.Name}{file.Extension}";

            if (file.Name != newFileName)
            {
                RenameQueue.TryAdd(file, Path.Combine(file.DirectoryName, newFileName));
            }
        }
    }

    private enum FileType
    {
        Undefined,

        EBooks,

        AudioBooks,
    }
}