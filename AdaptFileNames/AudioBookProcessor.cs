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

    private readonly IInteraction _interaction;

    public AudioBookProcessor(IRenameQueue renameQueue
        , IPath path
        , IInteraction output)
    {
        _renameQueue = renameQueue;
        _path = path;
        _interaction = output;
    }

    internal void Process(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.mp3", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mp4", SIO.SearchOption.TopDirectoryOnly))
            .Concat(folder.GetFiles("*.m4a", SIO.SearchOption.TopDirectoryOnly))
            .OrderBy(fn => fn.Name)
            .ToList();

        _interaction.WriteLine($"Book name is set to '{folder.Name}. Enter new name here or simply press enter for takin folder name:");

        var bookName = _interaction.ReadLine();

        if (string.IsNullOrWhiteSpace(bookName))
        {
            bookName = folder.Name;
        }

        if (files.Count == 1)
        {
            this.RenameMp3File(bookName, files[0], -1);
        }
        else
        {
            var chapterIndex = this.GetChapterIndex(files);

            for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, files.Count);

                var newName = $"{fileNumber} {bookName}";

                this.RenameMp3File(newName, files[fileIndex], chapterIndex);
            }
        }

        (new Helper(_renameQueue, _path)).RenameCover(folder);

        this.RenameEBook(folder, bookName);

        this.RenameXmlFile(folder, bookName);
    }

    private void RenameMp3File(string bookName
      , IFileInfo file
      , int chapterIndex)
    {
        bookName = bookName.Trim();

        if (chapterIndex >= 0)
        {
            var fileParts = file.SplitAtDash();

            if (chapterIndex < fileParts.Count)
            {
                for (var subChapterIndex = chapterIndex; subChapterIndex < fileParts.Count; subChapterIndex++)
                {
                    bookName = $"{bookName}{AddChapter(fileParts[subChapterIndex])}";
                }
            }
        }

        bookName = bookName
            .Replace("   ", " ")
            .Replace("  ", " ");

        var oldName = file.NameWithoutExtension;

        if (oldName != bookName)
        {
            _renameQueue.Add(file, _path.Combine(file.FolderName, $"{bookName}{file.Extension}"));
        }
    }

    private int GetChapterIndex(IEnumerable<IFileInfo> files)
    {
        var firstFileParts = files.First().SplitAtDash();

        var chapterIndex = -1;
        if (firstFileParts.Count > 1)
        {
            chapterIndex = -2;

            while (chapterIndex == -2)
            {
                _interaction.WriteLine("Select chapter index:");
                _interaction.WriteLine("-1: none");

                for (var partIndex = 0; partIndex < firstFileParts.Count; partIndex++)
                {
                    _interaction.WriteLine($"{partIndex}: {firstFileParts[partIndex]}");
                }

                var input = _interaction.ReadLine();

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

    private void RenameEBook(IFolderInfo folder,
        string bookName)
    {
        var files = folder.GetFiles("*.pdf", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.epub", SIO.SearchOption.TopDirectoryOnly))
            .Concat(folder.GetFiles("*.mobi", SIO.SearchOption.TopDirectoryOnly));

        foreach (var file in files)
        {
            var targetName = $"{bookName}{file.Extension}";

            if (!string.Equals(file.Name, targetName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, targetName));
            }
        }
    }

    private void RenameXmlFile(IFolderInfo folder
        , string bookName)
    {
        var files = folder.GetFiles("*.xml", SIO.SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var newFileName = $"{bookName}{file.Extension}";

            if (!string.Equals(file.Name, newFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                _renameQueue.Add(file, _path.Combine(file.FolderName, newFileName));
            }
        }
    }
}