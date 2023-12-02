using LanguageExt;
using static LanguageExt.Prelude;

namespace KtSubs.Core.Services
{
    public class SubsHelper
    {
        private static readonly Dictionary<string, SubtitlesType> extensionTypePairs = new()
        {
            [".srt"] = SubtitlesType.Srt,
            [".ssa"] = SubtitlesType.Ssa,
            [".ass"] = SubtitlesType.Ssa,
        };

        public static Option<SubtitlesType> GetSubtitlesType(string path)
        {
            var extension = Path.GetExtension(path);
            if (extensionTypePairs.ContainsKey(extension))
                return extensionTypePairs[extension];

            return None;
        }
    }
}