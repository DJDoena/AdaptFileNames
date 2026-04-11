using DoenaSoft.AbstractionLayer.IOServices;
using System.Linq;
using SIO = System.IO;

namespace DoenaSoft.AdaptBookFileNames;

/// <summary>
/// Processes e-book files (EPUB, MOBI) in folders, renaming them with consistent naming conventions based on the book name.
/// </summary>
public sealed class EBookProcessor : IBookProcessor
{
    private readonly IRenameQueue _renameQueue;

    private readonly IPath _path;

    private readonly IInteraction _interaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="EBookProcessor"/> class.
    /// </summary>
    /// <param name="renameQueue">The rename queue for queueing file rename operations.</param>
    /// <param name="path">The path service for file path operations.</param>
    /// <param name="output">The interaction service for user input and output.</param>
    public EBookProcessor(IRenameQueue renameQueue
        , IPath path
        , IInteraction output)
    {
        _renameQueue = renameQueue;
        _path = path;
        _interaction = output;
    }

    /// <summary>
    /// Processes all e-book files in the specified folder, renaming them to match the book name while preserving their file extensions.
    /// Also renames associated cover images in the same folder.
    /// </summary>
    /// <param name="folder">The folder containing e-book files to process.</param>
    public void Process(IFolderInfo folder)
    {
        var files = folder.GetFiles("*.epub", SIO.SearchOption.TopDirectoryOnly)
            .Concat(folder.GetFiles("*.mobi", SIO.SearchOption.TopDirectoryOnly))
            .ToList();

        if (files.Count is not 0 and not 2)
        {
            _interaction.WriteLine($"Check folder {folder.Name}");
        }

        _interaction.WriteLine($"Book name is set to \"{folder.Name}\". Enter new name here or simply press enter for taking folder name:");

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