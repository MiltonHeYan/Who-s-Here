namespace WhosHereServer
{
    /// <summary>
    /// The exception that is thrown when an invalid value is read from appsettings.json file.
    /// Please check appsettings.json again before starting the server.
    /// </summary>
    public class InvalidAppSettingsException : Exception
    {
        public InvalidAppSettingsException()
        {
        }

        public InvalidAppSettingsException(string? message) : base(message)
        {
        }

        public InvalidAppSettingsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
