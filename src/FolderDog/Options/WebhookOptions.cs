namespace FolderDog.Options
{
    public class WebhookOptions
    {
        /// <summary>
        /// Webhook server URL
        /// </summary>
        public string Url { get; set; }

        // HTTP method
        // Allowed:
        // - POST
        // - GET
        public string Method { get; set; }

        /// <summary>
        /// Body is required for POST method type.
        /// Here you can provide a request body.
        /// </summary>
        public Dictionary <string, string> Body { get; set; } = new();
    }
}