using System.Windows.Input;
using System.Windows;

namespace KtSubs.Wpf.XamlHelpers
{
    public static class HotkeyBehavior
    {
        public static readonly DependencyProperty PreviewKeyDownCommandProperty =
            DependencyProperty.RegisterAttached(
                "PreviewKeyDownCommand",
                typeof(ICommand),
                typeof(HotkeyBehavior),
                new PropertyMetadata(null, OnPreviewKeyDownCommandChanged));

        public static void SetPreviewKeyDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(PreviewKeyDownCommandProperty, value);
        }

        public static ICommand GetPreviewKeyDownCommand(UIElement element)
        {
            return (ICommand)element.GetValue(PreviewKeyDownCommandProperty);
        }

        private static void OnPreviewKeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    element.PreviewKeyDown += OnPreviewKeyDown;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    element.PreviewKeyDown -= OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetPreviewKeyDownCommand(element);
            if (command != null && command.CanExecute(e))
            {
                command.Execute(e);
            }
        }
    }
}
