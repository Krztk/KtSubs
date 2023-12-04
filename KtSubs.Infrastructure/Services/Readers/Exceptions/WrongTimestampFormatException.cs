namespace KtSubs.Core.Exceptions
{
    public class WrongTimestampFormatException : Exception
    {
        public WrongTimestampFormatException()
        { }

        public WrongTimestampFormatException(string? message) : base(message)
        {
        }
    }
}