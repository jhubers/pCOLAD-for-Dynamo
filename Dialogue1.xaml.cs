using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
