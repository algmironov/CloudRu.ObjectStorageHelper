using Amazon.Runtime;
using Amazon.S3;

using SimpleLogger;

namespace CloudRu.ObjectStorageHelper
{
    /// <summary>
    /// Класс-строитель для создания экземпляра ObjectStorageService.
    /// Позволяет пошагово настроить параметры для подключения к Объектному хранилищу Cloud.RU.
    /// </summary>
    public class S3ClientBuilder
    {
        private string _tenantId = string.Empty;
        private string _accessKey = string.Empty;
        private string _secretKey = string.Empty;
        private string _bucketName = string.Empty;
        private string _serviceUrl = "https://s3.cloud.ru";
        private bool _useLogger = false;
        private SimpleLoggerOptions? _options;

        /// <summary>
        /// Устанавливает идентификатор тенанта.
        /// </summary>
        /// <param name="tenantId">Идентификатор тенанта.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder WithTenantId(string tenantId)
        {
            _tenantId = tenantId;
            return this;
        }

        /// <summary>
        /// Устанавливает ключ доступа.
        /// </summary>
        /// <param name="accessKey">Ключ доступа.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder WithAccessKey(string accessKey)
        {
            _accessKey = accessKey;
            return this;
        }

        /// <summary>
        /// Устанавливает секретный ключ.
        /// </summary>
        /// <param name="secretKey">Секретный ключ.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder WithSecretKey(string secretKey)
        {
            _secretKey = secretKey;
            return this;
        }

        /// <summary>
        /// Устанавливает имя бакета.
        /// </summary>
        /// <param name="bucketName">Имя бакета.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder WithBucketName(string bucketName)
        {
            _bucketName = bucketName;
            return this;
        }

        /// <summary>
        /// Устанавливает URL сервиса S3.
        /// </summary>
        /// <param name="serviceUrl">URL сервиса S3.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder WithServiceUrl(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
            return this;
        }

        /// <summary>
        /// Включает использование логгера и устанавливает его настройки.
        /// </summary>
        /// <param name="options">Настройки логгера.</param>
        /// <returns>Текущий экземпляр S3ClientBuilder для цепочки вызовов.</returns>
        public S3ClientBuilder UseLogger(SimpleLoggerOptions options)
        {
            _options = options;
            _useLogger = true;
            return this;
        }

        /// <summary>
        /// Создает и возвращает настроенный экземпляр ObjectStorageService.
        /// </summary>
        /// <returns>Настроенный экземпляр ObjectStorageService.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если какой-либо из обязательных параметров не был установлен.</exception>
        public ObjectStorageService Build()
        {
            if (string.IsNullOrEmpty(_tenantId)) throw new ArgumentNullException(nameof(_tenantId));
            if (string.IsNullOrEmpty(_accessKey)) throw new ArgumentNullException(nameof(_accessKey));
            if (string.IsNullOrEmpty(_secretKey)) throw new ArgumentNullException(nameof(_secretKey));
            if (string.IsNullOrEmpty(_bucketName)) throw new ArgumentNullException(nameof(_bucketName));

            var config = new AmazonS3Config
            {
                AuthenticationRegion = "ru-central-1",
                ServiceURL = _serviceUrl,
                SignatureVersion = "4",
                SignatureMethod = SigningAlgorithm.HmacSHA256,
                ForcePathStyle = true,
            };

            var s3Client = new AmazonS3Client($"{_tenantId}:{_accessKey}", _secretKey, config);

            return (_useLogger && _options != null)
                    ? new ObjectStorageService(s3Client, _bucketName, _options)
                    : new ObjectStorageService(s3Client, _bucketName);
        }
    }
}
