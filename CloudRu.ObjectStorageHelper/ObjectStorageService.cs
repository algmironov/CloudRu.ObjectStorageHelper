using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

using SimpleLogger;

using Logger = SimpleLogger.Logger;

namespace CloudRu.ObjectStorageHelper
{
    /// <summary>
    ///Класс для взаимодействия с объектным хранилищем Cloud.ru
    ///Позволяет создавать/переименовывать/удалять/получать список папок и файлов в бакете
    /// </summary>
    public class ObjectStorageService
    {
        private readonly AmazonS3Client _s3Client;
        private readonly Logger? _logger;
        private readonly string _bucketName;
        private readonly bool _useLogger;

        #region CTORs

        /// <summary>
        /// Инициализирует новый экземпляр класса ObjectStorageService.
        /// </summary>
        /// <param name="client">Объект <see cref="AmazonS3Client"/></param>
        /// <param name="bucketName">Имя бакета по умолчанию.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если какой-либо из параметров null или пустой.</exception>
        public ObjectStorageService(AmazonS3Client client, string bucketName)
        {
            _s3Client = client;
            _bucketName = bucketName;
            _useLogger = false;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса ObjectStorageService.
        /// </summary>
        /// <param name="client">Объект <see cref="AmazonS3Client"/></param>
        /// <param name="bucketName">Имя бакета по умолчанию.</param>
        /// <param name="options">Параметры <see cref="SimpleLoggerOptions"/> для включения логирования</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если какой-либо из параметров null или пустой.</exception>
        public ObjectStorageService(AmazonS3Client client, string bucketName, SimpleLoggerOptions options)
        {
            _s3Client = client;
            _bucketName = bucketName;
            _useLogger = true;
            _logger = new Logger(options);
        }


        /// <summary>
        /// Инициализирует новый экземпляр класса ObjectStorageService.
        /// </summary>
        /// <param name="accessKey">Ключ доступа для сервиса Object Storage. Состоит из строки вида: "Your_Tenant_ID:Your_Access_Key_ID" </param>
        /// <param name="secretKey">Секретный ключ для сервиса Object Storage.</param>
        /// <param name="bucketName">Имя бакета по умолчанию.</param>
        /// <param name="serviceUrl">URL сервиса Object Storage. Использует дефолтное url от Cloud.ru, но в случае изменения можно указать другое значение</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если какой-либо из параметров null или пустой.</exception>
        public ObjectStorageService(string accessKey, string secretKey, string bucketName, string serviceUrl = "https://s3.cloud.ru")
        {
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));
            if (string.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey));
            if (string.IsNullOrEmpty(bucketName)) throw new ArgumentNullException(nameof(bucketName));

            var config = new AmazonS3Config
            {
                AuthenticationRegion = "ru-central-1",
                ServiceURL = serviceUrl,
                SignatureVersion = "4",
                SignatureMethod = SigningAlgorithm.HmacSHA256,
                ForcePathStyle = true,
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);

            _bucketName = bucketName;

            _useLogger = false;

        }

        /// <summary>
        /// Инициализирует новый экземпляр класса ObjectStorageService.
        /// </summary>
        /// <param name="accessKey">Ключ доступа для сервиса Object Storage. Состоит из строки вида: "Your_Tenant_ID:Your_Access_Key_ID" </param>
        /// <param name="secretKey">Секретный ключ для сервиса Object Storage.</param>
        /// <param name="serviceUrl">URL сервиса Object Storage. Использует дефолтное url от Cloud.ru, но в случае изменения можно указать другое значение</param>
        /// <param name="bucketName">Имя бакета по умолчанию.</param>
        /// <param name="options" cref="SimpleLoggerOptions">Параметры логирования</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если какой-либо из параметров null или пустой.</exception>
        public ObjectStorageService(string accessKey, string secretKey, string bucketName, SimpleLoggerOptions options, string serviceUrl = "https://s3.cloud.ru")
        {
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));
            if (string.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey));
            if (string.IsNullOrEmpty(bucketName)) throw new ArgumentNullException(nameof(bucketName));

            var config = new AmazonS3Config
            {
                AuthenticationRegion = "ru-central-1",
                SignatureVersion = "4",
                SignatureMethod = SigningAlgorithm.HmacSHA256,
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
            _bucketName = bucketName;


            _logger = new Logger(options);
            _useLogger = true;

        }

        #endregion

        #region Operations

        /// <summary>
        /// Создает новую папку в текущем бакете.
        /// </summary>
        /// <param name="folderName">Имя создаваемой папки.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке создания папки.</exception>
        public async Task CreateFolderAsync(string folderName)
        {

            await ExecuteS3OperationAsync(async () =>
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = folderName.TrimEnd('/') + "/",
                        ContentBody = string.Empty
                    };
                    await _s3Client.PutObjectAsync(request);
                }, $"Возникла ошибка при создании папки {folderName}");

        }

        /// <summary>
        /// Переименовывает папку в текущем бакете.
        /// 
        /// Поскольку S3 API не поддерживает непосредственного переисенования папки в облаке, все файлы из нее копируются в новую папку с новым именем, а по завершении операции старая папка удаляется.
        /// </summary>
        /// <param name="oldFolderName">Текущее имя папки.</param>
        /// <param name="newFolderName">Новое имя папки.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке переименования папки.</exception>
        public async Task RenameFolderAsync(string oldFolderName, string newFolderName)
        {
            await ExecuteS3OperationAsync(async () =>
            {
                var objects = await ListFilesInFolderAsync(oldFolderName);
                foreach (var obj in objects)
                {
                    var oldKey = obj;
                    var newKey = obj.Replace(oldFolderName, newFolderName);

                    await CopyObjectAsync(oldKey, newKey);
                    await DeleteObjectAsync(oldKey);
                }
                await DeleteFolderAsync(oldFolderName);
            }, $"Возникла ошибка при переименовании папки {oldFolderName} в {newFolderName}");
        }

        /// <summary>
        /// Удаляет папку и все ее содержимое из текущего бакета.
        /// </summary>
        /// <param name="folderName">Имя удаляемой папки.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке удаления папки.</exception>
        public async Task DeleteFolderAsync(string folderName)
        {
            await ExecuteS3OperationAsync(async () =>
            {
                var objects = await ListFilesInFolderAsync(folderName);
                foreach (var obj in objects)
                {
                    await DeleteObjectAsync(obj);
                }
            }, $"Возникла ошибка при удалении папки {folderName}");
        }

        /// <summary>
        /// Загружает файл в указанную папку текущего бакета.
        /// </summary>
        /// <param name="folderName">Имя папки, в которую загружается файл.</param>
        /// <param name="filePath">Полный путь к загружаемому файлу.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке загрузки файла.</exception>
        /// <exception cref="FileNotFoundException">Выбрасывается, если файл не найден.</exception>
        public async Task UploadFileAsync(string folderName, string filePath)
        {
            await ExecuteS3OperationAsync(async () =>
            {
                var fileName = Path.GetFileName(filePath);
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = Path.Combine(folderName, fileName).Replace("\\", "/"),
                    FilePath = filePath
                };
                await _s3Client.PutObjectAsync(request);
            }, $"Возникла ошибка при сохранении файла {filePath} в папке {folderName}");
        }

        /// <summary>
        /// Получает список всех папок в текущем бакете.
        /// </summary>
        /// <returns>Список имен папок.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке получения списка папок.</exception>
        public async Task<List<string>> ListFoldersAsync()
        {
            return await ExecuteS3OperationAsync(async () =>
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Delimiter = "/"
                };
                var response = await _s3Client.ListObjectsV2Async(request);
                return response.CommonPrefixes.Select(prefix => prefix.TrimEnd('/')).ToList();
            }, "Произошла ошибка при получении списка папок");
        }

        /// <summary>
        /// Получает список всех файлов в указанной папке текущего бакета.
        /// </summary>
        /// <param name="folderName">Имя папки для получения списка файлов.</param>
        /// <returns>Список имен файлов. Первым элементом возвращается всегда префикс: имя папки и '/'. Имена файлов так же содержат префикс - учитывайте это при обработке имен файлов</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке получения списка файлов.</exception>
        public async Task<List<string>> ListFilesInFolderAsync(string folderName)
        {
            return await ExecuteS3OperationAsync(async () =>
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = folderName.TrimEnd('/') + "/"
                };
                var response = await _s3Client.ListObjectsV2Async(request);
                return response.S3Objects.Select(obj => obj.Key).ToList();
            }, $"Произошла ошибка при получении списка файлов в папке {folderName}");
        }

        /// <summary>
        /// Получает поток данных для указанного файла из указанной папки текущего бакета.
        /// </summary>
        /// <param name="folderName">Имя папки, содержащей файл.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Поток данных файла.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке получения файла.</exception>
        public async Task<Stream> GetFileAsync(string folderName, string fileName)
        {
            return await ExecuteS3OperationAsync(async () =>
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = Path.Combine(folderName, fileName).Replace("\\", "/")
                };
                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }, $"Произошла ошибка при получении файла {fileName} из папки {folderName}");
        }


        /// <summary>
        /// Переименовывает файл в текущем бакете.
        /// </summary>
        /// <param name="oldKey">Текущий ключ (путь) файла.</param>
        /// <param name="newKey">Новый ключ (путь) файла.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        public async Task RenameFileAsync(string oldKey, string newKey)
        {
            await CopyObjectAsync(oldKey, newKey);
            await DeleteObjectAsync(oldKey);
        }

        /// <summary>
        /// Удаляет файл из указанной папки в текущем бакете.
        /// </summary>
        /// <param name="folderName">Имя папки, содержащей файл.</param>
        /// <param name="fileName">Имя удаляемого файла.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке удаления файла.</exception>
        public async Task DeleteFileAsync(string folderName, string fileName)
        {
            string key = Path.Combine(folderName, fileName).Replace("\\", "/");
            await DeleteObjectAsync(key);
        }

        /// <summary>
        /// Копирует объект внутри текущего бакета.
        /// </summary>
        /// <param name="sourceKey">Ключ исходного объекта.</param>
        /// <param name="destinationKey">Ключ целевого объекта.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке копирования объекта.</exception>
        private async Task CopyObjectAsync(string sourceKey, string destinationKey)
        {
            await ExecuteS3OperationAsync(async () =>
            {
                var request = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourceKey,
                    DestinationBucket = _bucketName,
                    DestinationKey = destinationKey
                };
                await _s3Client.CopyObjectAsync(request);
            }, $"Возникла ошибка при копировании объекта из {sourceKey} в {destinationKey}");
        }


        /// <summary>
        /// Удаляет объект из текущего бакета.
        /// </summary>
        /// <param name="key">Ключ удаляемого объекта.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        /// <exception cref="AmazonS3Exception">Выбрасывается при ошибке удаления объекта.</exception>
        private async Task DeleteObjectAsync(string key)
        {
            await ExecuteS3OperationAsync(async () =>
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };
                await _s3Client.DeleteObjectAsync(request);
            }, $"Возникла ошибка при удалении объекта {key}");

        }

        /// <summary>
        /// Выполняет асинхронную операцию S3 с обработкой исключений и логированием.
        /// </summary>
        /// <param name="operation">Асинхронная операция для выполнения.</param>
        /// <param name="errorMessage">Сообщение об ошибке для логирования в случае исключения.</param>
        /// <returns>Task, представляющий асинхронную операцию.</returns>
        private async Task ExecuteS3OperationAsync(Func<Task> operation, string errorMessage)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                if (_useLogger)
                {
                    _logger!.Error(errorMessage, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Выполняет асинхронную операцию S3 с возвращаемым значением, с обработкой исключений и логированием.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
        /// <param name="operation">Асинхронная операция для выполнения.</param>
        /// <param name="errorMessage">Сообщение об ошибке для логирования в случае исключения.</param>
        /// <returns>Результат выполнения операции.</returns>
        private async Task<T> ExecuteS3OperationAsync<T>(Func<Task<T>> operation, string errorMessage)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (_useLogger)
                {
                    _logger!.Error(errorMessage, ex);
                }

                throw;
            }
        }
        #endregion
    }
}
