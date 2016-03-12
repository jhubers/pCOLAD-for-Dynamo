﻿using Dynamo.Controls;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
        //private readonly DynamoViewModel dynamoViewModel;
        //bool firsttime = true;
        /// <summary>
        /// initialize the CSV control
        /// </summary>
        public bool ClosingStarted = false;
        public bool Canceling = false;
        public CSVControl()
        {
            //this.dynamoViewModel = dynamoViewModel;
            InitializeComponent();
            //doesn't work because the instance of DynamoView is not in the visual tree of CSVControl
            //var view = FindUpVisualTree<DynamoView>(this);
            //Owner = view;
            Owner = pSHARE.dv;
        }

        private void myXamlTable_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Set properties on the columns during auto-generation
            switch (e.Column.Header.ToString())
            {
                case "Images":
                    // Create a new template column.
                    DataGridTemplateColumn imageTemplateColumn = new DataGridTemplateColumn();
                    imageTemplateColumn.Header = "Images";
                    imageTemplateColumn.CellTemplate = (DataTemplate)Resources["convertedImagePath"];
                    // Replace the auto-generated column with the templateColumn.
                    e.Column = imageTemplateColumn;
                    //e.Column.Width = 200;
                    break;
                case "Comments":
                    MyDataGridTemplateColumn commentCol = new MyDataGridTemplateColumn();
                    commentCol.ColumnName = e.PropertyName;  // so it knows from which column to get the Item
                    //changedCells are the cells that have different value then in the csv copy file
                    commentCol.CellTemplate = (DataTemplate)FindResource("commentCells");
                    //this style avoids selection of cells except the checkbox colomn because that one is not autogenerated
                    //commentCol.CellStyle = (Style)FindResource("AvoidCellSelection");
                    e.Column = commentCol;
                    e.Column.Header = e.PropertyName;
                    //e.Column.Width = 100;
                    break;

                default:
                    MyDataGridTemplateColumn col = new MyDataGridTemplateColumn();
                    col.ColumnName = e.PropertyName;  // so it knows from which column to get the Item
                    //changedCells are the cells that have different value then in the csv copy file
                    col.CellTemplate = (DataTemplate)FindResource("changedCells");
                    //this style avoids selection of cells except the checkbox colomn because that one is not autogenerated
                    col.CellStyle = (Style)FindResource("AvoidCellSelection");
                    e.Column = col;
                    e.Column.Header = e.PropertyName;
                    //e.Column.Width = 100;
                    break;
            }


            //switch (e.Column.Header.ToString())
            //{
            //    case "Comments":
            //        // Create a new template column.
            //        DataGridTemplateColumn commentsTemplateColumn = new DataGridTemplateColumn();
            //        commentsTemplateColumn.Header = "Comments";
            //        commentsTemplateColumn.CellTemplate = (DataTemplate)Resources["changedComments"];
            //        // Replace the auto-generated column with the templateColumn.
            //        e.Column = commentsTemplateColumn;
            //        e.Column.Width = 100;
            //        break;
            //    case "New Value":
            //        // Create a new template column.
            //        DataGridTemplateColumn newValueTemplateColumn = new DataGridTemplateColumn();
            //        newValueTemplateColumn.Header = "New Value";
            //        newValueTemplateColumn.CellTemplate = (DataTemplate)Resources["changedNewValue"];
            //        // Replace the auto-generated column with the templateColumn.
            //        e.Column = newValueTemplateColumn;
            //        //e.Column.Width = 100;
            //        break;
            //    case "Importance":
            //        // Create a new template column.
            //        DataGridTemplateColumn importanceTemplateColumn = new DataGridTemplateColumn();
            //        importanceTemplateColumn.Header = "Importance";
            //        importanceTemplateColumn.CellTemplate = (DataTemplate)Resources["changedImportance"];
            //        // Replace the auto-generated column with the templateColumn.
            //        e.Column = importanceTemplateColumn;
            //        //e.Column.Width = 100;
            //        break;
            //    default:
            //        //e.Column.CanUserSort = false;
            //        //DataGridTemplateColumn defaultTemplateColumn = new DataGridTemplateColumn();
            //        //defaultTemplateColumn.CellTemplate = (DataTemplate)Resources["changedDefault"];
            //        //e.Column = defaultTemplateColumn;
            //        e.Column.Width = 100;
            //        //e.Column.Visibility = Visibility.Visible;
            //        break;
            //}
        }
        private void myCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            //if you select the checkbox the username should appear in the column Obstruction.
            CheckBox cb = (CheckBox)sender;
            //the last row is not a valid DataRowView
            if (cb.DataContext != null && cb.DataContext.GetType() == typeof(DataRowView))
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
        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var im = (System.Windows.Controls.Image)sender;
            //if (e.ClickCount == 2)
            FullScreenImage myFullScreenImage = new FullScreenImage();
            myFullScreenImage.fullImage.Source = im.Source;
            myFullScreenImage.Show();
        }
        private void imageBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            var im = (System.Windows.Controls.Image)b.Child;
            im.Height = 50;
            b.Height = 52;
        }
        private void imageBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            var b = (Border)sender;
            var im = (System.Windows.Controls.Image)b.Child;
            im.Height = 14;
            b.Height = 16;
            var dgtc = (DataGridTemplateColumn)this.FindName("dgtc");
            //apparently you have to first reset the DataGridTemplateColumn
            dgtc.Width = new DataGridLength();
            var md = Mouse.DirectlyOver;
            if (md == null)
            {
                dgtc.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
                return;
            }
            if (md.GetType() != typeof(Border))
            {
                dgtc.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
            }
            else
            {
                Border mdb = (Border)md;
                string mdn = mdb.Name;
                if (mdn != "imageBorder")
                {
                    dgtc.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
                }
            }
        }
        //make a difference if user hit the red X button top right or the Cancel button
        //With X-button you first go here and then to CancelCommand, So set Closing property to 
        //true and then in CancelCommand don't close again.
        //But check if you come from CancelCommand, then you don't do anything
        private void Window_Closing(object sender, EventArgs e)
        {
            if (!Canceling)
            {
                ClosingStarted = true;
                // run the CancelCommand by simulating click on Cancel button
                ButtonAutomationPeer peer = new ButtonAutomationPeer(Cancel);
                IInvokeProvider invokeProv = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
                invokeProv.Invoke(); 
            }
        }
        //static public void BringToFront(Panel pParent, ContentPresenter pToMove)
        //{
        //    try
        //    {
        //        int currentIndex = Canvas.GetZIndex(pToMove);
        //        int zIndex = 0;
        //        int maxZ = 0;
        //        ContentPresenter child;
        //        for (int i = 0; i < pParent.Children.Count; i++)
        //        {
        //            if (pParent.Children[i] is ContentPresenter &&
        //                pParent.Children[i] != pToMove)
        //            {
        //                child = pParent.Children[i] as ContentPresenter;
        //                zIndex = Canvas.GetZIndex(child);
        //                maxZ = Math.Max(maxZ, zIndex);
        //                if (zIndex > currentIndex)
        //                {
        //                    Canvas.SetZIndex(child, zIndex - 1);
        //                }
        //            }
        //        }
        //        Canvas.SetZIndex(pToMove, maxZ);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        //public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        //{
        //    // get parent item
        //    DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        //    // we’ve reached the end of the tree
        //    if (parentObject == null) return null;

        //    // check if the parent matches the type we’re looking for
        //    T parent = parentObject as T;
        //    if (parent != null)
        //    {
        //        return parent;
        //    }
        //    else
        //    {
        //        // use recursion to proceed with next level
        //        return FindVisualParent<T>(parentObject);
        //    }
        //}
        //private static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        //{
        //    DependencyObject current = initial;

        //    while (current != null && current.GetType() != typeof(T))
        //    {
        //        current = VisualTreeHelper.GetParent(current);
        //    }
        //    return current as T;
        //}
    }
}
