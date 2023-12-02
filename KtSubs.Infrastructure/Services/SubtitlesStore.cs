using KtSubs.Core.Entries;
using KtSubs.Core.Exceptions;
using KtSubs.Core.Services;
using KtSubs.Infrastructure.Services.Readers;
using Serilog;

namespace KtSubs.Infrastructure.Services
{
    public class SubtitlesStore : ISubtitlesStore
    {
        private readonly ReaderFactory readerFactory;
        private readonly ILogger logger;

        public event EventHandler? SubtitlesLoaded;

        public IReadOnlyList<IEntry> Entries { get; private set; } = new List<IEntry>();

        public IReadOnlySet<string> Layers { get; private set; } = new HashSet<string>();

        public SubtitlesStore(ReaderFactory readerFactory, ILogger logger)
        {
            this.readerFactory = readerFactory;
            this.logger = logger;
        }

        public void Load(string path)
        {
            var subsTypeOption = SubsHelper.GetSubtitlesType(path);
            subsTypeOption.IfSome(subsType =>
            {
                ISubtitlesReader reader = readerFactory.CreateReader(subsType);
                try
                {
                    logger.Debug("Starting reading subtitles file.");
                    var result = reader.GetEntries(path);
                    logger.Debug("Subtitle file has been read.");
                    Entries = result.Entries;
                    Layers = result.Layers;
                    SubtitlesLoaded?.Invoke(this, EventArgs.Empty);
                }
                catch (SubtitlesReadingException ex)
                {
                    logger.Error(ex, "SubtitlesReadingException thrown while getting subtitles content.");
                    throw;
                }
            });
        }
    }
}