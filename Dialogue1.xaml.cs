using System.Windows;

namespace pCOLADnamespace
{
    /// <summary>
    /// Interaction logic for Dialogue1.xaml
    /// </summary>
    public partial class Dialogue1 : Window
    {
        public Dialogue1()
        {
            InitializeComponent();
        }
        public bool Canceled { get; set; }
        private void OK1_Click(object sender, RoutedEventArgs e)
        {
            Canceled = false;
            Close();
        }

        private void Cancel1_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }
    }
}
