using System.Windows;
using System.Windows.Input;

namespace Ui.Views.Dialogs
{
    public partial class MicroprogramSearchDialog : Window
    {
        public string SearchText { get; private set; } = string.Empty;

        public MicroprogramSearchDialog()
        {
            InitializeComponent();
            SearchTextBox.Focus();
            
            // Update the placeholder visibility when the text changes
            SearchTextBox.TextChanged += (s, e) =>
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchTextBox.Text) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            };
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchText = SearchTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                CancelButton_Click(sender, e);
            }
        }
    }
}