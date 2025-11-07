using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal static class Program
{
    private static FileType _fileType;

    private static IIOServices _ioServices;

    private static IRenameQueue _renameQueue;

    private static IPath Path
        => _ioServices.Path;

    private static int Main(string[] args)
    {
        Console.WriteLine($"v{Assembly.GetExecutingAssembly().GetName().Version}");

        _ioServices = new IOServices();

        string fileType;
        string folderName;
        if (args.Length != 2)
        {
            Console.WriteLine($"Invalid arg count: {args.Length}: {PrintArgs(args)}");
            Console.WriteLine("Expected:");
            Console.WriteLine("[0] = mp3/epub");
            Console.WriteLine("[1] = folder path");

            if (args.Length > 0 && (args[0] == "mp3" || args[1] == "epub"))
            {
                fileType = args[0];
            }
            else
            {
                do
                {
                    Console.WriteLine();
                    Console.WriteLine($"Enter file type:");
                    Console.WriteLine($"0: mp3");
                    Console.WriteLine($"1: epub");

                    fileType = Console.ReadLine().ToLowerInvariant();

                    if (fileType == "0")
                    {
                        fileType = "mp3";
                    }
                    else if (fileType == "1")
                    {
                        fileType = "epub";
                    }
                } while (fileType != "mp3" && fileType != "epub");
            }

            do
            {
                Console.WriteLine();
                Console.WriteLine($"Enter folder path:");

                folderName = Console.ReadLine().Trim().Trim('"');
            } while (!_ioServices.Folder.Exists(folderName));
        }
        else
        {
            fileType = args[0];
            folderName = args[1];
        }

        if (fileType != "mp3" && fileType != "epub")
        {
            Console.WriteLine("Invalid file type: " + fileType);
            Console.ReadLine();

            return -1;
        }
        else if (!_ioServices.Folder.Exists(folderName))
        {
            Console.WriteLine("Folder does not exist: " + folderName);
            Console.ReadLine();

            return -2;
        }

        if (fileType == "mp3")
        {
            _fileType = FileType.AudioBooks;
        }
        else if (fileType == "epub")
        {
            _fileType = FileType.EBooks;
        }

        try
        {
            _renameQueue = new RenameQueue(_ioServices);

            _renameQueue.Initialize();

            ProcessFolder(_ioServices.GetFolder(folderName));

            var count = _renameQueue.Commit();

            Console.WriteLine($"{count} files renamed.");
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

    private static string PrintArgs(string[] args)
        => string.Join(" ", args.Select(PrintArg));

    private static string PrintArg(string arg)
        => !string.IsNullOrEmpty(arg) && !arg.Contains(' ')
            ? arg
            : $"\"{arg}\"";

    private static void ProcessFolder(IFolderInfo folder)
    {
        var subFolders = folder.GetFolders("*.*", SIO.SearchOption.TopDirectoryOnly);

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

    private static void ProcessEBook(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.epub", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mobi", SIO.SearchOption.TopDirectoryOnly))
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
                _renameQueue.Add(file, Path.Combine(file.FolderName, $"{newName}{file.Extension}"));
            }
        }

        RenameCover(folder);
    }

    private static void RenameCover(IFolderInfo folder)
    {
        var coverfiles = folder.GetFiles("*.jpg", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.jpeg", SIO.SearchOption.TopDirectoryOnly));

        foreach (var file in coverfiles)
        {
            var targetName = "cover.jpg";

            if (!string.Equals(file.Name, targetName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, Path.Combine(file.FolderName, targetName));
            }
        }
    }

    private static void ProcessAudioBook(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.mp3", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mp4", SIO.SearchOption.TopDirectoryOnly))
            .OrderBy(fn => fn.Name)
            .ToList();

        if (files.Count == 1)
        {
            RenameMp3File(folder.Name, files[0], -1);
        }
        else
        {
            var chapterIndex = GetChapterIndex(files);

            for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, files.Count);

                var newName = $"{fileNumber} {folder.Name}";

                RenameMp3File(newName, files[fileIndex], chapterIndex);
            }
        }

        RenameCover(folder);

        RenamePdf(folder);

        RenameXmlFile(folder);
    }

    private static int GetChapterIndex(IEnumerable<IFileInfo> files)
    {
        var firstFileParts = files.First().NameWithoutExtension
            .Split('-')
            .Select(p => p.Trim())
            .ToList();

        int chapterIndex = -1;
        if (firstFileParts.Count > 1)
        {
            chapterIndex = -2;

            while (chapterIndex == -2)
            {
                Console.WriteLine("Select chapter index:");
                Console.WriteLine("-1: none");

                for (var partIndex = 0; partIndex < firstFileParts.Count; partIndex++)
                {
                    Console.WriteLine($"{partIndex}: {firstFileParts[partIndex]}");
                }

                var input = Console.ReadLine();

                if (int.TryParse(input, out chapterIndex))
                {
                    if (chapterIndex < -1 || chapterIndex >= firstFileParts.Count)
                    {
                        chapterIndex = -2;
                    }
                }
            }
        }

        return chapterIndex;
    }

    private static void RenameMp3File(string newName
        , IFileInfo file
        , int chapterIndex)
    {
        newName = newName.Trim();

        if (chapterIndex >= 0)
        {
            var fileParts = file.NameWithoutExtension
                .Split('-')
                .Select(p => p.Trim())
                .ToList();

            if (chapterIndex < fileParts.Count)
            {
                for (var subChapterIndex = chapterIndex; subChapterIndex < fileParts.Count; subChapterIndex++)
                {
                    newName = $"{newName}{AddChapter(fileParts[subChapterIndex])}";
                }
            }
        }

        newName = newName
            .Replace("  ", " ")
            .Replace("  ", " ");

        var oldName = file.NameWithoutExtension;

        if (oldName != newName)
        {
            _renameQueue.Add(file, Path.Combine(file.FolderName, $"{newName}{file.Extension}"));
        }
    }

    private static string AddChapter(string chapter)
    {
        if (string.IsNullOrWhiteSpace(chapter))
        {
            return string.Empty;
        }
        else
        {
            var cleaned = chapter
                .Trim()
                .Replace("_", " - ")
                .TrimEnd();

            return $" - {cleaned}";
        }
    }

    private static void RenamePdf(IFolderInfo folder)
    {
        var pdfFiles = folder.GetFiles("*.pdf", SIO.SearchOption.TopDirectoryOnly);

        foreach (var file in pdfFiles)
        {
            var targetName = $"{folder.Name}.pdf";

            if (!string.Equals(file.Name, targetName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, Path.Combine(file.FolderName, targetName));
            }
        }
    }

    private static void RenameXmlFile(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.xml", SIO.SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var newFileName = $"{folder.Name}{file.Extension}";

            if (!string.Equals(file.Name, newFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, Path.Combine(file.FolderName, newFileName));
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