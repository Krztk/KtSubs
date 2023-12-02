using FluentAssertions;
using KtSubs.Core.Entries;
using KtSubs.Core.Services;
using KtSubs.Wpf;
using KtSubs.Wpf.Globals;
using KtSubs.Wpf.Services;
using KtSubs.Wpf.ViewModels;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KtSubs.WpfTests.ViewModels.SelectionViewModelTests
{
    public class IncludeTests
    {
        /// <summary>
        /// <br>Layers are sorted in alphabetical order</br>
        /// <br>0 -> 3</br>
        /// <br>[Dialogue]</br>
        /// <br>Hello? (Id: 1)</br>
        /// <br/>
        /// <br>3 -> 5</br>
        /// <br>[Info]</br>
        /// <br>Thanks for downloading our subtitles (Id: 3)</br>
        /// <br/>
        /// <br>5 -> 8</br>
        /// <br>[Dialogue]</br>
        /// <br>Is anyone there? (Id: 2)</br>
        /// <br>[Info]</br>
        /// <br>Thanks for downloading our subtitles (Id: 3)</br>
        /// <br/>
        /// <br>8 -> 10</br>
        /// <br>[Info]</br>
        /// <br>Thanks for downloading our subtitles (Id: 3)</br>
        /// </summary>
        public SelectionViewModel vm;

        public IncludeTests()
        {
            var eventAggregator = Substitute.For<IEventAggregator>();
            var documentCreator = Substitute.For<IEntryDocumentCreator>();
            var documentHighlighter = Substitute.For<IDocumentHighlighter>();
            var dialogService = Substitute.For<IDialogService>();
            var subtitlesStore = new TestStore();
            var layerSettingsManager = new LayersSettingsManager(subtitlesStore);
            vm = new SelectionViewModel(eventAggregator,
                                        new SubtitlesEntryFinder(new TestStore(), layerSettingsManager),
                                        documentCreator,
                                        documentHighlighter,
                                        dialogService,
                                        layerSettingsManager);
        }

        [Fact]
        public void ShouldNotHaveMultipleContentLines()
        {
            ActivateView(4);
            vm.displayEntry.HasMultipleContentLines.Should().BeFalse();
        }

        [Fact]
        public void ShouldOnlyIncludePreviousEntryContentThatIsNotAlreadyPresent()
        {
            ActivateView(6);

            vm.SearchPattern = "1";
            vm.IncludePreviousEntryCommand.Execute(null);

            var entries = vm.displayEntry.Entries.Values.SelectMany(x => x).Select(x => x.Id);
            entries.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
            vm.SearchPattern.Should().BeEquivalentTo("2");
        }

        [Fact]
        public void ShouldNotIncludeNextEntryBecauseThereIsNotEntryContentWhichIsNotAlreadyPresent()
        {
            ActivateView(6);

            vm.SearchPattern = "1";
            vm.IncludeNextEntryCommand.Execute(null);

            var entries = vm.displayEntry.Entries.Values.SelectMany(x => x).Select(x => x.Id);
            entries.Should().BeEquivalentTo(new List<int> { 2, 3 });
            vm.SearchPattern.Should().BeEquivalentTo("1");
        }

        [Fact]
        public void ShouldIncludeNextEntry()
        {
            ActivateView(1);

            vm.SearchPattern = "1";
            vm.IncludeNextEntryCommand.Execute(null);

            var entries = vm.displayEntry.Entries.Values.SelectMany(x => x).Select(x => x.Id);
            entries.Should().BeEquivalentTo(new List<int> { 1, 3 });
            vm.SearchPattern.Should().BeEquivalentTo("1");
        }

        private void ActivateView(int seconds)
        {
            var windowParams = new WindowParameters();
            windowParams.Add(WindowParameterNames.Time, new TimeSpan(0, 0, seconds));
            vm.OnWindowActivated(windowParams);
        }

        public class TestStore : ISubtitlesStore
        {
            public IReadOnlyList<IEntry> Entries { get; } = PrepareTestData();

            public IReadOnlySet<string> Layers { get; } = new HashSet<string> { "Dialogue", "Info" };

            public event EventHandler? SubtitlesLoaded;

            private static List<IEntry> PrepareTestData()
            {
                var dialogueEntryContent1 = new EntryContentBuilder(1).SetLayerName("Dialogue").SetContentStr("Hello?").Build();
                var dialogueEntryContent2 = new EntryContentBuilder(2).SetLayerName("Dialogue").SetContentStr("Is anyone there?").Build();
                var infoEntryContent = new EntryContentBuilder(3).SetLayerName("Info").SetContentStr("Thanks for downloading our subtitles").Build();

                return new List<IEntry>
                {
                    new EntryBuilder(0, 3).AddContent(dialogueEntryContent1).Build(),
                    new EntryBuilder(3, 5).AddContent(infoEntryContent).Build(),
                    new EntryBuilder(5, 8)
                        .AddContent(dialogueEntryContent2)
                        .AddContent(infoEntryContent)
                        .Build(),
                    new EntryBuilder(8, 10).AddContent(infoEntryContent).Build(),
                };
            }

            public void Load(string path)
            {
                throw new NotImplementedException();
            }
        }
    }
}