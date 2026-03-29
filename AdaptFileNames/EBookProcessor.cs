using System;
using System.Linq;
using AdaptFileNames;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal sealed class EBookProcessor
{
    private readonly IRenameQueue _renameQueue;

    private readonly IPath _path;

    public EBookProcessor(IRenameQueue renameQueue
        , IPath path)
    {
        _renameQueue = renameQueue;
        _path = path;
    }

    internal void Process(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.epub", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mobi", SIO.SearchOption.TopDirectoryOnly))
            .ToList();

        if (files.Count is not 0 and not 2)
        {
            Console.WriteLine($"Check folder {folder.Name}");
        }

        foreach (var file in files)
        {
            var oldName = _path.GetFileNameWithoutExtension(file.Name);

            var newName = folder.Name;

            if (oldName != newName)
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, $"{newName}{file.Extension}"));
            }
        }

        (new Helper(_renameQueue, _path)).RenameCover(folder);
    }
}