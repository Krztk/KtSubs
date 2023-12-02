namespace KtSubs.Core.Extensions
{
    public static class StringExtensions
    {
        public static string RemovePunctuationCharactersFromEnd(this string str)
        {
            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (!Char.IsPunctuation(str[i]))
                {
                    if (i == str.Length - 1)
                        return str;

                    return str[..(i + 1)];
                }
            }

            return String.Empty;
        }

        public static string[] GetSubstringsAtIndexes(this string line, int lastIndex, IList<int> indexes, IList<char> delimiters, int startIndex = 0)
        {
            var values = new string[indexes.Count];
            var maxSubstringIndex = indexes.Max();
            var previousDelimiterIndex = startIndex - 1;
            for (int substringIndex = 0, delimiterIndex = 0;
                 substringIndex <= maxSubstringIndex && delimiterIndex < delimiters.Count;
                 substringIndex++, delimiterIndex++)
            {
                previousDelimiterIndex = SetValueAndGetPreviousDelimiterIndex(
                    line,
                    lastIndex,
                    indexes,
                    delimiters[delimiterIndex],
                    values,
                    ref previousDelimiterIndex,
                    ref substringIndex);
            }

            return values;
        }

        public static string[] GetSubstringsAtIndexes(this string line, int lastIndex, IList<int> indexes, char delimiter, int startIndex = 0)
        {
            var values = new string[indexes.Count];
            var maxSubstringIndex = indexes.Max();
            var previousDelimiterIndex = startIndex - 1;
            for (int substringIndex = 0; substringIndex <= maxSubstringIndex; substringIndex++)
            {
                previousDelimiterIndex = SetValueAndGetPreviousDelimiterIndex(
                    line,
                    lastIndex,
                    indexes,
                    delimiter,
                    values,
                    ref previousDelimiterIndex,
                    ref substringIndex);
            }
            return values;
        }

        private static int SetValueAndGetPreviousDelimiterIndex(
            string line,
            int lastIndex,
            IList<int> indexes,
            char delimiter,
            string[] values,
            ref int previousDelimiterIndex,
            ref int subStringIndex)
        {
            int delimiterIndex = line.IndexOf(delimiter, previousDelimiterIndex + 1);
            var length = delimiterIndex - previousDelimiterIndex - 1;

            var indexPosition = indexes.IndexOf(subStringIndex);
            if (indexPosition != -1)
            {
                if (subStringIndex == lastIndex)
                {
                    values[indexPosition] = line.Substring(previousDelimiterIndex + 1);
                }
                else
                    values[indexPosition] = line.Substring(previousDelimiterIndex + 1, length);
            }

            previousDelimiterIndex = delimiterIndex;
            return previousDelimiterIndex;
        }
    }
}