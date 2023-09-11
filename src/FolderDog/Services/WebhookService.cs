using System.Text;
using System.Text.Json;
using FolderDog.Interfaces;
using FolderDog.Models;
using FolderDog.Options;
using Serilog;

namespace FolderDog.Services
{
    /// <summary>
    /// Web hook service
    /// <see cref="IHookService"/>
    /// </summary>
    public class WebhookService : IHookService
    {
        private const string
            JsonContent = "application/json",
            FileNameMask = "{{fileName}}",
            FileFullPathMask = "{{fileFullPath}}";
        private readonly WebhookOptions _options;
        private readonly ILogger _logger;

        public WebhookService(WebhookOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// <see cref="IHookService.SendHookAsync(FileSystemEventArgs)"/>
        /// </summary>
        public async Task<Result> SendHookAsync(FileSystemEventArgs e)
        {
            string correlationId = Guid.NewGuid().ToString();
            _logger.Debug("{CorrelationId} Start webhook execution for the file '{FileFullPath}'...", 
                correlationId,
                e.FullPath);
            var result = new Result();
            var httpClient = new HttpClient();

            var response = new HttpResponseMessage();

            if (string.Equals(_options.Method, HttpMethod.Post.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                string body = JsonSerializer
                    .Serialize(_options.Body)
                    .Replace(FileNameMask, e.Name)
                    .Replace(FileFullPathMask, e.FullPath);
                var content = new StringContent(
                    body, 
                    Encoding.UTF8,
                    JsonContent);

                try
                {
                    response = await httpClient.PostAsync(_options.Url, content);
                }
                catch (HttpRequestException ex)
                {
                    string errMessage = $"{correlationId} Webhook has been failed for the file '{e.Name}'. Error message: '{ex.Message}'";
                    _logger.Warning(errMessage);
                    result.Errors.Add(errMessage);

                    return result;
                }
            }
            else if (string.Equals(_options.Method, HttpMethod.Get.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    response = await httpClient.GetAsync(_options.Url);
                }
                catch (HttpRequestException ex)
                {
                    string errMessage = $"{correlationId} Webhook has been failed for the file '{e.Name}'. Error message: '{ex.Message}'.";
                    _logger.Warning(errMessage);
                    result.Errors.Add(errMessage);

                    return result;
                }
            }

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                
                string message = $"{correlationId} Webhook has been executed successfully. Message '{responseString}'";
                result.Message = message;
                _logger.Information(message);
            }
            else
            {
                string message = $"{correlationId} HTTP returned '{response.StatusCode}' for file '{e.Name}'.";
                result.Errors.Add(message);
                _logger.Warning(message);
            }

            _logger.Debug("{CorrelationId} Webhook execution has been completed for the file '{FileFullPath}'...", 
                correlationId,
                e.FullPath);

            return result;
        }
    }
}