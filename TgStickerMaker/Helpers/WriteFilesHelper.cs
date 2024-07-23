namespace TgStickerMaker.Helpers
{
    public class WriteFilesHelper
    {
        public static string GetUniqueFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string uniqueFilePath = filePath;

            int counter = 1;
            while (File.Exists(uniqueFilePath))
            {
                uniqueFilePath = Path.Combine(directory, $"{filename} ({counter++}){extension}");
            }

            return uniqueFilePath;
        }
    }
}
