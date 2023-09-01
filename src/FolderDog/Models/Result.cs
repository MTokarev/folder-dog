namespace FolderDog.Models
{
    /// <summary>
    /// Represents operation result
    /// </summary>
    public class Result
    {
        /// <summary>
        /// List of errors
        /// </summary>
        public IList<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Operation result
        /// </summary>
        public bool IsSuccessful => Errors.Count == 0;

        /// <summary>
        /// Result message
        /// </summary>
        public string Message { get; set; }
    }
}