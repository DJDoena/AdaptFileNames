using System.Linq;
using AdaptFileNames;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal sealed class EBookProcessor
{
    private readonly IRenameQueue _renameQueue;

    private readonly IPath _path;

    private readonly IInteraction _interaction;

    public EBookProcessor(IRenameQueue renameQueue
        , IPath path
        , IInteraction output)
    {
        _renameQueue = renameQueue;
        _path = path;
        _interaction = output;
    }

    internal void Process(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.epub", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mobi", SIO.SearchOption.TopDirectoryOnly))
            .ToList();

        if (files.Count is not 0 and not 2)
        {
            _interaction.WriteLine($"Check folder {folder.Name}");
        }

        _interaction.WriteLine($"Book name is set to '{folder.Name}. Enter new name here or simply press enter for taking folder name:");

        var bookName = _interaction.ReadLine();

        if (string.IsNullOrWhiteSpace(bookName))
        {
            bookName = folder.Name;
        }

        foreach (var file in files)
        {
            var oldName = _path.GetFileNameWithoutExtension(file.Name);

            var newName = bookName;

            if (oldName != newName)
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, $"{newName}{file.Extension}"));
            }
        }

        (new Helper(_renameQueue, _path)).RenameCover(folder);
    }
}