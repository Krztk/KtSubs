namespace KtSubs.Infrastructure.Services.Vlc
{
    public class VlcConnectionException : Exception
    {
        public VlcConnectionException(string? message) : base(message)
        {
        }

        public VlcConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}