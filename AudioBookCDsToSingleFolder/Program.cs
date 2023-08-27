using System.IO;
using System.Linq;
using DoenaSoft.AdaptFileNames;

namespace DoenaSoft.AudioBookCDsToSingleFolder
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var root = args[0];

            var mp3Count = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories).Length;

            var subFolders = Directory.GetDirectories(root, "*.*", SearchOption.TopDirectoryOnly).OrderBy(f => f);

            var fileIndex = 0;

            foreach (var subFolder in subFolders)
            {
                var mp3Files = Directory.GetFiles(subFolder, "*.mp3", SearchOption.TopDirectoryOnly).OrderBy(f => f);

                foreach (var mp3File in mp3Files)
                {
                    var fileNumber = FileNumberHelper.GetFileNumber(fileIndex, mp3Count);

                    fileIndex++;

                    File.Move(mp3File, Path.Combine(root, $"{fileNumber}.mp3"));
                }
            }
        }
    }
}