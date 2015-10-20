﻿using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for CSVControl.xaml
    /// </summary>
    public partial class CSVControl : Window
    {
        //bool firsttime = true;
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
                //case "Accepted":
                //e.Column.CanUserSort = false;
                //e.Column.Width = 20;
                //e.Column.Visibility = Visibility.Collapsed;
                //break;
                //case "Parameter":
                //    //e.Column.CanUserSort = false;
                //    //e.Column.Width = 20;
                //    //e.Column.Visibility = Visibility.Visible;
                //    break;
                case "Comments":
                    // Create a new template column.
                    DataGridTemplateColumn commentsTemplateColumn = new DataGridTemplateColumn();
                    commentsTemplateColumn.Header = "Comments";
                    commentsTemplateColumn.CellTemplate = (DataTemplate)Resources["changedComments"];
                    // Replace the auto-generated column with the templateColumn.
                    e.Column = commentsTemplateColumn;
                    //e.Column.Width = 100;
                    break;
                case "New Value":
                    // Create a new template column.
                    DataGridTemplateColumn newValueTemplateColumn = new DataGridTemplateColumn();
                    newValueTemplateColumn.Header = "New Value";
                    newValueTemplateColumn.CellTemplate = (DataTemplate)Resources["changedNewValue"];
                    // Replace the auto-generated column with the templateColumn.
                    e.Column = newValueTemplateColumn;
                    //e.Column.Width = 100;
                    break;
                case "Importance":
                    // Create a new template column.
                    DataGridTemplateColumn importanceTemplateColumn = new DataGridTemplateColumn();
                    importanceTemplateColumn.Header = "Importance";
                    importanceTemplateColumn.CellTemplate = (DataTemplate)Resources["changedImportance"];
                    // Replace the auto-generated column with the templateColumn.
                    e.Column = importanceTemplateColumn;
                    //e.Column.Width = 100;
                    break;
                //case "Importance":
                //    // Create a new template column.
                //    DataGridTemplateColumn importanceTemplateColumn = new DataGridTemplateColumn();
                //    importanceTemplateColumn.Header = "Importance";
                //    importanceTemplateColumn.CellTemplate = (DataTemplate)Resources["changedImportance"];
                //    // Replace the auto-generated column with the templateColumn.
                //    e.Column = importanceTemplateColumn;
                //    //e.Column.Width = 100;
                //    break;
                default:
                    //e.Column.CanUserSort = false;
                    e.Column.Width = 100;
                    //e.Column.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void myCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            //set the checkbox to the value in the hidden column "Accepted"
            //this should only happen the first time you add pSHARE
            //or you should update column "Accepted" 
            //if (firsttime)
            //{

            CheckBox cb = (CheckBox)sender;
            //the last row is not a valid DataRowView
            if (cb.DataContext.GetType() == typeof(DataRowView))
            {
                DataRowView drv = (DataRowView)cb.DataContext;
                DataRow dr = drv.Row;
                string obstruction = dr["Obstruction"].ToString();
                bool b;
                //bool? b = (bool?)dr["Accepted"];
                //check if field in column "Obstruction" is empty
                if (obstruction == "")
                {
                    b = true;
                }
                else
                {
                    b = false;
                }
                cb.IsChecked = b;
            }
            //}
        }



    }
}
