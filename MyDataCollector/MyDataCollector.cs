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

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass
    {
        // later replace with an input
        public static string inputFile = "D:\\Temp\\test2.csv";
        public static string userName = "Hans";
        public static bool formPopulate = false;
        public static List<string> csvList = new List<string>();
        public static List<List<string>> pSHAREoutputs = new List<List<string>>();
        public static DataTable myDataTable;
        public static event EventHandler UpdateCSVControl= delegate {};
        public static void openCSV()
        {
            //openCSV() should run when somebody changed the CSV-file.
            //But you should then always start with an empty myDataTable and csvList
            //and only when the button is on you should get warnings!!!
            //maybe better use a seperate table for the loaded csv file, now when you change parameter name in pCOLLECT
            //you create a new line in the csv file !!!
            if (!formPopulate)
            {
                myDataTable= null;
                csvList.Clear();
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
                    myStream.Dispose();
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
            DataTable TblUnion = Functions.MergeAll(newParamTables, "Parameter");
            myDataTable = TblUnion;
            //check here with copy of csv table to set difference in red!!!

        }
        public static List<string> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            pSHAREoutputs.Clear();
            foreach (List<string> item in _Ninputs)
            {
                pSHAREoutputs.Add(item);
            }
            //The inputs of the pCOLLECTs must be added to the content of the csv file, changing the myDataTable property.
            openCSV();
            addNewPararemeters();
            List<string> pSHAREoutputList = new List<string>();
            //now myDataTable contains the union of the csv file and the new parameters
            //so, you can use the columns "Parameter" and "New Value"
            for (int i = 0; i < myDataTable.Rows.Count ; i++)
            {
                pSHAREoutputList.Add(myDataTable.Rows[i].Field<string>("Parameter"));
                pSHAREoutputList.Add(myDataTable.Rows[i].Field<string>("New Value"));
            }
            //when you change a parameter you should have immediate update of the display!!!
            UpdateCSVControl(null, EventArgs.Empty);
            return pSHAREoutputList;
        }
        public static string pPARAMinputs(string _Parameter, List<string> _pSHAREoutput)
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
    }
}

