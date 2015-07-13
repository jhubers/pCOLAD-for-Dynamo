using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace pCOLADnamespace
{
    /// <summary>
    /// Interaction logic for CSVControl.xaml
    /// </summary>
    public partial class CSVControl : Window
    {
        bool firsttime = true;
        /// <summary>
        /// initialize the CSV control
        /// </summary>
        public CSVControl()
        {
            InitializeComponent();
        }
        private void myXamlTable_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Set properties on the columns during auto-generation
            switch (e.Column.Header.ToString())
            {
                case "Accepted":
                    //e.Column.CanUserSort = false;
                    //e.Column.Width = 20;
                    e.Column.Visibility = Visibility.Collapsed;
                    break;
                case "Parameter":
                    //e.Column.CanUserSort = false;
                    //e.Column.Width = 20;
                    //e.Column.Visibility = Visibility.Visible;
                    break;
                default:
                    //e.Column.CanUserSort = false;
                    e.Column.Width = 100;
                    //e.Column.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void y_Loaded(object sender, RoutedEventArgs e)
        {
            //set the checkbox to the value in the hidden column "Accepted"
            //this should only happen the first time you add pSHARE
            //or you should update column "Accepted" 
            if (firsttime)
            {

                CheckBox cb = (CheckBox)sender;
                //the last row is not a valid DataRowView
                if (cb.DataContext.GetType() == typeof(DataRowView))
                {
                    DataRowView drv = (DataRowView)cb.DataContext;
                    DataRow dr = drv.Row;
                    bool? b = (bool?)dr["Accepted"];
                    cb.IsChecked = b;                   
                }
            }
        }



    }
}
