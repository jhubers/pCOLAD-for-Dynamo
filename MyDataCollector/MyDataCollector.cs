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


namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass
    {
        // later replace with an input
        public static string sharedFile; //the project csv file in DropBox        
        public static FileSystemWatcher CSVwatcher;
        public static FileSystemWatcher ImagesWatcher;
        //public static string csvFile;//can be the sharedFile or the HistoryFile
        public static string localFile;//local copy of sharedFile
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
        public static event EventHandler<TextArgs> Message = delegate { };
        //private static DataTable mergedDataTable;
        public static bool AutoPlay;
        public static string testValue;
        public static DateTime lastRead = DateTime.MinValue;
        public static DateTime lastWriteTime;
        public static string msg = "";
        public static DynamoView dv;
        public static DynamoModel dm;
        public static bool firstRun = true;
        public static bool extShare = false;

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
            oldDataTable = Functions.ListToTable(oldCsvList,"not localFile");
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
                localDataTable = Functions.ListToTable(csvList,csvs);
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
            //but before you have to build the common multiple, or use the union of dataTables
            //turn list of list of strings into List of DataTables
            List<DataTable> newParamTables = new List<DataTable>();
            newParamTables.Add(localDataTable);
            foreach (List<string> ls in pSHAREoutputs)
            {
                //with Automatic run the first ls has null in line two, giving errors!!!
                //In fact you should not overwrite Old Values!!! Or you should put in Old Value before, or after.
                DataTable newParamTable = MyDataCollector.Functions.ListToTable(ls, "not localFile");
                //With DataTable.Merge you overwrite existing records that have the same primary key (Parameter)
                //That is not what you want. Only the outputs of pSHARE. E.g. the Comments that you type in
                //the display should not be overwritten with those in the output. Same goes for Obstruction. 
                //So Merging is not a good idea, or put in the right data before. E.g. get rid of comments !!!
                //in pCOLLECT. Only in display, like obstruction.
                //But what about extra attributes? If they are not your own. Don't overwrite. But in newParamTable
                //you only have your own parameters! So no problem with that.
                foreach (DataRow dr in localDataTable.Rows)
                {
                    DataRow newDataRow = newParamTable.Rows.Find(dr["Parameter"]);
                    if (newDataRow != null)
                    {
                        //Add the new comment to the existing if it is not there yet.
                        if (!dr["Comments"].ToString().Contains(newDataRow["Comments"].ToString()))
                        //if (!newDataRow["Comments"].Equals(dr["Comments"]))
                        {
                            newDataRow.BeginEdit();
                            newDataRow["Comments"] = new Item(dr["Comments"].ToString() + "/" + newDataRow["Comments"].ToString());
                            newDataRow.EndEdit();
                        }
                        else
                        {
                            newDataRow["Comments"] = dr["Comments"];
                        }
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
            //merging tables needs more than 1 table of course. But if you connect only 1 pCOLLECT
            //you have at this point only 1 table. With one table you get the warning to put a List.Create
            //between pCOLLECT and pSHARE. But if you ignore that message, and hit the pSHARE button,
            //you get the warning to first hit the Run button and this keeps showing up.
            if (newParamTables.Count > 1)
            {
                DataTable TblUnion = Functions.MergeAll(newParamTables, "Parameter");
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
        }

        public static string projection(int i)
        {
            return "Value = " + i.ToString();
        }
        public static List<string> pCOLLECTinputs4(string _headers, string _P, string _V, string _I, string _C)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C);
            return m;
        }
        public static List<string> pCOLLECTinputs5(string _headers, string _P, string _V, string _I, string _C, string _E1)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C + ";" + _E1);
            return m;
        }
        public static List<string> pCOLLECTinputs6(string _headers, string _P, string _V, string _I, string _C, string _E1, string _E2)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C + ";" + _E1 + ";" + _E2);
            return m;
        }
        public static List<string> pCOLLECTinputs7(string _headers, string _P, string _V, string _I, string _C, string _E1, string _E2, string _E3)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C + ";" + _E1 + ";" + _E2 + ";" + _E3);
            return m;
        }
        public static List<string> pCOLLECTinputs8(string _headers, string _P, string _V, string _I, string _C, string _E1, string _E2, string _E3, string _E4)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C + ";" + _E1 + ";" + _E2 + ";" + _E3 + ";" + _E4);
            return m;
        }
        public static List<string> pCOLLECTinputs9(string _headers, string _P, string _V, string _I, string _C, string _E1, string _E2, string _E3, string _E4, string _E5)
        {
            List<string> m = new List<string>();
            m.Add(_headers);
            m.Add(_P + ";" + _V + ";" + _I + ";" + _C + ";" + _E1 + ";" + _E2 + ";" + _E3 + ";" + _E4 + ";" + _E5);
            return m;
        }
        public static List<string> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            //construct the file paths

            //check if the paths are *.csv files
            List<string> m = new List<string>();
            if (!_IfilePath.EndsWith(".csv") | !_LfilePath.EndsWith(".csv"))
            {
                Message(null, new TextArgs("Please make sure the file paths end with '.csv'"));
                m.Add("Please make sure the file paths end with '.csv'");
                return m;

            }
            //check if owner input is connected
            if (_owner == null)
            {
                Message(null, new TextArgs("Please connect the owner name to pSHARE..."));
                m.Add("Please connect the owner name to pSHARE...");
                return m;
            }
            //this runs every time you hit Run in Dynamo.
            sharedFile = _IfilePath;
            localFile = _LfilePath;
            //contruct the path for the oldLocalFile
            string dir = Path.GetDirectoryName(_LfilePath);
            string fileName = Path.GetFileName(_LfilePath);
            oldLocalFile = dir + "\\old_" + fileName;

            //check if sharedFile exist; otherwise make one with default headers.
            if(!_IfilePath.Equals("")){ Functions.FileExist(_IfilePath); }
            if (!_LfilePath.Equals("")) { Functions.FileExist(_LfilePath); }
            // the first time you run make a local copy of the shared csv file in DropBox;
            // and a copy from the local file to old_local file for comparing changes
            //in Grasshopper you copy the file from the DropBox every time...but no need, because OnChange.
            if (firstRun)
            {
                //no need to check if localFile exists. In first run always overwrite with DropBox file
                //But what if the user changes the paths?!!! You should keep track of that. Later.
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
                makeOldDataTable();
                firstRun = false;
            } 
            userName = _owner;
            pSHAREoutputs.Clear();
            bool OnepCOLLECT = false;
            foreach (List<string> item in _Ninputs)
            {
                //If you connect only 1 pCOLLECT directly to pSHARE you get an error while merging datatables
                //Strange enough _Niputs then is not a List<List<string>>, but a List<string>. And
                //this doesn't give an error... But item is then the first line of pCOLLECT output (the headers).
                //So in that case show a warning that you always should put a List.Create node in between.
                if (item.Count == 1)
                {
                    OnepCOLLECT = true;
                }
                else
                {
                    pSHAREoutputs.Add(item);
                }
            }
            if (OnepCOLLECT)
            {
                msg = "Please put a List.Create node between pCOLLECT and pSHARE...";
                dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBox.Show(dv, msg);
                }));
            }
            //The inputs of the pCOLLECTs must be added to the display of the csv file, changing the localDataTable property.
            //Populate localDataTable with the csv file
            if (extShare)
            {
                openCSV(sharedFile);
                makeOldDataTable();
            }            
            // Union the pCOLLECTs to localDataTable
            // Check if not only 1 pCOLLECT is connected otherwise you get an error
            List<string> pSHAREoutputList = new List<string>();
            if (pSHAREoutputs.Count > 0)
            {
                addNewPararemeters();
                //now localDataTable contains the union of the csv file and the new parameters
                //so, you can use the columns "Parameter" and "New Value"
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
                pSHAREoutputList.Add("WARNING: output is not generated. Did you connect the input without a List.Create?");
            }
            //when you change a parameter you should have immediate update of the display when you hit run.
            UpdateCSVControl(null, EventArgs.Empty);//includes Compare()
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
            CSVwatcher.Path = Path.GetDirectoryName(MyDataCollectorClass.sharedFile);
            CSVwatcher.NotifyFilter = NotifyFilters.LastWrite;
            CSVwatcher.Filter = Path.GetFileName(MyDataCollectorClass.sharedFile);
            CSVwatcher.Changed += new FileSystemEventHandler(OnChanged);
            CSVwatcher.EnableRaisingEvents = true;
            ImagesWatcher = new FileSystemWatcher();
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
            ImagesWatcher.EnableRaisingEvents = false;
            CSVwatcher.EnableRaisingEvents = false;
            //show the message on top of Dynamo. Because it comes from a different thread
            //you need a dispatcher. Should not work if you save yourself. So disable in Share command.
            lastWriteTime = File.GetLastWriteTime(sharedFile);
            string msg = "Some changes occured in the shared information. I will start over... ";
            //if (AutoPlay)
            //{
            //    //stop watching because otherwise you get nummerous messages
            //    ImagesWatcher.EnableRaisingEvents = false;
            //    CSVwatcher.EnableRaisingEvents = false;
            //}
            if (lastWriteTime != lastRead)
            {
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
                dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                 {
                     //MessageBox.Show(Application.Current.MainWindow, msg);
                     MessageBox.Show(dv, msg);
                     //Change the background colour to red

                     //copy the sharedFile to the localFile
                     File.Copy(sharedFile, localFile, true);
                     formPopulate = false;
                     //since you have to hit Run next three lines will run there
                     //openCSV(sharedFile);
                     //addNewPararemeters();
                     //makeOldDataTable();
                     //but hide the _CSVControl anyway to make clear something changed
                     //next line also runs when you hit Run, but is a way to hide the _CSVControl
                     //UpdateCSVControl(null, EventArgs.Empty);
                     ImagesWatcher.EnableRaisingEvents = true;
                     CSVwatcher.EnableRaisingEvents = true;
                     extShare = true;
                     //!!!try this
                     dm.ForceRun();
                 }));
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
                lastRead = lastWriteTime;
            }
            else
            {
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

