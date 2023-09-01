namespace FolderDog.Options
{
    public class EmailOptions
    {
        public string SendFrom { get; set; }
        public IList<string> SendTos { get; set; }
        public string SmtpServerIpv4Address { get; set; }
        public int SmtpServerPortNumber { get; set; } = 25;
        public IList<string> SendCcs { get; set; } = new List<string>();
        public IList<string> SendBccs { get; set; } = new List<string>();
        public string MessageSubject { get; set; } = string.Empty;
    }
}