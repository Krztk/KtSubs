namespace KtSubs.Core.Services
{
    public interface IMediaTimeProvider
    {
        public Task<int> GetTimeMs();
    }
}