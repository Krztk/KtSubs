﻿namespace KtSubs.Infrastructure.Services.EntryMergers
{
    internal static class TimeSpanExtensions
    {
        public static IEnumerable<TimeInterval> ToIntervals(this IEnumerable<TimeSpan> dates)
        {
            using var datesEnumerator = dates.GetEnumerator();

            if (!datesEnumerator.MoveNext())
                yield break;

            var start = datesEnumerator.Current;

            while (datesEnumerator.MoveNext())
            {
                yield return new TimeInterval(start, datesEnumerator.Current);
                start = datesEnumerator.Current;
            }
        }
    }
}