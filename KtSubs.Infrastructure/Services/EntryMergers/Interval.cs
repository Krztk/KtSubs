namespace KtSubs.Infrastructure.Services.EntryMergers
{
    internal class Interval
    {
        public TimeSpan Start { get; }
        public TimeSpan End { get; }

        public Interval(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        public bool Intersects(TimeSpan start, TimeSpan end) =>
            this.Start < end
            && this.End > start;
    }
}