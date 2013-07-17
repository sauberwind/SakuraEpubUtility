using System.Windows;

namespace SakuraEpubLibrary
{
    public partial class EpubCheckResultDialog : Window
    {
        public EpubCheckResultDialog()
        {
            InitializeComponent();
        }
        private void OnOKClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
