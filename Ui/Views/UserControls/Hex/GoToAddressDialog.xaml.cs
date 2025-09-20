using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ui.Views.UserControls.Hex
{
    public partial class GoToAddressDialog : Window
    {
        public string Address { get; set; } = "0x0000";
        
        public long ParsedAddress => ParseAddress(Address);

        public GoToAddressDialog()
        {
            InitializeComponent();
            DataContext = this;
            
            // Focus the address text box when the dialog loads
            Loaded += (s, e) => 
            {
                AddressTextBox.Focus();
                AddressTextBox.SelectAll();
            };
            
            // Add input validation
            AddressTextBox.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidHexadecimalInput(e.Text);
            };
            
            // Allow paste operations but validate the content
            AddressTextBox.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (Clipboard.ContainsText())
                    {
                        string clipboardText = Clipboard.GetText();
                        if (!IsValidHexadecimalInput(clipboardText))
                        {
                            e.Handled = true;
                        }
                    }
                }
            };
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(AddressTextBox))
            {
                MessageBox.Show("Please enter a valid hexadecimal address.", "Invalid Address", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidHexadecimalInput(string text)
        {
            return Regex.IsMatch(text, "^[0-9a-fA-FxX]*$");
        }
        
        private static long ParseAddress(string addressText)
        {
            // Handle 0x prefix
            if (addressText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                addressText = addressText.Substring(2);
            }
            
            // Parse the hexadecimal value
            if (long.TryParse(addressText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }
            
            return 0;
        }
        
        // Override OnPreviewKeyDown to handle keyboard navigation and Esc key
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            
            // Allow tab navigation
            if (e.Key == Key.Tab)
            {
                return;
            }
            
            // Close on Escape key
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }
    }
    
    public class HexadecimalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string addressText)
            {
                return new ValidationResult(false, "Invalid input");
            }
            
            // Empty string is not valid
            if (string.IsNullOrWhiteSpace(addressText))
            {
                return new ValidationResult(false, "Address cannot be empty");
            }
            
            // Handle 0x prefix
            if (addressText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                addressText = addressText.Substring(2);
            }
            
            // Validate hex format
            if (!Regex.IsMatch(addressText, "^[0-9a-fA-F]+$"))
            {
                return new ValidationResult(false, "Not a valid hexadecimal value");
            }
            
            // Try to parse the value
            if (!long.TryParse(addressText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            {
                return new ValidationResult(false, "Cannot parse as hexadecimal value");
            }
            
            return ValidationResult.ValidResult;
        }
    }
}