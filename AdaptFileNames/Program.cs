using DoenaSoft.AbstractionLayer.IOServices;
using System;
using System.Reflection;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal static class Program
{
    private static IInteraction _interaction;

    private static FileType _fileType;

    private static IIOServices _ioServices;

    private static IRenameQueue _renameQueue;

    private static int Main(string[] args)
    {
        _interaction = new Interaction();

        _interaction.WriteLine($"v{Assembly.GetExecutingAssembly().GetName().Version}");

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
                    _interaction.WriteLine();
                    _interaction.WriteLine($"Enter file type:");
                    _interaction.WriteLine($"0: mp3");
                    _interaction.WriteLine($"1: epub");

                    fileType = _interaction.ReadLine().ToLowerInvariant();

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
                _interaction.WriteLine();
                _interaction.WriteLine($"Enter folder path:");

                folderName = _interaction.ReadLine().Trim().Trim('"');
            } while (!_ioServices.Folder.Exists(folderName));
        }
        else
        {
            fileType = args[0];
            folderName = args[1];
        }

        if (fileType is not "mp3" and not "epub")
        {
            _interaction.WriteLine("Invalid file type: " + fileType);
            _interaction.ReadLine();

            return -1;
        }
        else if (!_ioServices.Folder.Exists(folderName))
        {
            _interaction.WriteLine("Folder does not exist: " + folderName);
            _interaction.ReadLine();

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

            _interaction.WriteLine($"{count} files renamed.");
        }
        catch (Exception ex)
        {
            _interaction.WriteLine(ex.Message);

            if (ex.InnerException != null)
            {
                _interaction.WriteLine(ex.InnerException.Message);
            }
        }

        _interaction.WriteLine("Press <Enter> to exit.");
        _interaction.ReadLine();

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
            (new EBookProcessor(_renameQueue, _ioServices.Path, _interaction)).Process(folder);
        }
        else if (_fileType == FileType.AudioBooks)
        {
            (new AudioBookProcessor(_renameQueue, _ioServices.Path, _interaction)).Process(folder);
        }
    }

    private enum FileType
    {
        Undefined,

        EBooks,

        AudioBooks,
    }
}