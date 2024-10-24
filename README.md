# CloudRu.ObjectStorageHelper

# ObjectStorageService для Cloud.ru и Yandex.Cloud

ObjectStorageService - это .NET библиотека для удобного взаимодействия с Object Storage сервиса Cloud.ru. Библиотека предоставляет простой и интуитивно понятный интерфейс для выполнения основных операций с объектным хранилищем.
> Может быть использована для Yandex Cloud

## Основные возможности

- Создание, переименование, и удаление папок
- Загрузка, скачивание и удаление файлов
- Получение списка папок и файлов
- Простой в использовании асинхронный API

## Установка

Установите пакет ObjectStorageService через NuGet Package Manager:

```
NuGet\Install-Package CloudRu.ObjectStorageHelper -Version 2.0.0
```

Или через .NET CLI:

```
dotnet add package CloudRu.ObjectStorageHelper --version 2.0.0
```

## Использование

Вот пример базового использования библиотеки:

### С помощью Builder() :
```csharp

var s3Client = new S3ClientBuilder()
    .WithTenantId("YOUR_TENANT_ID") 
    .WithAccessKey("YOUR_ACCESS_KEY")
    .WithSecretKey("YOUR_SECRET_KEY")
    .WithBucketName("YOUR_BUCKET_NAME")
    .Build();

var folders = await s3Client.ListFoldersAsync();

foreach (var folder in folders)
{
    Console.WriteLine(folder);
}

```
### С помощью конструктора :
```csharp
using ObjectStorageService;

// Инициализация сервиса
var service = new ObjectStorageService(
    "your_access_key", // здесь accessKey - это строка, состоящая из: <tenantId:accessKey>
    "your_secret_key",
    "your-bucket-name"
);

// Создание папки
await service.CreateFolderAsync("my-folder");

// Загрузка файла
await service.UploadFileAsync("my-folder", "path/to/local/file.txt");

// Получение списка файлов в папке
var files = await service.ListFilesInFolderAsync("my-folder");

// Скачивание файла
using (var fileStream = await service.GetFileAsync("my-folder", "file.txt"))
{
    // Работа с файловым потоком
}

// Удаление файла
await service.DeleteFileAsync("my-folder", "file.txt");
```

## Документация

Подробную документацию по всем доступным методам можно (будет) найти в Wiki проекта.

## Требования

.NET8
[AWSSDK.S3](https://www.nuget.org/packages/AWSSDK.S3) (3.7.0 или выше)

## Лицензия

Этот проект лицензирован под MIT License - см. файл LICENSE для деталей.

## Вклад в проект

Мы приветствуем вклад в развитие проекта! Пожалуйста, ознакомьтесь с нашим руководством по внесению изменений перед тем, как отправлять pull request.

## Поддержка
Если у вас возникли проблемы или есть предложения по улучшению библиотеки, пожалуйста, создайте issue в GitHub репозитории проекта.

## Авторы

[algmironov](https://github.com/algmironov)

## Благодарности

Спасибо команде [Cloud.ru](https://cloud.ru) за отличный сервис Object Storage.

Библиотека использует [AWSSDK.S3](https://www.nuget.org/packages/AWSSDK.S3) для взаимодействия с S3-совместимым API
