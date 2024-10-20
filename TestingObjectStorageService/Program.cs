using CloudRu.ObjectStorageHelper;
using SimpleLogger;



namespace TestingObjectStorageService
{



    internal class Program
    {
        static async Task Main(string[] args)
        {
            var options = new SimpleLoggerOptions
            {
                LoggingType = LoggingType.FileAndConsole
            };

            var logger = new Logger();

            var objStorage = new ObjectStorageService(
                                        accessKey: "b6307ba7-3a26-4a17-9c25-969fe7c90b5c:8ce22f6e06ef822c0855cc5ca618f4a6", 
                                        secretKey: "ad1d26d43ca225af1ada9ccc171e29f5",
                                        bucketName: "mac-bot-cards");

            var folders = await objStorage.ListFoldersAsync();

            Console.WriteLine("Folders: ");

            foreach (var folder in folders)
            {
                Console.WriteLine(folder);
            }

            Console.WriteLine("Files: ");

            foreach(var folder in folders)
            {
                var files = await objStorage.ListFilesInFolderAsync(folder);

                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }
            }

            var folderName = folders[0];
            var filesInFolder = await objStorage.ListFilesInFolderAsync(folderName);

            /**
            try
            {
                foreach (var file in filesInFolder)
                {
                    var extension = Path.GetExtension(file);
                    var newFileName = file.Replace("Allegorii", "Аллегории");
                    await objStorage.RenameFileAsync(file, newFileName);

                    logger.Info($"filename {file} renamed with {newFileName} successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Возникла ошибка при переименовании файла ", ex);
            }
            **/



        }
    }
}
