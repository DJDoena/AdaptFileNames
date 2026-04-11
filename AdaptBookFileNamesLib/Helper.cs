using DoenaSoft.AbstractionLayer.IOServices;
using System;
using System.Linq;
using SIO = System.IO;

namespace DoenaSoft.AdaptBookFileNames;

internal class Helper
{
    private readonly IRenameQueue _renameQueue;

    private readonly IPath _path;

    public Helper(IRenameQueue renameQueue
        , IPath path)
    {
        _renameQueue = renameQueue;
        _path = path;
    }

    internal void RenameCover(IFolderInfo folder)
    {
        var coverfiles = folder.GetFiles("*.jpg", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.jpeg", SIO.SearchOption.TopDirectoryOnly));

        foreach (var file in coverfiles)
        {
            var targetName = "cover.jpg";

            if (!string.Equals(file.Name, targetName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, targetName));
            }
        }
    }
}