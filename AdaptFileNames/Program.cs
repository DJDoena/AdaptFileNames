using System;
using System.Reflection;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal static class Program
{
    private static FileType _fileType;

    private static IIOServices _ioServices;

    private static IRenameQueue _renameQueue;

    private static int Main(string[] args)
    {
        Console.WriteLine($"v{Assembly.GetExecutingAssembly().GetName().Version}");

        _ioServices = new IOServices();

        string fileType;
        string folderName;
        if (args.Length != 2)
        {
            if (args.Length > 0 && (args[0] == "mp3" || args[0] == "epub"))
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
                } while (fileType is not "mp3" and not "epub");
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

        if (fileType is not "mp3" and not "epub")
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

    private static void ProcessFolder(IFolderInfo folder)
    {
        var subFolders = folder.GetFolders("*.*", SIO.SearchOption.TopDirectoryOnly);

        foreach (var subFolder in subFolders)
        {
            ProcessFolder(subFolder);
        }

        if (_fileType == FileType.EBooks)
        {
            (new EBookProcessor(_renameQueue, _ioServices.Path)).Process(folder);
        }
        else if (_fileType == FileType.AudioBooks)
        {
            (new AudioBookProcessor(_renameQueue, _ioServices.Path)).Process(folder);
        }
    }

    private enum FileType
    {
        Undefined,

        EBooks,

        AudioBooks,
    }
}