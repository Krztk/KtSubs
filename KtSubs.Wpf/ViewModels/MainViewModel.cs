using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KtSubs.Core.Exceptions;
using KtSubs.Core.Services;
using KtSubs.Core.Settings;
using KtSubs.Infrastructure.Services.MuxedSubtitles;
using KtSubs.Infrastructure.Services.Vlc;
using KtSubs.Wpf.Globals;
using KtSubs.Wpf.Messages;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace KtSubs.Wpf.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string title = "KtSubs";
        private readonly IDialogService dialogService;
        private readonly VlcStatusService vlcStatusService;
        private readonly IEventAggregator eventAggregator;
        private readonly MkvSubtitleExtractor mkvSubtitleExtractor;
        private readonly ISubtitlesStore subtitlesStore;
        private readonly ISettingsProvider settingsProvider;
        private bool loading;

        public bool Loading
        {
            get { return loading; }
            set { SetProperty(ref loading, value); }
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand OpenSelectionWindowCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand OpenFileCommand { get; }
        private bool subsLoaded = false;

        private bool hasPausedVlc = false;
        private Settings settings;
        public ObservableCollection<string> SelectedWords { get; set; } = new();
        public ICommand DeleteWordCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenLayersSettingsCommand { get; }

        public MainViewModel(IDialogService dialogService,
                             VlcStatusService vlcStatusService,
                             IEventAggregator eventAggregator,
                             MkvSubtitleExtractor mkvSubtitleExtractor,
                             ISubtitlesStore subtitlesStore,
                             ISettingsProvider settingsProvider)
        {
            this.dialogService = dialogService;
            this.vlcStatusService = vlcStatusService;
            this.eventAggregator = eventAggregator;
            this.mkvSubtitleExtractor = mkvSubtitleExtractor;
            this.subtitlesStore = subtitlesStore;
            SaveCommand = new RelayCommand(SaveWords);
            this.eventAggregator.RegisterHandler<AddSelectedWordsMessage>(OnNewSelectedWords);
            this.eventAggregator.RegisterHandler<HotkeyPressedMessage>(OnHotkeyPressed);
            this.eventAggregator.RegisterHandler<CloseSelectionWindowMessage>(OnCloseSelectionWindow);
            OpenSelectionWindowCommand = new RelayCommand(OpenSelectionWindow);
            OpenFileCommand = new RelayCommand(OnOpenFile);
            this.settingsProvider = settingsProvider;
            settings = settingsProvider.GetSettings();
            settingsProvider.SettingsChanged += OnSettingsChanged;
            DeleteWordCommand = new RelayCommand<int>(HandleWordDeletion);
            OpenSettingsCommand = new RelayCommand(HandleOpenSettings);
            OpenLayersSettingsCommand = new RelayCommand(HandleOpenLayerSettings);
        }

        private void HandleOpenLayerSettings()
        {
            dialogService.ShowDialog<LayersSettingsViewModel>();
        }

        private void HandleOpenSettings()
        {
            dialogService.ShowDialog<SettingsViewModel>();
        }

        private void HandleWordDeletion(int selectedIndex)
        {
            if (selectedIndex == -1)
                return;

            SelectedWords.RemoveAt(selectedIndex);
        }

        private async void OnCloseSelectionWindow(CloseSelectionWindowMessage message)
        {
            try
            {
                if (settings.PauseVideoWhenSelecting && hasPausedVlc)
                    await vlcStatusService.Play();
            }
            catch (VlcConnectionException e)
            {
                MessageBox.Show(e.Message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            settings = settingsProvider.GetSettings();
        }

        private async void OnOpenFile()
        {
            var result = dialogService.OpenFileDialog();
            if (result == null)
                return;

            Loading = true;
            var pathToSubs = result.Path;
            var extension = Path.GetExtension(result.Path);
            if (extension == ".mkv")
            {
                if (!mkvSubtitleExtractor.HasMkvToolkit())
                {
                    Loading = false;
                    MessageBox.Show("MkvToolkit not detected. Check settings.");
                    return;
                }
                var tracks = await mkvSubtitleExtractor.GetSubtitlesTrackData(result.Path);
                if (tracks.Count == 0)
                {
                    Loading = false;
                    MessageBox.Show("File does not contain any muxed subtitles");
                    return;
                }
                var muxedSubsVm = new MuxedSubtitlesSelectorViewModel(tracks, result.Path, mkvSubtitleExtractor);
                var confirmed = dialogService.ShowDialog(muxedSubsVm);
                if (!confirmed)
                {
                    Loading = false;
                    return;
                }
;
                pathToSubs = muxedSubsVm.ExtractedSubtitlesPath;
            }

            extension = Path.GetExtension(pathToSubs);
            if (extension == ".srt" || extension == ".ass" || extension == ".ssa")
            {
                LoadSubtitles(pathToSubs);
            }
            else
            {
                Loading = false;
                MessageBox.Show("Not supported extension");
            }
        }

        private void LoadSubtitles(string path)
        {
            try
            {
                subtitlesStore.Load(path);
                subsLoaded = true;
            }
            catch (SubtitlesReadingException)
            {
                MessageBox.Show("It seems there was a problem while trying to read the subtitles.",
                    "Subtitles content error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            Loading = false;
        }

        private void OnHotkeyPressed(HotkeyPressedMessage obj)
        {
            OpenSelectionWindow();
        }

        private void SaveWords()
        {
            var result = dialogService.ShowSaveFileDialog();
            if (result == null)
                return;

            using (var sw = new StreamWriter(result.Path, false, System.Text.Encoding.UTF8))
            {
                foreach (var value in SelectedWords)
                {
                    sw.WriteLine(value);
                }
            }
        }

        private void OnNewSelectedWords(AddSelectedWordsMessage message)
        {
            foreach (var word in message.words)
            {
                SelectedWords.Add(word);
            }
        }

        public async void OpenSelectionWindow()
        {
            try
            {
                if (settings.PauseVideoWhenSelecting)
                {
                    hasPausedVlc = await vlcStatusService.Pause();
                }

                if (!subsLoaded)
                {
                    SystemSounds.Beep.Play();
                    return;
                }

                var parameters = new WindowParameters();
                var ms = await vlcStatusService.GetTimeMs();
                var time = new TimeSpan(0, 0, 0, 0, ms);
                parameters.Add(WindowParameterNames.Time, time);
                dialogService.Show<SelectionViewModel>(parameters);
            }
            catch (VlcConnectionException e)
            {
                MessageBox.Show(e.Message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}