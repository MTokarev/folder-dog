using System.IO;
using FolderDog.Interfaces;
using FolderDog.Options;
using FolderDog.Services;
using Microsoft.Extensions.Configuration;
using Serilog;


internal class Program
{
    private const string ConfigFileName = "appsettings.json";
    private static readonly BindingOptions _fileOptions = new();
    private static readonly EmailOptions _emailOptions = new();
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
        ConfigureFileListener();
    }

    /// <summary>
    /// Set file listeners
    /// </summary>
    private static void ConfigureFileListener()
    {
        foreach(string fileExtension in _fileOptions.FileExtensions)
        {
            var watcher = new FileSystemWatcher(_fileOptions.FolderPath);
            watcher.NotifyFilter = NotifyFilters.Attributes
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.Size;

            watcher.Created += OnCreated;
            watcher.Filter = $"*.{fileExtension}";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        var fullPath = Path.Join(Directory.GetCurrentDirectory(), 
            _fileOptions.FolderPath == "./" 
                ? string.Empty
                : _fileOptions.FolderPath);
        string fileExtensionsString = string.Join("|", _fileOptions.FileExtensions);

         _logger.Information("The app has started listening for file creation with extensions " + 
            "'{FileExtension}' in the folder '{FullPath}' and all child directories.",
            fileExtensionsString,
            fullPath);
        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }

    /// <summary>
    /// OnFile creation handler
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">File event</param>
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.Information("New file creation has been detected: '{FullPath}'", e.FullPath);
        var result = _messageSender.SendMessage($"File was created '{e.Name}'", e.FullPath);

        if (result.IsSuccessful)
        {
            _logger.Information("File {FileName} has been processed", e.Name);
        }
        else
        {
            _logger.Error("Unable to process '{FileName}'. Errors: '{Errors}'", 
                e.Name, 
                string.Join(",", result.Errors));
        }
    }

    /// <summary>
    /// Bind setting to models
    /// </summary>
    private static void BindOptions()
    {
        _config.GetSection("Binding")
            .Bind(_fileOptions);
        _config.GetSection("Email")
            .Bind(_emailOptions);
    }

    /// <summary>
    /// Load configuration from the config file
    /// </summary>
    /// <returns>Return config</returns>
    private static IConfigurationRoot LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: true);

        var config = builder.Build();
        return config;
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