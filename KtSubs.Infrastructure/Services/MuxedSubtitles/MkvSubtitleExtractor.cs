using CliWrap;
using KtSubs.Core.Settings;
using LanguageExt.Common;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace KtSubs.Infrastructure.Services.MuxedSubtitles
{
    public class MkvSubtitleExtractor
    {
        private string mvkmergePath;
        private string mkvextractPath;
        private string extractedSubsFolder;
        private readonly ISettingsProvider settingsProvider;
        private readonly ILogger logger;

        private Dictionary<string, string> codecExtensions = new Dictionary<string, string>()
        {
            ["S_TEXT/UTF8"] = "srt",
            ["S_TEXT/ASCII"] = "srt",
            ["S_TEXT/SSA"] = "ssa",
            ["S_TEXT/ASS"] = "ass",
            ["S_SSA"] = "ssa",
            ["S_ASS"] = "ass",
        };

        public MkvSubtitleExtractor(ISettingsProvider settingsProvider, ILogger logger)
        {
            this.settingsProvider = settingsProvider;
            this.logger = logger;
            SetSettings();
        }

        public bool HasMkvToolkit()
        {
            return Path.Exists(mvkmergePath) && Path.Exists(mkvextractPath);
        }

        public async Task<List<Track>> GetSubtitlesTrackData(string filePath)
        {
            var stdOutBuffer = new StringBuilder();

            var result = await Cli.Wrap(mvkmergePath)
                .WithArguments($"-i -J \"{filePath}\"")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var mkvmergeOutput = JsonConvert.DeserializeObject<MkvmergeOutput>(stdOut);
            return mkvmergeOutput.Tracks.Where(t => t.Type == "subtitles" && codecExtensions.ContainsKey(t.Properties.CodecId)).ToList();
        }

        public async Task<string> ExtractSubtitles(string inputFilePath, Track track, CancellationToken cancellationToken)
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var subtitlesFileName = $"{Path.GetFileNameWithoutExtension(inputFilePath)}_{track.Id}.{GetFileExtension(track)}";

            var result = await Cli.Wrap(mkvextractPath)
                .WithArguments($"tracks \"{inputFilePath}\" {track.Id}:\"{subtitlesFileName}\"")
                .WithWorkingDirectory(extractedSubsFolder)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cancellationToken);

            return Path.Combine(extractedSubsFolder, subtitlesFileName);
        }

        private string GetFileExtension(Track track)
        {
            var codecId = track?.Properties?.CodecId ?? String.Empty;
            if (codecExtensions.ContainsKey(codecId))
                return codecExtensions[codecId];

            return string.Empty;
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            var settings = settingsProvider.GetSettings();
            var mkvPath = settings.MkvToolnixFolder;
            mvkmergePath = Path.Combine(mkvPath, "mkvmerge");
            mkvextractPath = Path.Combine(mkvPath, "mkvextract");
            SetExtractedSubsFolder(settings);
        }

        private void SetExtractedSubsFolder(Settings settings)
        {
            var path = settings.LocationOfExtractedSubtitles;
            var expandedPath = Environment.ExpandEnvironmentVariables(path);

            var fallbackPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KtSubs", "subtitles");


            if (IsPathValid(expandedPath))
            {
                var result = TryCreateSubtitlesDirectory(expandedPath);
                if (result != TryCreateDirectoryResult.IOError)
                {
                    extractedSubsFolder = expandedPath;
                    return;
                }
            }

            var fallbackFolderCreationResult = TryCreateSubtitlesDirectory(fallbackPath);
            if (fallbackFolderCreationResult == TryCreateDirectoryResult.IOError)
            {
                throw new Exception("Cannot create a directory for a fallback path");
            }
            extractedSubsFolder = fallbackPath;
        }

        enum TryCreateDirectoryResult
        {
            Exists,
            Created,
            IOError
        }

        private TryCreateDirectoryResult TryCreateSubtitlesDirectory(string path)
        {
            if (Directory.Exists(path)) return TryCreateDirectoryResult.Exists;

            try
            {
                Directory.CreateDirectory(path);
                return TryCreateDirectoryResult.Created;
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Exception while creating directory");
                return TryCreateDirectoryResult.IOError;
            }
        }

        private static bool IsPathValid(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}