namespace DoenaSoft.AdaptBookFileNames;

internal static class FileNumberHelper
{
    internal static string GetFileNumber(int fileIndex, int fileCount)
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