using KtSubs.Core.Services;
using KtSubs.Wpf;
using KtSubs.Wpf.Globals;
using KtSubs.Wpf.Messages;
using KtSubs.Wpf.Services;
using KtSubs.Wpf.ViewModels;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KtSubs.WpfTests.ViewModels.SelectionViewModelTests
{
    public class SelectionTests
    {
        private readonly SelectionViewModel vm;
        private readonly IEventAggregator eventAggregator;

        public SelectionTests()
        {
            var documentCreator = Substitute.For<IEntryDocumentCreator>();
            var documentHighlighter = Substitute.For<IDocumentHighlighter>();
            var dialogService = Substitute.For<IDialogService>();
            eventAggregator = Substitute.For<IEventAggregator>();
            var subtitlesStore = new TestStore();
            var layersSettingsManager = new LayersSettingsManager(subtitlesStore);
            vm = new SelectionViewModel(eventAggregator,
                                        new SubtitlesEntryFinder(subtitlesStore, layersSettingsManager),
                                        documentCreator,
                                        documentHighlighter,
                                        dialogService,
                                        layersSettingsManager);
        }

        [Fact]
        public void ShouldHaveContentOnActivation()
        {
            //Arrange
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 0, 21));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "1,2";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "Second", "Word" })));
        }

        [Fact]
        public void ShouldActivateWithEntryThatIsCloserToTimeStamp()
        {
            //Arrange
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 0, 14));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "1";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "First" })));
        }

        [Fact]
        public void ShouldActivateWithEntryThatIsLastAndTimestampIsGreaterThanThatEntryDisappearAt()
        {
            //Arrange
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 1, 58));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "1";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "Third" })));
        }

        [Fact]
        public void ShouldSelectSentence()
        {
            //Arrange
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 1, 58));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "1-3";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "Third The end" })));
        }

        [Fact]
        public void ShouldSelectMultipleConsecutiveWords()
        {
            //Arrange
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 1, 58));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "1-3*";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "Third", "The", "end" })));
        }

        [Fact]
        public void ShouldSelectWordsWithoutPunctuationCharacters()
        {
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 0, 6));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "2";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string> { "word" })));
        }

        [Fact]
        public void ShouldNotSelectAnyWordsIfPatternDoNotMatchAnything()
        {
            var windowParams = CreateSelectionViewModelActivationParameters(new TimeSpan(0, 0, 6));

            //Act
            vm.OnWindowActivated(windowParams);
            vm.SearchPattern = "20";
            vm.ConfirmWordsSelection();

            //Assert
            eventAggregator
                .Received()
                .SendMessage(Arg.Is<AddSelectedWordsMessage>(x => x.words.SequenceEqual(new List<string>())));
        }

        private WindowParameters CreateSelectionViewModelActivationParameters(TimeSpan timeSpan)
        {
            var windowParams = new WindowParameters();
            windowParams.Add(WindowParameterNames.Time, timeSpan);
            return windowParams;
        }
    }
}