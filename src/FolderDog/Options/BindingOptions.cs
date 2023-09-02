namespace FolderDog.Options
{
    /// <summary>
    /// Options model for file binding
    /// </summary>
    public class BindingOptions
    {
        /// <summary>
        /// Binding folder path
        /// </summary>
        public string FolderPath { get; set; } = "./";

        /// <summary>
        /// Listen in the subfolders
        /// </summary>
        public bool ListenInSubfolders { get; set; } = true;

        /// <summary>
        /// Array of file extensions to listen
        /// </summary>
        public IList<string> FileExtensions { get; set; }
    }
}