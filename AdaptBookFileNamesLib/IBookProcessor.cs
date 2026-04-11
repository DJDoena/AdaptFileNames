using DoenaSoft.AbstractionLayer.IOServices;

namespace DoenaSoft.AdaptBookFileNames;

/// <summary>
/// Defines a processor for organizing and renaming book files within a folder.
/// </summary>
public interface IBookProcessor
{
    /// <summary>
    /// Processes all book files in the specified folder, renaming them according to the processor's rules.
    /// </summary>
    /// <param name="folder">The folder containing book files to process.</param>
    void Process(IFolderInfo folder);
}