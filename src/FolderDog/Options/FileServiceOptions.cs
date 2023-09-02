namespace FolderDog.Options
{
	/// <summary>
	/// File service options
	/// </summary>
	public class FileServiceOptions
	{
		/// <summary>
		/// File can be used by the process.
		/// This param control how many attempts will be made accessing file.
		/// </summary>
		public int RepeatAccessAttempts { get; set; }

		/// <summary>
		/// The wait time in milliseconds before attempts
		/// <seealso cref="RepeatAccessAttempts"/>
		/// </summary>
		public int WaitUntilNextRetryInMilliseconds { get; set; }

		/// <summary>
		/// Optional: If true - process the same file more than once if we get more than one file creation event
		/// By default it set to false. That means if the app has proccessed the file with the same name and size
		/// then program will log event and ignore.
		/// </summary>
		public bool SkipProcessedFiles { get; set; } = false;
	}
}
