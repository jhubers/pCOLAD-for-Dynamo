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
using System.Windows.Threading;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass
    {

        // later replace with an input
        public static string inputFile; // = "D:\\Temp\\test2.csv";        
        public static FileSystemWatcher CSVwatcher;
        public static FileSystemWatcher ImagesWatcher;
        public static string ShareInputFile;//to store the inputFile path so you can change it for the History file path
        public static string inputFileCopy;
        public static string userName;// = "Hans";
        public static bool formPopulate = false;
        public static List<string> csvList = new List<string>();
        public static List<string> copyCsvList = new List<string>();
        public static List<List<string>> pSHAREoutputs = new List<List<string>>();
        public static DataTable myDataTable;
        public static DataTable csvDataTable;
        public static DataTable copyDataTable;
        public static event EventHandler UpdateCSVControl = delegate { };
        private static DataTable mergedDataTable;
        public static bool AutoPlay;
        public static string testValue;

        public static void openCSV()
        {            
            if (!formPopulate)
            {
                myDataTable = null;
                copyDataTable = null;
                csvList.Clear();
                copyCsvList.Clear();
                //first make a List<string> out of the csv-file (because pCOLLECTs are also turned into List<string>
                //then turn List<string> into a DataTable with Functions.ListToTable
                //first make a list and datatable from the copy csv-file
                copyCsvList = Functions.CSVtoList(inputFileCopy);
                csvList = Functions.CSVtoList(inputFile);
                //if you start with an empty csv file...
                if (csvList.Count == 0)
                {
                    csvList.Add("Images;Comments;Parameter;New Value;Obstruction;Old Value;Owner;Importance;Date;Author");
                }
                if (copyCsvList.Count == 0)
                {
                    copyCsvList.Add("Images;Comments;Parameter;New Value;Obstruction;Old Value;Owner;Importance;Date;Author");
                }
                myDataTable = Functions.ListToTable(csvList);
                copyDataTable = Functions.ListToTable(copyCsvList);
                csvDataTable = myDataTable.Copy();
                //UpdateCSVControl(null, EventArgs.Empty);                
            }
            else
            {
                myDataTable = csvDataTable.Copy();
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
               //with Automatic run the first ls has null in line two giving errors!!!
                DataTable newParamTable = MyDataCollector.Functions.ListToTable(ls);
                //since you removed owner from pCOLLECT add it here 
                //maybe was wrong, because owner can change? Then simply disconnect pCOLLECT concerned!
                newParamTable.Columns.Add("Owner", typeof(Item));
                //add a column for the images at the left of the table               
                newParamTable.Columns.Add("Images", typeof(Item)).SetOrdinal(0);
                //check if parameterName is already used. But if it is your own, just update the value
                for (int i = 0; i < newParamTable.Rows.Count; i++)
                {
                    //since you removed owner from pCOLLECT add it here 
                    newParamTable.Rows[i]["Owner"] = new Item(userName);
                    for (int j = i; j < myDataTable.Rows.Count; j++)
                    {
                        string t1 = myDataTable.Rows[j]["Parameter"].ToString();
                        string t2 = newParamTable.Rows[i]["Parameter"].ToString();
                        string p1 = myDataTable.Rows[j]["Owner"].ToString();
                        string p2 = userName;
                        if (t1 == t2 && p1 != p2)
                        {
                            MessageBox.Show("Parameter " + t2 + " already exists. Please use another parameter name...");
                            //replace the Parameter by ERROR
                            newParamTable.Rows[i]["Parameter"] = new Item("---ERROR---");
                        }
                    }
                }
                newParamTables.Add(newParamTable);
            }
            if (newParamTables.Count > 2)
            {
                DataTable TblUnion = Functions.MergeAll(newParamTables, "Parameter");
                myDataTable = TblUnion;
                //make a copy of myDataTable so you can return to it if changes are undone
                if (!formPopulate && !inputFile.Contains("History"))
                {
                    mergedDataTable = myDataTable.Copy();
                    formPopulate = true;
                }
            }
            //add a property ImageList to Item with image paths from the folder with the
            //name of the parameter in the Images folder in the DropBox
            //also for the parameters in myDataTable...So do it after the merge.
            for (int i = 0; i < myDataTable.Rows.Count; i++)
            {

                List<MyImage> lmi = new List<MyImage>();
                string inputFolder = inputFile.Remove(inputFile.LastIndexOf("\\") + 1);
                string imageFolderPath = inputFolder + "Images\\" + myDataTable.Rows[i]["Parameter"].ToString();
                var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
                //next function returns a list of strings with only the path names of files
                //with extension in filters
                List<string> files = Functions.GetFilesFrom(imageFolderPath, filters, false);
                List<string> fileNames = new List<string>();
                foreach (string st in files)
                {
                    fileNames.Add(Path.GetFileName(st));
                    //create an new MyImage with ImagePath
                    MyImage it = new MyImage(st);
                    lmi.Add(it);
                }
                //set the ImageList property of the new Item                    
                Item ni = new Item("");
                ni.ImageFileNameList = fileNames;
                ni.ImageList = lmi;
                myDataTable.Rows[i]["Images"] = ni;

            }
        }

        public static string projection(int i)
        {
            return "Value = " + i.ToString();
        }
        public static List<string> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            inputFile = _IfilePath;
            watch();
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
            // Check if not only 1 pCOLLECT is connected otherwise you get an error
            if (pSHAREoutputs.Count > 1)
            {
                addNewPararemeters();
            }
            List<string> pSHAREoutputList = new List<string>();
            //now myDataTable contains the union of the csv file and the new parameters
            //so, you can use the columns "Parameter" and "New Value"
            for (int i = 0; i < myDataTable.Rows.Count; i++)
            {
                pSHAREoutputList.Add(myDataTable.Rows[i]["Parameter"].ToString());
                pSHAREoutputList.Add(myDataTable.Rows[i]["New Value"].ToString());
            }
            //when you change a parameter you should have immediate update of the display when you hit run.
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
        private static void watch()
        {
            CSVwatcher = new FileSystemWatcher();
            CSVwatcher.Path = Path.GetDirectoryName(MyDataCollectorClass.inputFile);
            CSVwatcher.NotifyFilter = NotifyFilters.LastWrite;
            CSVwatcher.Filter = Path.GetFileName(MyDataCollectorClass.inputFile);
            CSVwatcher.Changed += new FileSystemEventHandler(OnChanged);
            CSVwatcher.EnableRaisingEvents = true;
            ImagesWatcher = new FileSystemWatcher();
            ImagesWatcher.Path = Path.GetDirectoryName(MyDataCollectorClass.inputFile) + "\\" + "Images" + "\\";
            ImagesWatcher.NotifyFilter = NotifyFilters.LastWrite;
            ImagesWatcher.Filter = "*.*";//can not filter several types 
            ImagesWatcher.IncludeSubdirectories = true;
            ImagesWatcher.Changed += OnChanged;
            ImagesWatcher.Created += OnChanged;
            ImagesWatcher.Renamed += OnChanged;
            ImagesWatcher.Deleted += OnChanged;
            ImagesWatcher.EnableRaisingEvents = true;
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            //stop watching because otherwise you get nummerous messages
            ImagesWatcher.EnableRaisingEvents = false;
            CSVwatcher.EnableRaisingEvents = false;
            //show the message on top of Dynamo. Because it comes from a different thread
            //you need a dispatcher. Should not work if you save yourself. So disable in Share command.

            string msg = "Some changes occured in the shared information. I will start over... " +
            "Hit the Run button if you are not in Automatic mode.";
            //if (AutoPlay)
            //{
            //    //stop watching because otherwise you get nummerous messages
            //    ImagesWatcher.EnableRaisingEvents = false;
            //    CSVwatcher.EnableRaisingEvents = false;
            //}
            if  (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBox.Show(Application.Current.MainWindow, msg);
                    ImagesWatcher.EnableRaisingEvents = true;
                    CSVwatcher.EnableRaisingEvents = true;
                }));
            }
            //else
            //{
            //    MessageBox.Show(Application.Current.MainWindow, msg);
            //    ImagesWatcher.EnableRaisingEvents = true;
            //    CSVwatcher.EnableRaisingEvents = true;
            //}

            //Update the CSVControll with new csv file.
            formPopulate = false;
            openCSV();
            addNewPararemeters();
            UpdateCSVControl(null, EventArgs.Empty);
        }

        #region OldFunc
        //public static List<string> pCOLLECToutputs(params string[] ss)
        //{
        //    //pCOLLECT should output a list of ;-separated strings in the format:
        //    //Parameter;New Value;Importance;Comments;Owner;Extra Attribute Name;Extra Attribute Name; ...etc
        //    //And on a second line the ;-separated string values of these attributes.
        //    //But how do we get the names of the inputs? Maybe have to create a node in pCOLLECT script and add
        //    //this node to it?
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

