using KtSubs.Core.Entries;
using System;
using System.Collections.Generic;

namespace KtSubs.WpfTests.ViewModels.SelectionViewModelTests
{
    public class EntryBuilder
    {
        private int appearAtSec;
        private int disappearAtSec;
        private List<EntryContent> entryContentGroup = new();

        public EntryBuilder(int appearAtSec, int disappearAtSec)
        {
            this.appearAtSec = appearAtSec;
            this.disappearAtSec = disappearAtSec;
        }

        public EntryBuilder AddContent(EntryContent content)
        {
            entryContentGroup.Add(content);
            return this;
        }

        public IEntry Build()
        {
            if (entryContentGroup.Count == 0) throw new Exception("Cannot build entry without content");
            if (entryContentGroup.Count == 1) return new Entry(TimeSpan.FromSeconds(appearAtSec), TimeSpan.FromSeconds(disappearAtSec), entryContentGroup[0]);

            return new EntryGroup(TimeSpan.FromSeconds(appearAtSec), TimeSpan.FromSeconds(disappearAtSec), entryContentGroup);
        }
    }
}