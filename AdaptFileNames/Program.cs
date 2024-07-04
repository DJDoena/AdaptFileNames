using System;
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

        if (args.Length != 2)
        {
            Console.WriteLine($"Invalid arg count: {args.Length}: {PrintArgs(args)}");
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
        else if (!_ioServices.Folder.Exists(args[1]))
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
            _renameQueue = new RenameQueue(_ioServices);

            _renameQueue.StartRename();

            ProcessFolder(_ioServices.GetFolderInfo(args[1]));

            var count = _renameQueue.FinishRename();

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
        var subFolders = folder.GetDirectories("*.*", SIO.SearchOption.TopDirectoryOnly);

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
            if (!string.Equals(file.Name, "cover.jpg", StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, Path.Combine(file.FolderName, "cover.jpg"));
            }
        }
    }

    private static void ProcessAudioBook(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.mp3", SIO.SearchOption.TopDirectoryOnly)
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

    private static void RenameMp3File(string newName, IFileInfo file)
    {
        var oldName = Path.GetFileNameWithoutExtension(file.Name);

        if (oldName != newName)
        {
            _renameQueue.Add(file, Path.Combine(file.FolderName, $"{newName}{file.Extension}"));
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