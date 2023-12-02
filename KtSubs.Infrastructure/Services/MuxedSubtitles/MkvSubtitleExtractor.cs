using CliWrap;
using KtSubs.Core.Settings;
using Newtonsoft.Json;
using System.Text;

namespace KtSubs.Infrastructure.Services.MuxedSubtitles
{
    public class MkvSubtitleExtractor
    {
        private string mvkmergePath;
        private string mkvextractPath;
        private readonly ISettingsProvider settingsProvider;

        private Dictionary<string, string> codecExtensions = new Dictionary<string, string>()
        {
            ["S_TEXT/UTF8"] = "srt",
            ["S_TEXT/ASCII"] = "srt",
            ["S_TEXT/SSA"] = "ssa",
            ["S_TEXT/ASS"] = "ass",
            ["S_SSA"] = "ssa",
            ["S_ASS"] = "ass",
        };

        public MkvSubtitleExtractor(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            SetMkvtoolnixProgramPaths();
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

        public async Task<string> ExtractSubtitles(string inputFilePath, Track track, string outputDirectory, CancellationToken cancellationToken)
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var subtitlesFileName = $"{Path.GetFileNameWithoutExtension(inputFilePath)}_{track.Id}.{GetFileExtension(track)}";

            var result = await Cli.Wrap(mkvextractPath)
                .WithArguments($"tracks \"{inputFilePath}\" {track.Id}:\"{subtitlesFileName}\"")
                .WithWorkingDirectory(outputDirectory)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync(cancellationToken);

            return Path.Combine(outputDirectory, subtitlesFileName);
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
            SetMkvtoolnixProgramPaths();
        }

        private void SetMkvtoolnixProgramPaths()
        {
            var mkvPath = settingsProvider.GetSettings().MkvToolnixFolder;
            mvkmergePath = Path.Combine(mkvPath, "mkvmerge");
            mkvextractPath = Path.Combine(mkvPath, "mkvextract");
        }
    }
}