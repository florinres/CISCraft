using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Ui.Views.UserControls.Microprogram
{
    public partial class GoToRowDialog : Window
    {
        public int ParsedRow { get; private set; }

        public GoToRowDialog()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                RowTextBox.Focus();
                RowTextBox.SelectAll();
            };

            RowTextBox.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidNumberInput(e.Text);
            };
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (TryParseRow())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool TryParseRow()
        {
            var input = RowTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                ShowError("Please enter a row number.");
                return false;
            }

            if (int.TryParse(input, out int row))
            {
                if (row < 0)
                {
                    ShowError("Row number must be 0 or greater.");
                    return false;
                }

                ParsedRow = row;
                HideError();
                return true;
            }
            else
            {
                ShowError("Invalid row number format.");
                return false;
            }
        }

        private void ShowError(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            StatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private bool IsValidNumberInput(string text)
        {
            return Regex.IsMatch(text, "^[0-9]*$");
        }

        // Override OnPreviewKeyDown to handle Esc key
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }
    }
}