namespace KtSubs.Core.Exceptions
{
    public class SubtitlesReadingException : Exception
    {
        public SubtitlesReadingException(string? message) : base(message)
        {
        }

        public SubtitlesReadingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}