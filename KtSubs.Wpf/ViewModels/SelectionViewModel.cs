using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KtSubs.Core.Entries;
using KtSubs.Core.Selection;
using KtSubs.Core.Services;
using KtSubs.Infrastructure.Entries;
using KtSubs.Wpf.Globals;
using KtSubs.Wpf.Messages;
using KtSubs.Wpf.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace KtSubs.Wpf.ViewModels
{
    public class SelectionViewModel : ObservableObject, IRequestClose, IWindowActivationHandler
    {
        private string searchPattern;

        public string SearchPattern { get => searchPattern; set => SetProperty(ref searchPattern, value); }

        private FlowDocument subtitlesContent;

        public FlowDocument SubtitlesContent
        {
            get { return subtitlesContent; }
            set { SetProperty(ref subtitlesContent, value); }
        }

        private string entryTimeStamp;

        public string EntryTimeStamp { get => entryTimeStamp; set => SetProperty(ref entryTimeStamp, value); }

        public RelayCommand ConfirmSelectionCommand { get; }
        public ICommand CancelSelection { get; }

        public ICommand NextEntryCommand { get; }
        public ICommand PreviousEntryCommand { get; }
        public ICommand IncludePreviousEntryCommand { get; }
        public ICommand IncludeNextEntryCommand { get; }
        public ICommand OpenLayersSettingsCommand { get; }

        private HashSet<int> lastSelectionNumbers = new();
        private HashSet<ISelection> lastSelections = new();
        private int entryIndex;
        private int nextIncluded = 0;
        private int previousIncluded = 0;
        private readonly IEventAggregator eventAggregator;
        private readonly SubtitlesEntries subtitlesEntries;
        private readonly IEntryDocumentCreator documentCreator;
        private readonly IDocumentHighlighter documentHiglighter;
        private readonly IDialogService dialogService;
        private readonly LayersSettings layersSettings;

        public event CancelEventHandler? CloseHandler;

        public DisplayEntry displayEntry;

        public SelectionViewModel(IEventAggregator eventAggregator,
                                  SubtitlesEntries subtitlesEntries,
                                  IEntryDocumentCreator documentCreator,
                                  IDocumentHighlighter documentHiglighter,
                                  IDialogService dialogService,
                                  LayersSettings layersSettings)
        {
            entryTimeStamp = "";
            searchPattern = "";
            this.eventAggregator = eventAggregator;
            this.subtitlesEntries = subtitlesEntries;
            this.documentCreator = documentCreator;
            this.documentHiglighter = documentHiglighter;
            this.dialogService = dialogService;
            this.layersSettings = layersSettings;
            displayEntry = new();
            subtitlesContent = documentCreator.Create(displayEntry);
            PropertyChanged += PropertyChangedHandler;
            ConfirmSelectionCommand = new RelayCommand(ConfirmWordsSelection, CanConfirmWordsSelection);
            NextEntryCommand = new RelayCommand(NextEntry);
            PreviousEntryCommand = new RelayCommand(PreviousEntry);
            IncludeNextEntryCommand = new RelayCommand(IncludeNextEntry);
            IncludePreviousEntryCommand = new RelayCommand(IncludePreviousEntry);
            CancelSelection = new RelayCommand(OnCancelSelection);
            OpenLayersSettingsCommand = new RelayCommand(HandleOpenLayersSettings);
            this.layersSettings.LayersSettingsChanged += HandleLayersSettingsChange;
        }

        private void HandleLayersSettingsChange(object? sender, EventArgs e)
        {
            var leftIndex = Math.Max(0, entryIndex - previousIncluded);
            var rightIndex = Math.Min(subtitlesEntries.EntriesCount, entryIndex + nextIncluded);
            var activeContent = subtitlesEntries.GetEntries(leftIndex, rightIndex)
                                                    .Where(x => layersSettings.IsLayerActive(x.Layer));

            displayEntry = new DisplayEntry(activeContent);
            SubtitlesContent = documentCreator.Create(displayEntry);
            lastSelections = new();
            lastSelectionNumbers = new();
            SearchPattern = "";
        }

        private void HandleOpenLayersSettings()
        {
            dialogService.ShowDialog<LayersSettingsViewModel>();
        }

        private void IncludePreviousEntry()
        {
            HashSet<ISelection>? offsetedSelections = null;

            previousIncluded++;
            int includeIndex = entryIndex - previousIncluded;

            while (includeIndex >= 0)
            {
                var result = subtitlesEntries.GetEntry(includeIndex);
                if (result == null)
                    break;

                var activeLayersContent = result.GroupOfEntryContent.Where(x => layersSettings.IsLayerActive(x.Layer));

                var nameSelectionGroupPair = GetNameSelectionGroupPair(displayEntry);
                var includeResult = displayEntry.IncludeLeft(activeLayersContent);

                if (includeResult.HasIncluded)
                {
                    offsetedSelections = GetOffsetedSelections(nameSelectionGroupPair, includeResult);
                    break;
                }

                previousIncluded++;
                includeIndex = entryIndex - previousIncluded;
            }

            if (offsetedSelections == null)
            {
                return;
            }

            IncludeEntry(offsetedSelections);
        }

        private void IncludeNextEntry()
        {
            HashSet<ISelection>? offsetedSelections = null;

            nextIncluded++;
            var includeIndex = entryIndex + nextIncluded;
            while (includeIndex < subtitlesEntries.EntriesCount)
            {
                var result = subtitlesEntries.GetEntry(includeIndex);

                if (result != null)
                {
                    var activeLayersContent = result.GroupOfEntryContent.Where(x => layersSettings.IsLayerActive(x.Layer));
                    var nameSelectionGroupPair = GetNameSelectionGroupPair(displayEntry);
                    var includeResult = displayEntry.IncludeRight(activeLayersContent);

                    if (includeResult.HasIncluded)
                    {
                        offsetedSelections = GetOffsetedSelections(nameSelectionGroupPair, includeResult);
                        break;
                    }
                }

                nextIncluded++;
                includeIndex = entryIndex + nextIncluded;
            }

            if (offsetedSelections == null)
            {
                return;
            }

            IncludeEntry(offsetedSelections);
        }

        private void IncludeEntry(HashSet<ISelection> offsetedSelections)
        {
            lastSelections = offsetedSelections;
            lastSelectionNumbers = new HashSet<int>(GetNumbers(offsetedSelections));
            SearchPattern = GetSelectionsPattern(offsetedSelections);
            SubtitlesContent = documentCreator.Create(displayEntry);
            MarkSelectedWords(lastSelectionNumbers);
        }

        private static HashSet<ISelection> GetOffsetedSelections(Dictionary<string, List<ISelection>> nameSelectionGroupPair, IncludeResult includeResult)
        {
            var offsetedSelections = new HashSet<ISelection>();

            foreach (var nameSelectionPair in nameSelectionGroupPair)
            {
                foreach (var selection in nameSelectionPair.Value)
                {
                    var offset = includeResult[nameSelectionPair.Key];
                    if (offset != 0)
                    {
                        offsetedSelections.Add(selection.GetSelectionIncreasedBy(offset));
                    }
                    else
                    {
                        offsetedSelections.Add(selection);
                    }
                }
            }

            return offsetedSelections;
        }

        private Dictionary<string, List<ISelection>> GetNameSelectionGroupPair(DisplayEntry entry)
        {
            var result = new Dictionary<string, List<ISelection>>();
            foreach (var selection in lastSelections)
            {
                var selectionNumbers = selection.SelectedWordNumbers.ToArray();
                var selectionLastWordIndex = selectionNumbers[^1] - 1;
                var layerName = entry.GetLayerNameAtWordIndex(selectionLastWordIndex);
                if (layerName == null)
                {
                    continue;
                }

                if (result.ContainsKey(layerName))
                {
                    result[layerName].Add(selection);
                }
                else
                {
                    result[layerName] = new List<ISelection> { selection };
                }
            }
            return result;
        }

        private string GetSelectionsPattern(HashSet<ISelection> selections)
        {
            var sb = new StringBuilder();
            var divider = "";

            foreach (var selection in selections)
            {
                sb.Append($"{divider}{selection.GetPattern()}");
                divider = ",";
            }

            return sb.ToString();
        }

        private bool CanConfirmWordsSelection()
        {
            return lastSelections.Count > 0;
        }

        public void ConfirmWordsSelection()
        {
            var selectedWords = new List<string>();
            foreach (var selection in lastSelections)
            {
                selectedWords.Add(selection.GetSelectedValue(displayEntry));
            }
            var message = new AddSelectedWordsMessage(selectedWords);
            eventAggregator.SendMessage(message);
            CloseHandler?.Invoke(this, new CancelEventArgs());
        }

        public void OnCancelSelection()
        {
            CloseHandler?.Invoke(this, new CancelEventArgs());
        }

        public void NextEntry()
        {
            var nextIndex = entryIndex + 1;
            if (nextIndex >= subtitlesEntries.EntriesCount)
                return;

            var entriesResult = subtitlesEntries.GetClosestEntryToRight(nextIndex);
            if (entriesResult == null)
                return;

            ChangeEntry(entriesResult);
        }

        public void PreviousEntry()
        {
            var prevIndex = entryIndex - 1;
            if (prevIndex < 0)
                return;

            var entriesResult = subtitlesEntries.GetClosestEntryToLeft(prevIndex);
            if (entriesResult == null)
                return;

            ChangeEntry(entriesResult);
        }

        private void ChangeEntry(EntriesResult entriesResult)
        {
            entryIndex = entriesResult.Index;
            displayEntry = new DisplayEntry(entriesResult.GroupOfEntryContent);
            SubtitlesContent = documentCreator.Create(displayEntry);
            SearchPattern = "";
            nextIncluded = 0;
            previousIncluded = 0;
            EntryTimeStamp = $"{entriesResult.Appear} - {entriesResult.Disappear}";
        }

        private void PropertyChangedHandler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchPattern))
                SearchPatternChanged();
        }

        private void SearchPatternChanged()
        {
            var lastWordNumber = displayEntry.NumberOfWords;
            var pattern = new Pattern(SearchPattern);
            var selections = pattern.ToSelections(lastWordNumber);
            var selectedNumbers = new HashSet<int>(GetNumbers(selections));

            if (!selectedNumbers.SetEquals(lastSelectionNumbers))
            {
                MarkSelectedWords(selectedNumbers);
            }
            lastSelectionNumbers = selectedNumbers;
            lastSelections = selections;
            ConfirmSelectionCommand.NotifyCanExecuteChanged();
        }

        private IEnumerable<int> GetNumbers(HashSet<ISelection> selections)
        {
            foreach (var selection in selections)
            {
                foreach (var wordNumber in selection.SelectedWordNumbers)
                {
                    yield return wordNumber;
                }
            }
        }

        private void MarkSelectedWords(HashSet<int> indexesToSelect)
        {
            documentHiglighter.Highlight(SubtitlesContent, displayEntry, indexesToSelect);
            OnPropertyChanged(nameof(SubtitlesContent));
        }

        public void OnWindowActivated(WindowParameters parameters)
        {
            var time = (TimeSpan)parameters.Get(WindowParameterNames.Time);
            var entryAndIndex = subtitlesEntries.GetEntry(time);

            if (entryAndIndex != null)
            {
                displayEntry = new DisplayEntry(entryAndIndex.GroupOfEntryContent);
                entryIndex = entryAndIndex.Index;
                EntryTimeStamp = $"{entryAndIndex.Appear} - {entryAndIndex.Disappear}";
                SubtitlesContent = documentCreator.Create(displayEntry);
            }
        }

        public void OnClose()
        {
            var message = new CloseSelectionWindowMessage();
            eventAggregator.SendMessage(message);
            layersSettings.LayersSettingsChanged -= HandleLayersSettingsChange;
        }
    }
}