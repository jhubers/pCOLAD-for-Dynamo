using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace pCOLADnamespace
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass //: NodeModel //INotifyPropertyChanged
    {
        // later replace with an input
        public static string inputFile = "D:\\Temp\\test2.csv";
        public static string userName = "Hans";
        public static DataTable myDataTable;
        public static DataTable myPropDataTable
        {
            get { return myDataTable; }
            set
            { myDataTable = value; }
        }
        ////public static int _rowIndex;
        ////public static int RowIndex
        ////{
        ////    get { return _rowIndex; }
        ////    set
        ////    {
        ////        _rowIndex = value;
        ////        RaisePropertyChanged("RowIndex");
        ////        // MessageBox.Show(string.Format("Row: {0}", _rowIndex.ToString()));
        ////    }
        ////}
        ////public static DataGridCellInfo _cellInfo;
        /////// <summary>
        /////// property of pSHARE about which cell is selected
        /////// </summary>
        ////public static DataGridCellInfo CellInfo
        ////{
        ////    get { return _cellInfo; }
        ////    set
        ////    {
        ////        _cellInfo = value;
        ////        RaisePropertyChanged("CellInfo");
        ////    }
        ////}
        ////public static bool _isChecked;
        /////// <summary>
        /////// property of pSHARE telling if a row is checked and so a value obstructed
        /////// </summary>
        ////public static bool isChecked
        ////{
        ////    get { return _isChecked; }
        ////    set
        ////    {
        ////        _isChecked = value;
        ////        //OnPropertyChanged("isChecked"); //this sets all checkboxes to checked...
        ////        DataRow dr = myDataTable.Rows[_rowIndex];
        ////        //also change the value in the hidden column "Accepted"
        ////        dr["Accepted"] = value;
        ////        string cellContent = dr["Obstruction"].ToString();
        ////        if (_cellInfo != null && !_isChecked) //add the userName
        ////        {
        ////            if (cellContent == "")
        ////            {
        ////                dr["Obstruction"] = userName;
        ////            }
        ////            else
        ////            {
        ////                dr["Obstruction"] += "," + userName;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            // remove username from the cell
        ////            cellContent = cellContent.Replace(userName, "");
        ////            //remove double and end commas
        ////            cellContent = Regex.Replace(cellContent, ",{2,}", ",").Trim(',');
        ////            dr["Obstruction"] = cellContent.Trim();
        ////        }
        ////    }
        ////}
        public static void openCSV()
        {
            myDataTable = new DataTable();
            List<string> csvList = new List<string>();
            //first make a List<string> out of the csv-file (because pCOLLECTs are also turned into List<string>
            //then turn List<string> into a DataTable with Functions.ListToTable
            try
            {
                StreamReader myStream = new StreamReader(inputFile);
                string line = "";
                int i = 0;
                while (line != null)
                {
                    line = myStream.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    csvList.Add(line);
                    i += 1;
                }
                myDataTable = Functions.ListToTable(csvList);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("We couldn't find the file. Are you sure it exists?");
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("We couldn't find the file. Are you sure the directory exists?");
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("We found a problem: {0}", e));//instance not set to a etc.
            }
        }

        //public static void ShowCSV()
        //{
        //    //make sure that control doesn't exist.
        //    try
        //    {
        //        //check if the control exist already
        //        bool isCSVControlOpen = false;
        //        foreach (Window w in Application.Current.Windows)
        //        {
        //            if (w is CSVControl)
        //            {
        //                isCSVControlOpen = true;
        //                w.Activate();
        //            }
        //        }
        //        if (!isCSVControlOpen)
        //        {
        //            myPropDataTable = myDataTable;
        //            CSVControl _CSVControl = new CSVControl();
        //            //bind the datatable to the xaml datagrid
        //            _CSVControl.myXamlTable.ItemsSource = myPropDataTable.DefaultView;
        //            //_CSVControl.DataContext = this;
        //            _CSVControl.Show();
        //        }
        //    }

        //    catch (System.Exception e)
        //    {
        //        MessageBox.Show("Exception source: {0}", e.Source);
        //    }
        //}
        //[IsVisibleInDynamoLibrary(true)]
        public static List<List<string>> output;
        public static List<List<string>> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            //The inputs of the pCOLLECTs must be added to the content of the csv file.
            //So first load the csv file!!!!!!!!!!!!!here



            //Make an output of pSHARE consisting of a list of alternating parameter names and new values.
            //Unless a value is obstructed. Then add "Obstructed" as new value.


            List<List<string>> pSHAREoutputs = new List<List<string>>();
            foreach (List<string> item in _Ninputs)
            {
                pSHAREoutputs.Add(item);
            }
            output = pSHAREoutputs;
            return pSHAREoutputs;
        }
    }
}

