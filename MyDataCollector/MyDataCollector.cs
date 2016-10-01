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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Dynamo.Controls;
using System.Threading;
using Dynamo.Graph.Nodes.CustomNodes;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass
    {

        public static bool myShare = false; //used to avoid warning if you hit Share button yourself
        public static string sharedFile; //the project csv file in DropBox
        public static string formerSharedFile; //a copy of the shared file path        
        public static FileSystemWatcher CSVwatcher;
        public static FileSystemWatcher ImagesWatcher;
        //public static string csvFile;//can be the sharedFile or the HistoryFile
        public static string localFile;//local copy of sharedFile
        public static string formerLocalFile; //a copy of the localFile 
        public static string oldLocalFile;//before last changed localFile
        public static string userName;// = "Hans";
        public static bool formPopulate = false;
        public static List<string> csvList = new List<string>();
        public static List<string> oldCsvList = new List<string>();
        public static List<List<string>> pSHAREoutputs = new List<List<string>>();
        public static DataTable localDataTable;
        public static DataTable csvDataTable;
        public static DataTable oldDataTable;
        public static event EventHandler UpdateCSVControl = delegate { };
        public static event EventHandler Update_pSHARE = delegate { };
        public static event EventHandler<TextArgs> Message = delegate { };
        //private static DataTable mergedDataTable;
        public static string AutoMaticMode = "";
        public static string testValue;
        public static DateTime lastRead = DateTime.MinValue;
        public static DateTime lastWriteTime;
        public static string msg = "";
        public static DynamoView dv;
        public static DynamoModel dm;
        public static bool firstRun = true;
        public static bool extShare = false;
        private static string ProjectName;

        //public static bool switching = false;

        public static void makeOldDataTable()
        {
            oldDataTable = null;
            oldCsvList.Clear();
            oldCsvList = Functions.CSVtoList(oldLocalFile);
            //if you start with empty csv file ...
            if (oldCsvList.Count == 0)
            {
                oldCsvList.Add("Images;Comments;Parameter;New Value;Obstruction;Old Value;Owner;Importance;Date;Author");
            }
            oldDataTable = Functions.ListToTable(oldCsvList, "not localFile");
        }

        public static void openCSV(string csvs)
        {
            if (!formPopulate)
            {
                //this is used for project csv file and History file
                localDataTable = null;
                csvList.Clear();
                //first make a List<string> out of the csv-file (because pCOLLECTs are also turned into List<string>
                //then turn List<string> into a DataTable with Functions.ListToTable
                csvList = Functions.CSVtoList(csvs);
                //if you start with empty csv files ...
                if (csvList.Count == 0)
                {
                    csvList.Add("Images;Comments;Parameter;New Value;Obstruction;Old Value;Owner;Importance;Date;Author");
                }
                localDataTable = Functions.ListToTable(csvList, csvs);
                csvDataTable = localDataTable.Copy();
                //UpdateCSVControl(null, EventArgs.Empty);                
            }
            else
            {
                //the display is there already, no need to load csv files just use the stored version
                localDataTable = csvDataTable.Copy();
            }
        }
        public static void addNewPararemeters()
        {
            //add the outputs of pSHARE to localDataTable so they can be shown in the display
            //turn list of list of strings into List of DataTables
            List<DataTable> newParamTables = new List<DataTable>();
            foreach (List<string> ls in pSHAREoutputs)
            {
                //with Automatic run the first ls has null in line two, giving errors!!!

                //ls is list with every two rows the headers or attributes of all pCOLLECTs. First check if parameter names are unique if 
                //they are not yours, otherwise you should have had a warning already. If tey are yours just update the values in the merge.
                DataTable newParamTable = MyDataCollector.Functions.ListToTable(ls, "not HistoryFile");
                newParamTable.PrimaryKey = new DataColumn[] { newParamTable.Columns["Parameter"] };
                //localDataTable.PrimaryKey = new DataColumn[] { localDataTable.Columns["Parameter"] };

                //don't overwrite comments
                foreach (DataRow dr in localDataTable.Rows)
                {
                    DataRow newDataRow = newParamTable.Rows.Find(dr["Parameter"]);//check if the parameter exist already. If so copy the comment
                    if (newDataRow != null)
                    {
                        //Comments are not cumulative anymore. So newDataRow["Comments"].ToString() is always "".
                        ////Add the new comment to the existing if it is not there yet.
                        //if (!dr["Comments"].ToString().Contains(newDataRow["Comments"].ToString()))
                        ////if (!newDataRow["Comments"].Equals(dr["Comments"]))
                        //{
                        //    newDataRow.BeginEdit();
                        //    newDataRow["Comments"] = new Item(dr["Comments"].ToString() + "/" + newDataRow["Comments"].ToString());
                        //    newDataRow.EndEdit();
                        //}
                        //else
                        //{
                        newDataRow["Comments"] = dr["Comments"];
                        //}
                        //override the comments in pSHAREoutput with that from localDataTable
                        //newDataRow["Comments"] = dr["Comments"];
                        //there is no Column Obstruction and Old Value in newParamTable
                        //newDataRow["Obstruction"] = dr["Obstruction"];
                        //newDataRow["Old Value"] = dr["Old Value"];
                    }
                }
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
                    for (int j = i; j < localDataTable.Rows.Count; j++)
                    {
                        string t1 = localDataTable.Rows[j]["Parameter"].ToString();
                        string t2 = newParamTable.Rows[i]["Parameter"].ToString();
                        string p1 = localDataTable.Rows[j]["Owner"].ToString();
                        string p2 = userName;
                        if (t1 == t2 && p1 != p2)
                        {
                            dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                MessageBox.Show(dv, "Parameter " + t2 + " already exists. Please use another parameter name...");
                            }));
                            //replace the Parameter by ERROR
                            newParamTable.Rows[i]["Parameter"] = new Item("---ERROR---");
                        }
                    }
                }
                newParamTables.Add(newParamTable);
            }
            DataTable mergedNewParamTables = Functions.MergeAll(newParamTables, "Parameter");
            List<DataTable> allParamTables = new List<DataTable>();
            allParamTables.Add(localDataTable);
            allParamTables.Add(mergedNewParamTables);
            //merging tables needs more than 1 table of course. But if you connect only 1 pCOLLECT
            //you have at this point only 1 table. With one table you get the warning to put a List.Create
            //between pCOLLECT and pSHARE. But if you ignore that message, and hit the pSHARE button,
            //you get the warning to first hit the Run button and this keeps showing up.
            if (allParamTables.Count > 1)
            {
                DataTable TblUnion = Functions.MergeAll(allParamTables, "Parameter");
                localDataTable = TblUnion;
                //make a copy of localDataTable so you can return to it if changes are undone
                //if (!formPopulate) // && !sharedFile.Contains("History"))
                //{
                //    mergedDataTable = localDataTable.Copy();
                formPopulate = true;
                //}
            }
            //add a property ImageList to Item with image paths from the folder with the
            //name of the parameter in the Images folder in the DropBox
            //also for the parameters in localDataTable...So do it after the merge.
            for (int i = 0; i < localDataTable.Rows.Count; i++)
            {

                List<MyImage> lmi = new List<MyImage>();
                string inputFolder = sharedFile.Remove(sharedFile.LastIndexOf("\\") + 1);
                string imageFolderPath = inputFolder + "Images\\" + localDataTable.Rows[i]["Parameter"].ToString();
                var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
                //next function returns a list of strings with only the path names of files
                //with extension in filters
                List<string> files = Functions.GetFilesFrom(imageFolderPath, filters, false);
                if (files.Count == 0)
                {
                    //put a pCOLADdummy.bmp button like image in the ImageList
                    MyImage dummy = Functions.dummyFunction();
                    lmi.Add(dummy);
                }
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
                localDataTable.Rows[i]["Images"] = ni;
            }
            //add rows to oldDataTable with empty Comments cell so you can set the IsMyChanged property in code behind
            //while (localDataTable.Rows.Count > oldDataTable.Rows.Count)
            //{
            //    DataRow nR = oldDataTable.NewRow();
            //    nR["Comments"] = new Item("");
            //    oldDataTable.Rows.Add(nR);
            //}
        }

        public static string projection(int i)
        {
            return "Value = " + i.ToString();
        }
        //public static List<string> pCOLLECTemptyInput(Function _headers, string _C, string _P, string _Vo, string _I)
        //{
        //    //string _V = Functions.ConvertToString(_Vo);
        //    //List<string> m = new List<string>();
        //    //m.Add(_headers);
        //    //m.Add(_C + ";" + _P + ";" + _V + ";" + _I);
        //    //return m;
        //}
        public static List<string> pCOLLECTinputs4(string _headers, string _C, string _P, object _Vo, string _I)
        {
            string _V = Functions.ConvertToString(_Vo);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I);
            return m;
        }
        public static List<string> pCOLLECTinputs5(string _headers, string _C, string _P, object _Vo, string _I, object _Eo1)
        {
            string _V = Functions.ConvertToString(_Vo);
            string _E1 = Functions.ConvertToString(_Eo1);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I + ";" + _E1);
            return m;
        }
        public static List<string> pCOLLECTinputs6(string _headers, string _C, string _P, object _Vo, string _I, object _Eo1, object _Eo2)
        {
            string _V = Functions.ConvertToString(_Vo);
            string _E1 = Functions.ConvertToString(_Eo1);
            string _E2 = Functions.ConvertToString(_Eo2);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I + ";" + _E1 + ";" + _E2);
            return m;
        }
        public static List<string> pCOLLECTinputs7(string _headers, string _C, string _P, object _Vo, string _I, object _Eo1, object _Eo2, object _Eo3)
        {
            string _V = Functions.ConvertToString(_Vo);
            string _E1 = Functions.ConvertToString(_Eo1);
            string _E2 = Functions.ConvertToString(_Eo2);
            string _E3 = Functions.ConvertToString(_Eo3);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I + ";" + _E1 + ";" + _E2 + ";" + _E3);
            return m;
        }
        public static List<string> pCOLLECTinputs8(string _headers, string _C, string _P, object _Vo, string _I, object _Eo1, object _Eo2, object _Eo3, object _Eo4)
        {
            string _V = Functions.ConvertToString(_Vo);
            string _E1 = Functions.ConvertToString(_Eo1);
            string _E2 = Functions.ConvertToString(_Eo2);
            string _E3 = Functions.ConvertToString(_Eo3);
            string _E4 = Functions.ConvertToString(_Eo4);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I + ";" + _E1 + ";" + _E2 + ";" + _E3 + ";" + _E4);
            return m;
        }
        public static List<string> pCOLLECTinputs9(string _headers, string _C, string _P, object _Vo, string _I, object _Eo1, object _Eo2, object _Eo3, object _Eo4, object _Eo5)
        {
            string _V = Functions.ConvertToString(_Vo);
            string _E1 = Functions.ConvertToString(_Eo1);
            string _E2 = Functions.ConvertToString(_Eo2);
            string _E3 = Functions.ConvertToString(_Eo3);
            string _E4 = Functions.ConvertToString(_Eo4);
            string _E5 = Functions.ConvertToString(_Eo5);
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_C + ";" + _P + ";" + _V + ";" + _I + ";" + _E1 + ";" + _E2 + ";" + _E3 + ";" + _E4 + ";" + _E5);
            return m;
        }
        //public static List<string> pSHAREemptyInput(object f, string s0, string s1, string s2, string s3)
        //{
        //    List<string> m = new List<string>();
        //    Message(null, new TextArgs("Please do not connect empty lists..."));
        //    m.Add("Please make sure the file paths end with '.csv'");
        //    return m;
        //}
        public static List<string> pSHAREinputs(List<List<object>> _NinputsO, string _ProjName, string _IdirPath, string _LdirPath, string _owner)
        {
            //this runs every time you hit Run in Dynamo.
            ProjectName = _ProjName;
            List<string> m = new List<string>();
            //construct the file paths
            //check if necessary imputs are given
            if (_ProjName == null || _IdirPath == null || _LdirPath == null || _NinputsO == null || _owner == null)
            {
                m.Add("Some inputs are missing...");
                return m;
            }
            //check if object is list<list<string>>
            //Type inputType = _NinputsO.GetType();
            //Type compareType = Type.GetType("System.Collections.Generic.List`1[System.Collections.Generic.List`1[System.String]]");
            ////Type compareType = Type.GetType("SystemProtoCore.DSASM.StackValue");
            //if (!inputType.Equals(compareType))
            //{
            //    string msg = "Please only connect lists of strings in right format...";
            //    Message(null, new TextArgs(msg));
            //    m.Add(msg);
            //    return m;
            //}
            List<List<string>> _Ninputs = new List<List<string>>();
            foreach (var listObjects in _NinputsO)
            {
                //_NinputsO consists of lists that consist of a list that consist of a first row with headers and a second row of Items.ToString()
                List<string> listStrings = new List<string>();
                for (int i = 0; i < listObjects.Count; i++)
                {
                    try
                    {
                        string checkTypeString = listObjects[i].GetType().ToString();
                        string msg = "Please only connect lists of strings in right format...";
                        if (listObjects[i].GetType() != typeof(string))
                        {
                            m.Add(msg);
                            return m;
                        }
                        listStrings.Add((string)listObjects[i]);
                    }
                    catch (Exception)
                    {
                        Message(null, new TextArgs(msg));
                        m.Add(msg);
                        return m;
                        throw;
                    }
                }
                _Ninputs.Add(listStrings);
            }
            string _IfilePath = _IdirPath + "\\" + _ProjName + ".csv";
            string _LfilePath = _LdirPath + "\\" + _owner + "_" + _ProjName + ".csv";
            //check if the paths are *.csv files
            if (!_IfilePath.EndsWith(".csv") || !_LfilePath.EndsWith(".csv"))
            {
                Message(null, new TextArgs("Please make sure the file paths end with '.csv'"));
                m.Add("Please make sure the file paths end with '.csv'");
                return m;
            }
            //check if owner input is connected
            //if (_owner == null)
            //{
            //    Message(null, new TextArgs("Please connect the owner name to pSHARE..."));
            //    m.Add("Please connect the owner name to pSHARE...");
            //    return m;
            //}
            //store these so if you change solutions and you use different paths you can start over
            formerSharedFile = sharedFile;
            formerLocalFile = localFile;

            sharedFile = _IfilePath;
            localFile = _LfilePath;

            if (!object.Equals(formerSharedFile, sharedFile) || !object.Equals(formerLocalFile, localFile))
            {
                //you switched solution has changed. Start over.
                //switching = true;
                firstRun = true;
                formPopulate = false;
                //UpdateCSVControl(null, EventArgs.Empty);
            }
            //else
            //{
            //    switching = false;
            //}
            //contruct the path for the oldLocalFile
            string dir = Path.GetDirectoryName(_LfilePath);
            string fileName = Path.GetFileName(_LfilePath);
            oldLocalFile = dir + "\\old_" + fileName;

            //check if sharedFile exist; otherwise make one with default headers.
            if (!_IfilePath.Equals("")) { Functions.FileExist(_IfilePath); }
            if (!_LfilePath.Equals("")) { Functions.FileExist(_LfilePath); }
            // the first time you run make a local copy of the shared csv file in DropBox;
            // and a copy from the local file to old_local file for comparing changes
            //in Grasshopper you copy the file from the DropBox every time...but no need, because OnChange.
            if (firstRun)
            {
                //no need to check if localFile exists. In first run always overwrite with DropBox file
                //But what if the user changes the paths?!!! So check anyway.
                if (!File.Exists(localFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localFile));
                }
                File.Copy(_IfilePath, _LfilePath, true);
                //copy local file to oldLocalFile if the latter don't exist
                if (!File.Exists(oldLocalFile))
                {
                    File.Copy(sharedFile, oldLocalFile, true);
                }
                // If History is On and you hit Run, you display the project csv file
                // but the History button stays... So maybe also put next line in firstRun if statement?!!!
                //csvFile = _IfilePath;
                //create filesystemwatcher also only once
                watch();
                openCSV(sharedFile);
                DataTable outputTable = localDataTable.Copy();
                makeOldDataTable();
            }
            userName = _owner;
            pSHAREoutputs.Clear();
            bool error = false;
            try
            {
                foreach (List<string> ls in _Ninputs)
                {
                    //If you connect only 1 pCOLLECT directly to pSHARE you get an error while merging datatables
                    //Strange enough _Niputs then is not a List<List<string>>, but a List<string>. And
                    //this doesn't give an error... But item is then the first line of pCOLLECT output (the headers).
                    //So in that case show a warning that you always should put a List.Create node in between.
                    if (ls.Count == 0)
                    {
                        //user connected an empty list
                        msg = "Did you connect an empty list...?";
                        m.Add(msg);
                        return m;
                        //error = true;
                    }
                    if (ls.Count == 1)
                    {
                        msg = "Please put a List.Create node between pCOLLECT and pSHARE...";
                        m.Add(msg);
                        return m;
                        //error = true;
                    }
                    else
                    {
                        pSHAREoutputs.Add(ls);
                    }
                }
            }
            catch (Exception ex)
            {
                //dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //{
                //    MessageBox.Show(dv, msg);
                //}));
                MessageBox.Show("Last error: " + ex.Message);
                throw;
            }
            //if (error)
            //{
            //    dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //    {
            //        MessageBox.Show(dv, msg);
            //    }));
            //}
            //The inputs of the pCOLLECTs must be added to the display of the csv file, changing the localDataTable property.
            //Populate localDataTable with the csv file
            if (extShare)
            {
                openCSV(sharedFile);
                //makeOldDataTable();//makes no sense it was not changed by external share
            }
            // Union the pCOLLECTs to localDataTable
            // Check if not only 1 pCOLLECT is connected otherwise you get an error
            List<string> pSHAREoutputList = new List<string>();
            if (pSHAREoutputs.Count > 0)
            {
                addNewPararemeters();
                //now localDataTable contains the union of the csv file and the new parameters
                //so, you can use the columns "Parameter" and "New Value"
                //But if you use a parameter from pSHARE's output for a pCOLLECT
                //you get a cyclic depency error.
                for (int i = 0; i < localDataTable.Rows.Count; i++)
                {
                    if (localDataTable.Rows[i]["Obstruction"].ToString() != "")
                    {
                        pSHAREoutputList.Add(localDataTable.Rows[i]["Parameter"].ToString());
                        pSHAREoutputList.Add("Obstructed...");
                    }
                    else
                    {
                        pSHAREoutputList.Add(localDataTable.Rows[i]["Parameter"].ToString());
                        pSHAREoutputList.Add(localDataTable.Rows[i]["New Value"].ToString());
                    }
                }
            }
            else
            {
                pSHAREoutputList.Add("WARNING: output is not generated. Did you connect the input without a List.Create? Or with empty list?");
            }
            //when you change a parameter you should have immediate update of the display when you hit run.
            UpdateCSVControl(null, EventArgs.Empty);//includes Compare()
            //if (extShare)
            //{
            //    //update the comments in oldDataTable and old file so if you change them back they become green
            //    for (int i = 0; i < oldDataTable.Rows.Count; i++)
            //    {
            //        oldDataTable.Rows[i]["Comments"] = localDataTable.Rows[i]["Comments"];
            //    }
            //}
            //firstRun = false;//set it after showing CSV display for the first time, so you can check Comments
            return pSHAREoutputList;
        }
        public static string pPARAMoutputs(string _Parameter, List<string> _pSHAREoutput)
        {
            string pPARAMoutput = "";
            if (_pSHAREoutput == null)
            {
                //Message(null, new TextArgs("Please do not connect empty lists to pSHARE..."));
                pPARAMoutput = "No valid pSHARE output connected...";
                return pPARAMoutput;
            }
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
            CSVwatcher.Path = Path.GetDirectoryName(MyDataCollectorClass.sharedFile);
            CSVwatcher.NotifyFilter = NotifyFilters.LastWrite;
            CSVwatcher.Filter = Path.GetFileName(MyDataCollectorClass.sharedFile);
            CSVwatcher.Changed += new FileSystemEventHandler(OnChanged);
            CSVwatcher.EnableRaisingEvents = true;
            ImagesWatcher = new FileSystemWatcher();
            if (!Directory.Exists(Path.GetDirectoryName(MyDataCollectorClass.sharedFile) + "\\" + "Images" + "\\"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MyDataCollectorClass.sharedFile) + "\\" + "Images" + "\\");
            }
            ImagesWatcher.Path = Path.GetDirectoryName(MyDataCollectorClass.sharedFile) + "\\" + "Images" + "\\";
            ImagesWatcher.NotifyFilter = NotifyFilters.LastWrite;
            ImagesWatcher.Filter = "*.*";//can not filter several types 
            ImagesWatcher.IncludeSubdirectories = true;
            ImagesWatcher.Changed += OnChanged;
            ImagesWatcher.Created += OnChanged;
            ImagesWatcher.Renamed += OnChanged;
            ImagesWatcher.Deleted += OnChanged;
            ImagesWatcher.EnableRaisingEvents = true;
        }
        //[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        //public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            //stop watching because otherwise you get nummerous messages (that doesn't work)
            if (myShare)
            {
                return;
            }
            ImagesWatcher.EnableRaisingEvents = false;
            CSVwatcher.EnableRaisingEvents = false;
            //show the message on top of Dynamo. Because it comes from a different thread
            //you need a dispatcher. Should not work if you save yourself. So disable in Share command.
            lastWriteTime = File.GetLastWriteTime(sharedFile);
            //DateTime timeSpan = lastRead.AddSeconds(1);
            string msg = "Some changes occured in " + ProjectName + " project. I will start over... ";
            //if (AutoMaticMode)
            //{
            //    //stop watching because otherwise you get nummerous messages
            //    ImagesWatcher.EnableRaisingEvents = false;
            //    CSVwatcher.EnableRaisingEvents = false;
            //}
            if (lastWriteTime != lastRead)
            {
                lastRead = lastWriteTime;
                //if (Application.Current != null) //in Revit Application.Current is null!
                //{
                //if (!Application.Current.Dispatcher.CheckAccess())
                //don't know how to get the right dispatcher, so invoke always...
                //Dispatcher dp = Dispatcher.CurrentDispatcher;
                //string sdp = dp.Thread.Name + "  " + dp.Thread.GetType().ToString();
                //if (!Dispatcher.CurrentDispatcher.CheckAccess())
                //{
                //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                if (dv == null)
                {
                    MessageBox.Show(msg);
                    File.Copy(sharedFile, localFile, true);
                    formPopulate = false;
                    //since you have to hit Run next three lines will run there
                    openCSV(sharedFile);
                    addNewPararemeters();
                    makeOldDataTable();
                    //but hide the _CSVControl anyway to make clear something changed
                    //next line also runs when you hit Run, but is a way to hide the _CSVControl
                    UpdateCSVControl(null, EventArgs.Empty);
                    Update_pSHARE(null, EventArgs.Empty);
                    ImagesWatcher.EnableRaisingEvents = true;
                    CSVwatcher.EnableRaisingEvents = true;
                    extShare = true;                    
                }
                else
                {
                    dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                     {
                         //MessageBox.Show(Application.Current.MainWindow, msg);
                         MessageBox.Show(dv, msg);
                         //Change the background colour to red

                         //copy the sharedFile to the localFile
                         File.Copy(sharedFile, localFile, true);
                         formPopulate = false;
                         //since you have to hit Run next three lines will run there
                         openCSV(sharedFile);
                         addNewPararemeters();
                         makeOldDataTable();
                         //but hide the _CSVControl anyway to make clear something changed
                         //next line also runs when you hit Run, but is a way to hide the _CSVControl
                         UpdateCSVControl(null, EventArgs.Empty);
                         Update_pSHARE(null, EventArgs.Empty);
                         ImagesWatcher.EnableRaisingEvents = true;
                         CSVwatcher.EnableRaisingEvents = true;
                         extShare = true;
                         //but pSHARE doesn't update

                         //if you are not in Automatic mode
                         //if (!AutoMaticMode)
                         //{
                         //dm.ForceRun();
                         //openCSV(sharedFile);
                         //}
                     }));
                }
                //    }
                //else
                //{
                //    //Process[] anotherApps = Process.GetProcessesByName("Revit");
                //    //if (anotherApps.Length > 0)
                //    //{
                //    //    if (anotherApps[0] != null)
                //    //    {
                //    //        //List<IntPtr> allChildWindows = new WindowHandleInfo(anotherApps[0].MainWindowHandle).GetAllChildHandles();
                //    //        //foreach (var cw in allChildWindows)
                //    //        //{
                //    //        //    childWindowNames.Add(cw.ToString());
                //    //        //}
                //    //        anotherApps[0].Refresh();
                //    //        var hwnd = anotherApps[0].MainWindowHandle;
                //    //        var window = HwndSource.FromHwnd((IntPtr)hwnd);
                //    //        dynamic customWindow = window.RootVisual;
                //    ////        you get errors not calling from the right thread
                //    //        MessageBox.Show(customWindow,"OwnerWindow name = " + customWindow.Name);

                //    //    }
                //    //}
                //    //Window w 
                //    //in order to show the message above Dynamo you will have to get the window of Dynamo
                //    //or if you manage to display the CSVcontrol (which is a window) above Dynamo
                //    //make a message event and subscribe pSHARE and other objects to it so you can 
                //    //put _CSVcontrol as windowOwner!!!

                //    MessageBox.Show(dv,msg);
                //    //MessageBox.Show(Application.Current.MainWindow, msg);
                //    ImagesWatcher.EnableRaisingEvents = true;
                //    CSVwatcher.EnableRaisingEvents = true;
                //}
                //}
            }
            else
            {
                ImagesWatcher.EnableRaisingEvents = true;
                CSVwatcher.EnableRaisingEvents = true;
                extShare = false;
            }
            // close the _CSVControl

            //Update the CSVControll with new csv file.
            //formPopulate = false;
            //openCSV();
            //addNewPararemeters();
            ////but hide the _CSVControl anyway to make clear something changed
            //formPopulate = false;
            //UpdateCSVControl(null, EventArgs.Empty);
            //ImagesWatcher.EnableRaisingEvents = true;
            //CSVwatcher.EnableRaisingEvents = true;
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
        //public static string pCOLLECToutputs(params string[] ss)
        //{
        //    //pCOLLECT should output a list of ;-separated strings in the format:
        //    //Parameter;New Value;Importance;Comments;Owner;Extra Attribute Name;Extra Attribute Name; ...etc
        //    string pCOLLECToutput = "";
        //    foreach (string s in ss)
        //    {
        //        pCOLLECToutput += s;
        //        pCOLLECToutput += ";";
        //    }
        //    return pCOLLECToutput;
        //}

        #endregion
    }
    //public class WindowHandleInfo
    //{
    //    private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

    //    [DllImport("user32")]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

    //    private IntPtr _MainHandle;

    //    public WindowHandleInfo(IntPtr handle)
    //    {
    //        this._MainHandle = handle;
    //    }

    //    public List<IntPtr> GetAllChildHandles()
    //    {
    //        List<IntPtr> childHandles = new List<IntPtr>();

    //        GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
    //        IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

    //        try
    //        {
    //            EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
    //            EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
    //        }
    //        finally
    //        {
    //            gcChildhandlesList.Free();
    //        }

    //        return childHandles;
    //    }

    //    private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
    //    {
    //        GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

    //        if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
    //        {
    //            return false;
    //        }

    //        List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
    //        childHandles.Add(hWnd);

    //        return true;
    //    }
    //}
    public class TextArgs : EventArgs
    {
        #region Fields
        private string szMessage;
        #endregion Fields

        #region Constructors
        public TextArgs(string TextMessage)
        {
            szMessage = TextMessage;
        }
        #endregion Constructors

        #region Properties
        public string Message
        {
            get { return szMessage; }
            set { szMessage = value; }
        }
        #endregion Properties
    }
}

