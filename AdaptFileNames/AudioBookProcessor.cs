using System;
using System.Collections.Generic;
using System.Linq;
using AdaptFileNames;
using DoenaSoft.AbstractionLayer.IOServices;
using SIO = System.IO;

namespace DoenaSoft.AdaptFileNames;

internal sealed class AudioBookProcessor
{
    private readonly IRenameQueue _renameQueue;

    private readonly IPath _path;

    public AudioBookProcessor(IRenameQueue renameQueue
        , IPath path)
    {
        _renameQueue = renameQueue;
        _path = path;
    }

    internal void Process(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.mp3", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mp4", SIO.SearchOption.TopDirectoryOnly))
            .OrderBy(fn => fn.Name)
            .ToList();

        if (files.Count == 1)
        {
            this.RenameMp3File(folder.Name, files[0], -1);
        }
        else
        {
            var chapterIndex = GetChapterIndex(files);

            for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, files.Count);

                var newName = $"{fileNumber} {folder.Name}";

                this.RenameMp3File(newName, files[fileIndex], chapterIndex);
            }
        }

        (new Helper(_renameQueue, _path)).RenameCover(folder);

        this.RenamePdf(folder);

        this.RenameXmlFile(folder);
    }

    private void RenameMp3File(string newName
      , IFileInfo file
      , int chapterIndex)
    {
        newName = newName.Trim();

        if (chapterIndex >= 0)
        {
            var fileParts = file.SplitAtDash();

            if (chapterIndex < fileParts.Count)
            {
                for (var subChapterIndex = chapterIndex; subChapterIndex < fileParts.Count; subChapterIndex++)
                {
                    newName = $"{newName}{AddChapter(fileParts[subChapterIndex])}";
                }
            }
        }

        newName = newName
            .Replace("   ", " ")
            .Replace("  ", " ");

        var oldName = file.NameWithoutExtension;

        if (oldName != newName)
        {
            _renameQueue.Add(file, _path.Combine(file.FolderName, $"{newName}{file.Extension}"));
        }
    }

    private static int GetChapterIndex(IEnumerable<IFileInfo> files)
    {
        var firstFileParts = files.First().SplitAtDash();

        var chapterIndex = -1;
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

    private void RenamePdf(IFolderInfo folder)
    {
        var pdfFiles = folder.GetFiles("*.pdf", SIO.SearchOption.TopDirectoryOnly);

        foreach (var file in pdfFiles)
        {
            var targetName = $"{folder.Name}.pdf";

            if (!string.Equals(file.Name, targetName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, targetName));
            }
        }
    }

    private void RenameXmlFile(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.xml", SIO.SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var newFileName = $"{folder.Name}{file.Extension}";

            if (!string.Equals(file.Name, newFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, newFileName));
            }
        }
    }
}