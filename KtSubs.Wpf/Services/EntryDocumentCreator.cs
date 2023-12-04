using KtSubs.Core.Entries;
using KtSubs.Core.Services;
using KtSubs.Infrastructure.Entries;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace KtSubs.Wpf.Services
{
    public class EntryDocumentCreator : IEntryDocumentCreator
    {
        private const double textFontSize = 16D;
        private const double subscriptFontSize = 12D;
        private const double layerNameFontSize = 14D;
        private readonly LayersSettings layersSettings;

        public EntryDocumentCreator(LayersSettings layerSettings)
        {
            this.layersSettings = layerSettings;
        }

        public FlowDocument Create(DisplayEntry displayEntry)
        {
            var document = new FlowDocument();
            var number = 0;
            foreach (var entry in displayEntry.Entries)
            {
                var layerName = entry.Key;
                if (!layersSettings.IsLayerActive(layerName))
                {
                    continue;
                }

                if (displayEntry.HasMultipleContentLines)
                {
                    var nameParagraph = new Paragraph();
                    var layerNameRun = new Run(layerName)
                    {
                        FontSize = layerNameFontSize,
                        Foreground = Globals.Brushes.LayerName
                    };
                    nameParagraph.Inlines.Add(layerNameRun);
                    document.Blocks.Add(nameParagraph);
                }
                var paragraph = CreateParagraph(entry.Value, ref number);
                document.Blocks.Add(paragraph);
            }

            document.Typography.NumeralStyle = FontNumeralStyle.Lining;
            return document;
        }

        private static Paragraph CreateParagraph(List<EntryContent> groupOfEntryContent, ref int number)
        {
            var inlines = new List<Inline>();
            var whitespace = "";

            foreach (var entryContent in groupOfEntryContent)
            {
                foreach (var word in entryContent.Words)
                {
                    number++;
                    var wordRun = new Run($"{whitespace}{word}");
                    wordRun.FontSize = textFontSize;
                    wordRun.Foreground = Globals.Brushes.NormalText;
                    inlines.Add(wordRun);
                    var numberRun = new Run(number.ToString())
                    {
                        FontSize = subscriptFontSize,
                        Foreground = Globals.Brushes.WordNumber,
                        BaselineAlignment = BaselineAlignment.Subscript
                    };
                    inlines.Add(numberRun);
                    whitespace = " ";
                }
            }

            var paragraph = new Paragraph();
            paragraph.Inlines.AddRange(inlines);
            return paragraph;
        }
    }
}