using CommunityToolkit.Mvvm.ComponentModel;

namespace KtSubs.Wpf
{
    public record SaveFileDialogResult(string Path);
    public record OpenFileDialogResult(string Path);

    public interface IDialogService
    {
        OpenFileDialogResult? OpenFileDialog();

        void Show<T>(WindowParameters windowParams) where T : ObservableObject;

        bool ShowDialog<T>() where T : ObservableObject;

        bool ShowDialog<T>(T vm) where T : ObservableObject;

        SaveFileDialogResult? ShowSaveFileDialog();
    }
}