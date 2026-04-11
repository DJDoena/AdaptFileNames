namespace DoenaSoft.AdaptBookFileNames;

/// <summary>
/// Provides helper methods for generating file number strings with leading zeros based on the total file count.
/// </summary>
/// <remarks>This class is intended for scenarios where files need to be numbered consistently with zero-padding,
/// such as when generating filenames for ordered sequences. All members are static and the class cannot be
/// instantiated.</remarks>
public static class FileNumberHelper
{
    /// <summary>
    /// Generates a file number string with leading zeros based on the provided file index and total file count.
    /// </summary>
    public static string GetFileNumber(int fileIndex
        , int fileCount)
    {
        var padCount = GetDigitCount(fileCount);

        var fileNumber = (fileIndex + 1).ToString().PadLeft(padCount, '0');

        return fileNumber;
    }

    private static int GetDigitCount(int number)
    {
        var count = 0;

        while (number != 0)
        {
            number /= 10;

            count++;
        }

        return count;
    }
}