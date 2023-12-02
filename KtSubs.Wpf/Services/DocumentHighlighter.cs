using KtSubs.Infrastructure.Entries;
using System.Collections.Generic;
using System.Windows.Documents;

namespace KtSubs.Wpf.Services
{
    public class DocumentHighlighter : IDocumentHighlighter
    {
        public void Highlight(FlowDocument document, DisplayEntry displayEntry, HashSet<int> indexesToSelect)
        {
            if (displayEntry.HasMultipleContentLines)
            {
                MarkMultipleLines(document, indexesToSelect);
            }
            else
            {
                MarkSingleLine(document, indexesToSelect);
            }
        }

        private static void MarkSingleLine(FlowDocument document, HashSet<int> indexesToSelect)
        {
            var block = document.Blocks.FirstBlock;
            var paragraph = block as Paragraph;
            var wordIndex = 1;
            MarkInParagraphWithContent(ref wordIndex, indexesToSelect, paragraph);
        }

        private static void MarkInParagraphWithContent(ref int wordIndex, HashSet<int> indexesToSelect, Paragraph? paragraph)
        {
            var inlines = paragraph?.Inlines;
            if (inlines == null)
                return;

            var index = 0;
            foreach (var inline in inlines)
            {
                bool isWord = index % 2 == 0;
                if (isWord)
                {
                    if (indexesToSelect.Contains(wordIndex))
                    {
                        inline.Foreground = Globals.Brushes.HighlightedText;
                    }
                    else
                    {
                        inline.Foreground = Globals.Brushes.NormalText;
                    }
                    wordIndex++;
                }

                index++;
            }
        }

        private static void MarkMultipleLines(FlowDocument document, HashSet<int> indexesToSelect)
        {
            var index = 0;
            var wordIndex = 1;
            foreach (var block in document.Blocks)
            {
                if (block is not Paragraph paragraph)
                    return;

                bool isContentParagraph = index % 2 == 1;
                index++;

                if (isContentParagraph)
                    MarkInParagraphWithContent(ref wordIndex, indexesToSelect, paragraph);
            }
        }
    }
}