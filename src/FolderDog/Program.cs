using System.Net;
using FolderDog.Interfaces;
using FolderDog.Models;
using FolderDog.Options;
using FolderDog.Services;
using Microsoft.Extensions.Configuration;
using Serilog;


internal class Program
{
    private const string ConfigFileName = "appsettings.json";
    private static readonly BindingOptions _bindingOptions = new();
    private static readonly EmailOptions _emailOptions = new();
    private static readonly WebhookOptions _webhookOptions = new();
    private static readonly FileServiceOptions _fileServiceOptions = new();
    private static WebhookService _webhookService;
    private static IFileService _fileService;
    private static IMessageSender _messageSender;
    private static ILogger _logger;
    private static  IConfiguration _config;

    private static void Main(string[] args)
    {
        // 1. Loads configuration from 'appsettings.json'
        // 2. Creates logger that logs message to a file and the console
        // 3. Instantiate SMTP mail client
        // 4. Applies a file filters that are specified in the configuration file (for example listen only for .csv and .xls files)
        // 5. Sends an email with the file as attachment on onCreate event
        
        _config = LoadConfiguration();
        InitLogger();
        BindOptions();
        _messageSender = new EmailService(_emailOptions, _logger);
        _fileService = new FileService(_fileServiceOptions, _logger);
        _webhookService = new WebhookService(_webhookOptions, _logger);
        ConfigureFileListener();

        var fullPath = string.Equals(_bindingOptions.FolderPath, "./")
            ? Directory.GetCurrentDirectory()
            : _bindingOptions.FolderPath;
        string fileExtensionsString = string.Join("|", _bindingOptions.FileExtensions);
        string optionalMessage = _bindingOptions.ListenInSubfolders
            ? " and all child directories"
            : string.Empty;

        _logger.Information("The app has started listening for file creation with extensions " +
           "'{FileExtensions}' in the folder '{FullPath}'{OptionalMessage}.",
           fileExtensionsString,
           fullPath,
           optionalMessage);
        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }

    /// <summary>
    /// Set file listeners
    /// </summary>
    private static void ConfigureFileListener()
    {
        foreach(string fileExtension in _bindingOptions.FileExtensions)
        {
            var watcher = new FileSystemWatcher(_bindingOptions.FolderPath);
            watcher.NotifyFilter = NotifyFilters.Attributes
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.Size;

            watcher.Filter = $"*.{fileExtension}";
            watcher.IncludeSubdirectories = _bindingOptions.ListenInSubfolders;
            watcher.EnableRaisingEvents = true;
            
            // Registering handlers
            if (!string.IsNullOrEmpty(_webhookOptions.Url))
            {
                watcher.Created += async (object sender, FileSystemEventArgs e) 
                    => await _webhookService.SendHookAsync(e);
            }

            if (!string.IsNullOrEmpty(_emailOptions.SmtpServerHost))
            {
                watcher.Created += OnCreatedMailSendAsync;
            }
                
        }
    }

    /// <summary>
    /// OnFile creation async handler for mail sending
    /// </summary>
    /// <remarks>
    /// Overall it is a bad practice to use 'async void' except for event handlers.
    /// <seealso cref="https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming"/>
    /// </remarks>
    /// <param name="sender">Event sender</param>
    /// <param name="e">File event</param>
    private static async void OnCreatedMailSendAsync(object sender, FileSystemEventArgs e)
    {
        // Run handler in the thread pool
        // File service has a conflict resolution logic that locks the thread before retry
        // accessing file.
        // To avoid blockage in the main thread the operation is offloaded to the ThreadPool.
        await Task.Run(async () =>
        {
            // Since it is fire and forget mode - any unhandled exception will terminate the app.
            // Here we need to log all exceptions.
            FileStream fs = null;
            try
            {
                _logger.Information("New file creation has been detected: '{FullPath}'", e.FullPath);
                if (_fileService.TryGetFileStream(e.FullPath, out fs))
                {
                    var fileInfo = new FileInfo(e.FullPath);
                    var result = _messageSender.SendMessage($"File was created '{fileInfo.Name}'", fs, fileInfo.Name);

                    if (result.IsSuccessful)
                    {
                        _logger.Information("File {FileFullName} has been processed", fileInfo.FullName);
                    }
                    else
                    {
                        _logger.Error("Unable to process '{FileFullName}'. Errors: '{Errors}'.",
                            fileInfo.FullName,
                            string.Join(",", result.Errors));
                    }
                }
            }
            // Catching all exceptions to avoid app crash
            catch (Exception ex)
            {
                _logger.Error("Exception has been thrown in the event handler. Error message '{Message}'.",
                    ex.Message);
            }
            finally
            {
                if (fs is not null)
                {
                    await fs.DisposeAsync();
                }
            }
        });
    }

    /// <summary>
    /// Bind setting to models
    /// </summary>
    private static void BindOptions()
    {
        _config.GetSection("Binding")
            .Bind(_bindingOptions);
        _config.GetSection("Email")
            .Bind(_emailOptions);
        _config.GetSection("FileService")
            .Bind(_fileServiceOptions);
        _config.GetSection("Webhook")
            .Bind(_webhookOptions);
    }

    /// <summary>
    /// Load configuration from the config file
    /// </summary>
    /// <returns>Return config</returns>
    private static IConfigurationRoot LoadConfiguration()
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigFileName, optional: false);

            var config = builder.Build();
            return config;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Unable to load 'appsettings.json' file." +
                "Please make sure you put it in the same location with this app." +
                $"Error message: '{ex.Message}'.");
            Console.WriteLine("Please check readme file: 'https://github.com/MTokarev/folder-dog/blob/main/README.MD'");
        }

        // App failed at this point
        return null;
    }

    /// <summary>
    /// Initialize logger. Use configuration provided in the config file
    /// </summary>
    /// <returns>Return Logger</returns>
    private static ILogger InitLogger()
    {
        _logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(_config)
            .CreateLogger();
        
        return _logger;
    }
}