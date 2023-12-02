using CommunityToolkit.Mvvm.Input;
using KtSubs.Infrastructure.Services.MuxedSubtitles;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace KtSubs.Wpf.ViewModels
{
    public class MuxedSubtitlesSelectorViewModel : Dialog, IDisposable
    {
        private List<Track> tracks;
        private readonly string pathToFileWithMuxedSubs;
        private readonly MkvSubtitleExtractor mkvSubtitleExtractor;

        public List<Track> Tracks
        {
            get { return tracks; }
            set { SetProperty(ref tracks, value); }
        }

        private Track? selectedTrack;

        public Track? SelectedTrack
        {
            get { return selectedTrack; }
            set
            {
                if (SetProperty(ref selectedTrack, value))
                {
                    ExtractSubsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool isExtracting;

        public bool IsExtracting { get => isExtracting; set => SetProperty(ref isExtracting, value); }
        public IRelayCommand ExtractSubsCommand { get; }
        public ICommand CancelExtractingCommand { get; }

        private CancellationTokenSource cancellationTokenSource = new();
        private bool disposedValue;
        public string? ExtractedSubtitlesPath { get; private set; }

        public MuxedSubtitlesSelectorViewModel(List<Track> tracks, string pathToFileWithMuxedSubs, MkvSubtitleExtractor mkvSubtitleExtractor)
        {
            this.tracks = tracks;
            this.pathToFileWithMuxedSubs = pathToFileWithMuxedSubs;
            this.mkvSubtitleExtractor = mkvSubtitleExtractor;
            IsExtracting = false;
            ExtractSubsCommand = new RelayCommand(ExtractSubs, () => this.tracks.Count > 0 && SelectedTrack != null);
            CancelExtractingCommand = new RelayCommand(CancelExtraction);
        }

        private async void ExtractSubs()
        {
            IsExtracting = true;
            if (SelectedTrack == null) return;

            try
            {
                ExtractedSubtitlesPath = await mkvSubtitleExtractor.ExtractSubtitles(pathToFileWithMuxedSubs, SelectedTrack, @"C:\KtSubs_muxed\", cancellationTokenSource.Token);
                Close(true);
            }
            catch (OperationCanceledException)
            {
                Close(false);
            }
        }

        private void CancelExtraction()
        {
            cancellationTokenSource.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancellationTokenSource.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}