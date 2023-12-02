using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using KtSubs.Wpf.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace KtSubs.Wpf
{
    public class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Window> activeWindows = new();
        private readonly ILifetimeScope lifetimeScope;
        private readonly MainView mainWindow;
        private readonly ViewProvider viewProvider;

        public DialogService(ILifetimeScope lifetimeScope, MainView mainWindow, ViewProvider viewProvider)
        {
            this.lifetimeScope = lifetimeScope;
            this.mainWindow = mainWindow;
            this.viewProvider = viewProvider;
        }

        public SaveFileDialogResult? ShowSaveFileDialog()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = "txt",
                Filter =
                "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            var dialogOkResult = dialog.ShowDialog();
            if (!dialogOkResult.HasValue || !dialogOkResult.Value)
                return null;

            return new SaveFileDialogResult(dialog.FileName);
        }

        public OpenFileDialogResult? OpenFileDialog()
        {
            var dialog = new OpenFileDialog();
            var dialogOkResult = dialog.ShowDialog();
            if (!dialogOkResult.HasValue || !dialogOkResult.Value)
                return null;

            return new OpenFileDialogResult(dialog.FileName);
        }

        public bool ShowDialog<T>() where T : ObservableObject
        {
            var viewModel = lifetimeScope.Resolve(typeof(T)) as ObservableObject;
            if (viewModel == null)
                throw new Exception("View model has not been resolved");

            return ShowDialog(viewModel);
        }

        public bool ShowDialog<T>(T viewModel) where T : ObservableObject
        {
            var view = viewProvider.GetView(viewModel.GetType());
            view.DataContext = viewModel;

            var window = view as Window;
            window ??= new Window()
            {
                MinWidth = 400,
                MinHeight = 200,
                Content = view
            };

            CancelEventHandler closeDialog = (object? sender, CancelEventArgs e) =>
            {
                if (sender is Dialog dialog)
                {
                    window.DialogResult = dialog.DialogStatus;
                }
                window.Close();
            };

            if (viewModel is Dialog dialog)
            {
                dialog.CloseHandler += closeDialog;
            }

            window.Closed += (s, e) => OnDialogClosed(s, e, closeDialog);
            return window.ShowDialog() ?? false;
        }

        private void OnDialogClosed(object? sender, EventArgs e, CancelEventHandler closeDialog)
        {
            var window = sender as Window;
            if (window == null)
                return;

            if (window.Content is FrameworkElement frameworkElement)
            {
                if (frameworkElement.DataContext is Dialog dialog)
                {
                    dialog.CloseHandler -= closeDialog;
                }
            }
        }

        public void Show<T>(WindowParameters windowParams) where T : ObservableObject
        {
            Show(typeof(T), windowParams);
        }

        public void Show(Type viewModelType, WindowParameters windowParams)
        {
            if (activeWindows.ContainsKey(viewModelType))
            {
                var activeWindow = activeWindows[viewModelType];
                if (activeWindow.WindowState == WindowState.Minimized)
                {
                    activeWindow.WindowState = WindowState.Normal;
                }

                if (activeWindow.Content is FrameworkElement frameworkElement
                    && frameworkElement.DataContext is IWindowActivationHandler contentDataContext)
                {
                    contentDataContext.OnWindowActivated(windowParams);
                }
                activeWindow.Activate();
                return;
            }

            if (lifetimeScope.Resolve(viewModelType) is not ObservableObject viewModel)
                throw new Exception("View model has not been resolved");

            var view = viewProvider.GetView(viewModelType);
            view.DataContext = viewModel;

            var window = view as Window;
            window ??= new Window()
            {
                MinWidth = 400,
                MinHeight = 200,
                Content = view,
            };

            CancelEventHandler closeChildWindowHandler = (object? sender, CancelEventArgs e) => window.Close();

            window.Closed += (s, e) => OnWindowClosed(s, e, closeChildWindowHandler);
            mainWindow.Closing += closeChildWindowHandler;
            activeWindows.Add(viewModelType, window);

            if (viewModel is IRequestClose requestClose)
            {
                requestClose.CloseHandler += closeChildWindowHandler;
            }

            window.Show();

            if (viewModel is IWindowActivationHandler win)
            {
                win.OnWindowActivated(windowParams);
            }
            window.Activate();
        }

        private void OnWindowClosed(object? sender, EventArgs e, CancelEventHandler closeChildWindowHandler)
        {
            var window = sender as Window;
            if (window == null)
                return;

            if (window.Content is FrameworkElement frameworkElement)
            {
                activeWindows.Remove(frameworkElement.DataContext.GetType());
                if (frameworkElement.DataContext is IRequestClose requestClose)
                {
                    requestClose.OnClose();
                    requestClose.CloseHandler -= closeChildWindowHandler;
                }
            }

            mainWindow.Closing -= closeChildWindowHandler;
        }
    }
}