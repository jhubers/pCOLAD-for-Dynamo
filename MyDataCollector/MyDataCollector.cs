using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.UI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]

    public static class MyDataCollectorClass
    {

        // later replace with an input
        public static string inputFile; // = "D:\\Temp\\test2.csv";
        public static string ShareInputFile;//to store the inputFile path so you can change it for the History file path
        public static string inputFileCopy;
        public static string userName;// = "Hans";
        public static bool formPopulate = false;
        public static List<string> csvList = new List<string>();
        public static List<string> copyCsvList = new List<string>();
        public static List<List<string>> pSHAREoutputs = new List<List<string>>();
        public static DataTable myDataTable;
        public static DataTable copyTable;
        public static event EventHandler UpdateCSVControl = delegate { };
       


        public static void openCSV()
        {
            //openCSV() should run when somebody changed the CSV-file.
            //compare with the copy of the CSV-file!!!
            if (!formPopulate)
            {
                myDataTable = null;
                copyTable = null;
                csvList.Clear();
                copyCsvList.Clear();
                //first make a List<string> out of the csv-file (because pCOLLECTs are also turned into List<string>
                //then turn List<string> into a DataTable with Functions.ListToTable
                //first make a list and datatable from the copy csv-file
                copyCsvList = Functions.CSVtoList(inputFileCopy);
                csvList = Functions.CSVtoList(inputFile);
                myDataTable = Functions.ListToTable(csvList);
                copyTable = Functions.ListToTable(copyCsvList);
                    //UpdateCSVControl(null, EventArgs.Empty);                
                formPopulate = true;
            }

        }
        public static void addNewPararemeters()
        {
            //add the outputs of pSHARE to myDataTable so they can be shown in the display
            //but before you have to build the common multiple, or use the union of dataTables
            //turn list of list of strings into List of DataTables
            List<DataTable> newParamTables = new List<DataTable>();
            newParamTables.Add(myDataTable);
            foreach (List<string> ls in pSHAREoutputs)
            {
                DataTable newParamTable = MyDataCollector.Functions.ListToTable(ls);
                newParamTables.Add(newParamTable);
            }
            if (newParamTables.Count > 2)
            {
                DataTable TblUnion = Functions.MergeAll(newParamTables, "Parameter");
                myDataTable = TblUnion;
            }
            //check here with copy of csv table to set difference in red!!!

        }
        public static List<string> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            inputFile = _IfilePath;
            ShareInputFile = _IfilePath;
            inputFileCopy = _LfilePath;
            userName = _owner;
            pSHAREoutputs.Clear();
            foreach (List<string> item in _Ninputs)
            {
                //If you connect only 1 pCOLLECT directly to pSHARE you get an error while merging datatables
                //Strange enough _Niputs then is not a List<List<string>>, but a List<string>. And
                //this doesn't give an error... But item is then the first line of pCOLLECT output (the headers).
                //So in that case show a warning that you always should put a List.Create node in between.
                if (item.Count == 1)
                {
                    MessageBox.Show("Please put a List.Create node between pCOLLECT and pSHARE...");
                    return null;
                }
                pSHAREoutputs.Add(item);
            }
            //The inputs of the pCOLLECTs must be added to the content of the csv file, changing the myDataTable property.
            //Populate myDataTable with the csv file
            openCSV();
            // Union the pCOLLECTs to myDataTable
            // Check if not only 1 pCOLLECT is connected otherwise you get an error!!!
            if (pSHAREoutputs.Count > 1)
            {
                addNewPararemeters();
            }
            List<string> pSHAREoutputList = new List<string>();
            //now myDataTable contains the union of the csv file and the new parameters
            //so, you can use the columns "Parameter" and "New Value"
            for (int i = 0; i < myDataTable.Rows.Count; i++)
            {
                pSHAREoutputList.Add(myDataTable.Rows[i].Field<string>("Parameter"));
                pSHAREoutputList.Add(myDataTable.Rows[i].Field<string>("New Value"));
            }
            //when you change a parameter you should have immediate update of the display!!!
            UpdateCSVControl(null, EventArgs.Empty);
            return pSHAREoutputList;
        }
        public static string pPARAMoutputs(string _Parameter, List<string> _pSHAREoutput)
        {
            string pPARAMoutput = "";
            for (int i = 0; i < _pSHAREoutput.Count; i++)
            {
                if (_pSHAREoutput[i] == _Parameter)
                {
                    pPARAMoutput = _pSHAREoutput[i + 1];
                    break;
                }
            }
            return pPARAMoutput;
        }
        #region OldFunc
        //public static List<string> pCOLLECToutputs(params string[] ss)
        //{
        //    //pCOLLECT should output a list of ;-separated strings in the format:
        //    //Parameter;New Value;Importance;Comments;Owner;Extra Attribute Name;Extra Attribute Name; ...etc
        //    //And on a second line the ;-separated string values of these attributes.
        //    //But how do we get the names of the inputs?!!! Maybe have to create a node in pCOLLECT script and add
        //    //this node to it?!!!
        //    List<string> pCOLLECToutputList = new List<string>();
        //    //string pCOLLECTattributes = "";

        //    string pCOLLECToutput = "";
        //    foreach (string s in ss)
        //    {
        //        pCOLLECToutput += s;
        //        pCOLLECToutput += ";";
        //    }
        //    pCOLLECToutputList.Add(pCOLLECToutput);
        //    return pCOLLECToutputList;
        //}
        #endregion
        #region NewFunc
        public static string pCOLLECToutputs(params string[] ss)
        {
            //pCOLLECT should output a list of ;-separated strings in the format:
            //Parameter;New Value;Importance;Comments;Owner;Extra Attribute Name;Extra Attribute Name; ...etc
            string pCOLLECToutput = "";
            foreach (string s in ss)
            {
                pCOLLECToutput += s;
                pCOLLECToutput += ";";
            }
            return pCOLLECToutput;
        }

        #endregion
    }
}

