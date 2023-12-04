using Autofac;
using KtSubs.Core.Services;
using KtSubs.Core.Settings;
using KtSubs.Infrastructure.Services;
using KtSubs.Infrastructure.Services.MuxedSubtitles;
using KtSubs.Infrastructure.Services.Readers;
using KtSubs.Infrastructure.Services.Vlc;
using KtSubs.Wpf.Services;
using KtSubs.Wpf.ViewModels;
using KtSubs.Wpf.Views;
using Serilog;
using System.Windows;

namespace KtSubs.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer? container;

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = new ContainerBuilder();
            builder.Register<ILogger>((c, p) => Log.Logger).SingleInstance();
            builder.RegisterType<ViewProvider>().SingleInstance();
            builder.RegisterType<SelectionViewModel>();
            builder.RegisterType<LayersSettingsViewModel>();
            builder.RegisterType<SettingsViewModel>();

            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<SubtitlesEntryFinder>().SingleInstance();
            builder.RegisterType<LayersSettingsManager>().SingleInstance();

            builder.RegisterType<MainViewModel>().SingleInstance();
            builder.RegisterType<MainView>().SingleInstance();

            builder.RegisterType<SettingsView>();
            builder.RegisterType<SelectionView>();
            builder.RegisterType<MuxedSubtitlesSelectorView>();
            builder.RegisterType<LayersSettingsView>();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("Connection", "close");
            builder.Register(c => new VlcStatusService(httpClient)).SingleInstance();
            builder.RegisterType<MkvSubtitleExtractor>().SingleInstance();
            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
            builder.RegisterType<ReaderFactory>().SingleInstance();
            builder.RegisterType<SubtitlesStore>().As<ISubtitlesStore>().SingleInstance();
            builder.RegisterType<DocumentHighlighter>().As<IDocumentHighlighter>().SingleInstance();
            builder.RegisterType<EntryDocumentCreator>().As<IEntryDocumentCreator>().SingleInstance();

            container = builder.Build();

            var settingsProvider = container.Resolve<ISettingsProvider>();
            settingsProvider.SettingsChanged += HandleSettingsChanged;
            var vlcService = container.Resolve<VlcStatusService>();
            var settings = settingsProvider.GetSettings();
            vlcService.SetAccessSettings(settings.WebInterfacePassword, settings.Port);
            MainWindow = container.Resolve<MainView>();
            MainWindow.DataContext = container.Resolve<MainViewModel>();

            base.OnStartup(e);
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            container?.Dispose();

            base.OnExit(e);
        }

        private void HandleSettingsChanged(object? sender, System.EventArgs e)
        {
            var settings = (sender as SettingsProvider)?.GetSettings();
            if (settings == null)
            {
                return;
            }

            var vlcService = container.Resolve<VlcStatusService>();
            vlcService.SetAccessSettings(settings.WebInterfacePassword, settings.Port);
        }
    }
}