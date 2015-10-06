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
        public static string inputFile; // = "D:\\Temp\\test2.csv";
        public static string ShareInputFile;
        public static string inputFileCopy;
        public static string userName;// = "Hans";
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
            if (newParamTables.Count>2)
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
                if (item.Count ==1)
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
            if (pSHAREoutputs.Count>1)
            {
            addNewPararemeters();                
            }
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

