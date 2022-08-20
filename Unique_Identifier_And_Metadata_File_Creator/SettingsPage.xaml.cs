using System.Windows.Controls;
using System.Windows.Input;

namespace Unique_Identifier_And_Metadata_File_Creator
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings() => InitializeComponent();

        private void SelectAllText(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null)
            {
                return;
            }

            if (!textBox.IsKeyboardFocusWithin)
            {
                textBox.SelectAll();
                e.Handled = true;
                textBox.Focus();
            }
        }
    }
}
