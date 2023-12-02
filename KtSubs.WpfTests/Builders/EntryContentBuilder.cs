using KtSubs.Core.Entries;
using System.Collections.Generic;
using System.Linq;

namespace KtSubs.WpfTests.ViewModels.SelectionViewModelTests
{
    public class EntryContentBuilder
    {
        private int id;
        private string layerName = "DEFAULT";
        private List<string> words = new();

        public EntryContentBuilder(int id)
        {
            this.id = id;
        }

        public EntryContentBuilder SetLayerName(string layerName)
        {
            this.layerName = layerName;
            return this;
        }

        public EntryContentBuilder SetContentStr(string contentText)
        {
            words = contentText.Split(' ').ToList();
            return this;
        }

        public EntryContent Build()
        {
            return new EntryContent(id, layerName, words);
        }
    }
}