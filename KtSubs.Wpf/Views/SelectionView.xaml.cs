using System.Windows.Controls;

namespace KtSubs.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SelectionView.xaml
    /// </summary>
    public partial class SelectionView : UserControl
    {
        public SelectionView()
        {
            InitializeComponent();
            SearchBox.Focus();
        }
    }
}