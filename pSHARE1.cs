using System.Collections.Generic;
using System.Windows;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI.Commands;
using ProtoCore.AST.AssociativeAST;
using System;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Dynamo.Wpf;
using MyDataCollector;
using Dynamo.Nodes;
using System.Linq;

namespace pCOLADnamespace
{
    #region some node settings
    /// pSHARE takes care of the communication of parameter changes through a shared *.csv file.
    [NodeName("pSHARE")]
    [NodeCategory("pCOLAD")]
    [NodeDescription("Load and share changes to parameters.")]
    [IsDesignScriptCompatible]
    #endregion
    public class pSHARE : NodeModel
    {
        #region properties
        public static string ttt;
        private string historyFile = "";
        private string oldCSV = "";
        private bool On = false;
        public static bool HistoryOn = false;
        //public static bool AutoPlayOn = false;
        private bool CheckAllButton = false;
        private bool UncheckAllButton = false;
        private string _OnOffButton = "";
        public DataTable latestMyDataTable;
        CSVControl _CSVControl;
        /// <summary>
        /// the property pSHARE.myPropDataTable is used as itemsSource for the datagrid
        /// </summary>
        private DataTable myPropDataTable;
        public DataTable MyPropDataTable
        {
            get { return myPropDataTable; }
            set
            {
                myPropDataTable = value;
                //The RaisePropertyChanged fires an event with the name "MyPropDataTable"
                //The CSVControl.myXamlTable is listening to it for its itemSource
                //but since it displays the item properties probably RaisePropertyChanged not necessary? No History doesn't work without.
                RaisePropertyChanged("MyPropDataTable");
            }
        }
        private List<string> testList;
        public List<string> TestList
        {
            get { return testList; }
            set
            {
                testList = value;
                //RaisePropertyChanged("TestList");
            }
        }
        public static DynamoModel dm;
        ////the idea is that in myImageFolderList we store a list of folder path objects (MyImageFolder)
        ////then we can bind to that list and display the MyImagePath property of the nested object (MyImage)        
        //private List<MyImageFolder> myImageFolderList;
        //public List<MyImageFolder> MyImageFolderList 
        //{
        //    get { return myImageFolderList; }
        //    set
        //    {
        //        myImageFolderList = value;
        //        RaisePropertyChanged("MyImageFolderList");
        //    }
        //}

        public void CSVUpdateHandler(object o, EventArgs e)
        {
            Compare();
            myPropDataTable = MyDataCollectorClass.myDataTable;
            RaisePropertyChanged("MyPropDataTable");
            //update the solution
            this.OnNodeModified(forceExecute: true);
            runtype(dm);
        }
        private int _rowIndex;
        public int RowIndex
        {
            get { return _rowIndex; }
            set
            {
                _rowIndex = value;
                if (_rowIndex < 0)
                {
                    _rowIndex = 0;
                }
                RaisePropertyChanged("RowIndex");
                // MessageBox.Show(string.Format("Row: {0}", _rowIndex.ToString()));
            }
        }
        private DataGridCellInfo _cellInfo;
        /// <summary>
        /// property of pSHARE about which cell is selected
        /// </summary>
        public DataGridCellInfo CellInfo
        {
            get { return _cellInfo; }
            set
            {
                _cellInfo = value;
                RaisePropertyChanged("CellInfo");
            }
        }
        #region old
        //private string _Comments;
        //public string MyComments
        //{
        //    get { return _Comments; }
        //    set
        //    {
        //        _Comments = value;
        //        RaisePropertyChanged("MyComments");
        //    }
        //}
        //private bool _newComments;
        //public bool newComments
        //{
        //    get { return _newComments; }
        //    set
        //    {
        //        _newComments = value;
        //        RaisePropertyChanged("newComments");
        //    }
        //}
        //private string _myNewValue;
        //public string MyNewValue
        //{
        //    get { return _myNewValue; }
        //    set
        //    {
        //        _myNewValue = value;
        //        RaisePropertyChanged("MyNewValue");
        //    }
        //}
        //private bool _newNewValue;
        //public bool newNewValue
        //{
        //    get { return _newNewValue; }
        //    set
        //    {
        //        _newNewValue = value;
        //        RaisePropertyChanged("newNewValue");
        //    }
        //}
        //private string _Importance;
        //public string MyImportance
        //{
        //    get { return _Importance; }
        //    set
        //    {
        //        _Importance = value;
        //        RaisePropertyChanged("MyImportance");
        //    }
        //}
        //private bool _newImportance;
        //public bool newImportance
        //{
        //    get { return _newImportance; }
        //    set
        //    {
        //        _newImportance = value;
        //        RaisePropertyChanged("newImportance");
        //    }
        //} 
        #endregion

        private bool _isChecked;
        /// <summary>
        /// property of pSHARE telling if a row is checked and so a value obstructed
        /// </summary>

        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                if (CheckAllButton)
                {
                    foreach (DataRow dr in MyDataCollectorClass.myDataTable.Rows)
                    {
                        string cellContent = dr["Obstruction"].ToString();
                        if (cellContent.Contains(MyDataCollectorClass.userName))
                        {
                            // remove username from the cell
                            cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                            //remove double and end commas
                            cellContent = Regex.Replace(cellContent, "/{2/}", "/").Trim('/');
                            dr["Obstruction"] = new Item(cellContent.Trim());
                        }
                    }
                    RaisePropertyChanged("isChecked");
                }
                else
                {
                    if (UncheckAllButton)
                    {
                        foreach (DataRow dr in MyDataCollectorClass.myDataTable.Rows)
                        {
                            string cellContent = dr["Obstruction"].ToString();
                            if (cellContent == "")
                            {
                                dr["Obstruction"] = new Item(MyDataCollectorClass.userName);
                            }
                            else
                            {
                                if (!cellContent.Contains(MyDataCollectorClass.userName))
                                {
                                    dr["Obstruction"] = new Item(cellContent + "/" + MyDataCollectorClass.userName);
                                }
                            }
                        }
                        RaisePropertyChanged("isChecked");
                    }
                    else
                    {
                        _isChecked = value;
                        //OnPropertyChanged("isChecked"); //this sets all checkboxes to checked...
                        DataRow dr = MyDataCollectorClass.myDataTable.Rows[_rowIndex];
                        //also change the value in the hidden column "Accepted"
                        //dr["Accepted"] = value;
                        string cellContent = dr["Obstruction"].ToString();
                        if (_cellInfo != null && !_isChecked) //add the userName
                        {
                            if (cellContent == "")
                            {
                                dr["Obstruction"] = new Item(MyDataCollectorClass.userName);
                            }
                            else
                            {
                                dr["Obstruction"] = new Item(cellContent + "/" + MyDataCollectorClass.userName);
                            }
                        }
                        else
                        {
                            // remove username from the cell
                            cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                            //remove double and end commas
                            cellContent = Regex.Replace(cellContent, "/{2/}", "/").Trim('/');
                            dr["Obstruction"] = new Item(cellContent.Trim());
                        }
                        RaisePropertyChanged("MyPropDataTable");
                    }
                }
            }
        }

        /// <summary>
        /// the property pSHARE.OnOffButton is used to open or close the CSV display
        /// </summary>
        public string OnOffButton
        {
            get { return _OnOffButton; }
            set
            {
                _OnOffButton = value;
                //Raise a property changed notification to alert the UI that an element needs an update
                //RaisePropertyChanged("NodeMessage");
                RaisePropertyChanged("OnOffButton");
            }
        }
        /// DelegateCommand objects allow you to bind UI interaction to methods on your data context.
        [IsVisibleInDynamoLibrary(false)]
        public DelegateCommand OnOff { get; set; }
        public DelegateCommand ShareCommand { get; set; }
        public DelegateCommand HistoryCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand CheckAllCommand { get; set; }
        public DelegateCommand UnCheckAllCommand { get; set; }

        //public DelegateCommand CommentsCommand { get; set; }
        //public DelegateCommand newCommentsCommand { get; set; }
        //public DelegateCommand NewValueCommand { get; set; }
        //public DelegateCommand newNewValueCommand { get; set; }
        //public DelegateCommand ImportanceCommand { get; set; }
        //public DelegateCommand newImportanceCommand { get; set; }

        /// <summary>
        /// Don't know why this is here. Maybe it was needed to hide a DelegateCommand
        /// </summary>
        /// <param name="workspace"></param>
        [IsVisibleInDynamoLibrary(false)]
        #endregion
        #region constructor
        /// The constructor for a NodeModel is used to create the input and output ports and specify the argument lacing.
        public pSHARE()
        {
            MyDataCollectorClass.UpdateCSVControl += CSVUpdateHandler;
            InPortData.Add(new PortData("N", "Input (a List.CreateList) of pCOLLECT output(s)"));
            InPortData.Add(new PortData("I", "Input a FilePath for the shared csv files."));
            InPortData.Add(new PortData("L", "Input a FilePath for the local copy of the csv file."));
            InPortData.Add(new PortData("U", "Input a the user namen (Code Block)."));
            OutPortData.Add(new PortData("O", "Output of parameter name and value on next line; two by two."));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.CrossProduct;
            OnOff = new DelegateCommand(ShowParams, CanShowParams);
            ShareCommand = new DelegateCommand(Share, CanShare);
            HistoryCommand = new DelegateCommand(History, CanHistory);
            CancelCommand = new DelegateCommand(Cancel, CanCancel);
            CheckAllCommand = new DelegateCommand(CheckAll, CanCheckAll);
            UnCheckAllCommand = new DelegateCommand(UnCheckAll, CanUnCheckAll);
            //CommentsCommand = new DelegateCommand(Comments, CanComments);
            //newCommentsCommand = new DelegateCommand(newComments, CanNewComments);
            //NewValueCommand = new DelegateCommand(NewValue, CanNewValue);
            //newNewValueCommand = new DelegateCommand(NewNewValue, CanNewNewValue);
            //ImportanceCommand = new DelegateCommand(Importance, CanImportance);
            //newImportanceCommand = new DelegateCommand(NewImportance, CanNewImportance);
            // update UI            
            OnOffButton = "Share";
        }
        #endregion
        #region public methods
        /// <summary>
        /// If this method is not overriden, Dynamo will, by default
        /// pass data through this node. So we should later use it to pass the OutputList.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></return
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //Func<int, string> projection = x => "Value=" + x;
            var x = new Func<int,string> (MyDataCollectorClass.projection);
            int[] values = { 3, 7, 10 };
            var strings = values.Select(MyDataCollectorClass.projection);

            foreach (string s in strings)
            {
                Console.WriteLine(s);
            }


            var t = new Func<List<List<string>>, string, string, string, List<string>>(MyDataCollectorClass.pSHAREinputs);
            string testj = MyDataCollectorClass.inputFile;
            //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
            var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);
            return new[] 
            { 
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)
             };
        }
        /// <summary>
        /// shows the CSV display
        /// </summary>
        public void ShowCSV()
        {
            //make sure that control doesn't exist.
            try
            {
                //check if the control exist already
                bool isCSVControlOpen = false;
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is CSVControl)
                    {
                        isCSVControlOpen = true;
                        w.Show();
                        w.Activate();
                    }
                }
                if (!isCSVControlOpen)
                {
                    //the CSVControl should be created only once
                    _CSVControl = new CSVControl();

                    //List<MyImageFolder> mifs = new List<MyImageFolder>();//contains MyImageFolder with prop MyImageFolderPath and List<MyImage>
                    //List<MyImage> mis = new List<MyImage>();//contains MyImage with prop MyImagePath
                    //List<string> mip = new List<string>();//contains MyImagePath
                    //for (int i = 0; i < myPropDataTable.Rows.Count; i++)
                    //{
                    //    Item it = (Item)myPropDataTable.Rows[i]["Parameter"];
                    //    string pn = (string)it.Value;//the name of the parameter
                    //    mip= Functions.imagePaths(pn);//list of image paths in folder with parameter name
                    //    MyImageFolder mif = new MyImageFolder(pn);
                    //    foreach (var ip in mip)
                    //    {
                    //        MyImage mi = new MyImage(ip);
                    //        mis.Add(mi);
                    //        //mif.MyImageList.Add(mi);
                    //    }
                    //    mif.MyImageList = mis;
                    //    mifs.Add(mif);                        
                    //}
                    //MyImageFolderList = mifs;

                    //List<string> myTestList = new List<string>();
                    //for (int i = 0; i < MyImageFolderList.Count + 1; i++)
                    //{
                    //    myTestList.Add(i.ToString());
                    //}
                    //TestList = myTestList;
                    //compare the csv file with the copy
                    Compare();
                    //niet nodig?
                    //this.MyPropDataTable = MyDataCollectorClass.myDataTable;
                    //bind the datatable to the xaml datagrid
                    //_CSVControl.myXamlTable.ItemsSource = this.MyPropDataTable.DefaultView;
                    ////Binding CSVControlBinding = new Binding("MyDataTableProp");
                    ////CSVControlBinding.Mode = BindingMode.TwoWay;
                    //_CSVControl.myXamlTable.ItemsSource = MyDataCollectorClass.myDataTable.DefaultView;
                    ////_CSVControl.myXamlTable.SetBinding(DataGrid.ItemsSourceProperty, CSVControlBinding);
                    _CSVControl.DataContext = this;
                    _CSVControl.Show();
                }
                else
                {
                    _CSVControl.Show();
                }
            }

            catch (System.Exception e)
            {
                MessageBox.Show("Exception deze source: {0}", e.Source);
            }
        }
        public class pSHARENodeViewCustomization : INodeViewCustomization<pSHARE>
        {
            /// <summary>
            /// At run-time, this method is called during the node 
            /// creation. Here you can create custom UI elements and
            /// add them to the node view, but we recommend designing
            /// your UI declaratively using xaml, and binding it to
            /// properties on this node as the DataContext.
            /// </summary>
            /// <param name="model">The NodeModel representing the node's core logic.</param>
            /// <param name="nodeView">The NodeView representing the node in the graph.</param>
            //probably Dynamo has a method that makes it go here asa pSHARE is loaded

            public void CustomizeView(pSHARE model, NodeView nodeView)
            {
                var pSHAREControl = new pSHAREcontrol();
                nodeView.inputGrid.Children.Add(pSHAREControl);
                pSHAREControl.DataContext = model;                
                Dynamo.ViewModels.NodeViewModel vm = nodeView.ViewModel;
                Dynamo.Models.NodeModel nm = vm.NodeModel;
                Dynamo.ViewModels.DynamoViewModel dvm = vm.DynamoViewModel;
                pSHARE.dm = dvm.Model;
            }
            /// <summary>
            /// Here you can do any cleanup you require if you've assigned callbacks for particular 
            /// UI events on your node.
            /// </summary>

            public void Dispose() { }

        }
        /// <summary>
        /// try to get Dynamo recalculate the solution when you hit On button
        /// </summary>
        /// <param name="actual"></param>
        public void recalc(DynamoModel actual)
        {
            //if pSHARE is ON the solution should be recalculated. Doesn't acutally work.
            actual.ResetEngine(true);
        }
        public void runtype(DynamoModel actual)
        {
            //!!!check if Automatic running is on
            DynamoModel dm = actual;
            foreach (var item in dm.Workspaces)
            {
                if (item.GetType() == typeof(HomeWorkspaceModel))
                {
                    HomeWorkspaceModel hm = (HomeWorkspaceModel)item;
                    RunType rt = hm.RunSettings.RunType;
                    if (rt == RunType.Automatic)
                    {
                        MyDataCollectorClass.AutoPlay = true;
                        hm.RunSettings.RunType = RunType.Manual;//is needed to avoid hanging when filesystemwatcher fires
                    }
                    else
                    {
                        MyDataCollectorClass.AutoPlay = false;
                    }
                }
            }
        }
        /// <summary>
        /// close the CSV display
        /// </summary>
        public void closeCSVControl()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is CSVControl)
                {
                    //w.Hide();
                    //MyDataCollectorClass.myDataTable = MyDataCollectorClass.csvDataTable.Copy();
                    //MyPropDataTable = MyDataCollectorClass.myDataTable;
                    w.Close();
                }
            }
        }
        #endregion
        #region command methods
        private bool CanShowParams(object obj)
        {
            return true;
        }
        private bool CanShare(object obj)
        {
            return true;
        }
        private bool CanHistory(object obj)
        {
            return true;
        }
        private bool CanCancel(object obj)
        {
            return true;
        }
        private bool CanCheckAll(object obj)
        {
            return true;
        }
        private bool CanUnCheckAll(object obj)
        {
            return true;
        }
        private void ShowParams(object obj)
        {
            //switch the On boolean to show or not the *.csv file
            if (On == false)
            {
                //and show the *.csv file if the solution ran before
                if (MyPropDataTable != null)
                {
                    On = true;
                    ShowCSV();
                }
                else
                {
                    MessageBox.Show("Please hit the Run button first...");
                    //set the button to red again
                    RaisePropertyChanged("OnOff");
                }
            }
            else
            {
                On = false;
                //close *.csv display
                closeCSVControl();
            }

        }
        private void Share(object obj)
        {
            //write myDataTable to the csv files if something changed
            Boolean newHistoryFile = false;
            myPropDataTable = MyDataCollectorClass.myDataTable;
            string csv = Functions.ToCSV(myPropDataTable, "myPropDataTable");
            if (oldCSV == csv)
            {
                MessageBox.Show("Nothing changed with last share ...");
            }
            else
            {
                try
                {
                    //myPropDataTable = MyDataCollectorClass.myDataTable;
                    //avoid SystemFileWatcher to fire when you save the csv file yourself.
                    MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = false;
                    File.WriteAllText(MyDataCollectorClass.inputFile, csv);
                    File.WriteAllText(MyDataCollectorClass.inputFileCopy, csv);
                    //add a timestamp and owner name to the author column for the History file
                    //int lastrow = MyDataCollectorClass.myDataTable.Rows.Count - 1;
                    DateTime time = DateTime.UtcNow;
                    myPropDataTable.Rows[1]["Date"] = new Item("utc " + time.ToString());
                    myPropDataTable.Rows[1]["Author"] = new Item(MyDataCollectorClass.userName);
                    csv = Functions.ToCSV(myPropDataTable, "myPropDataTable");
                    historyFile = MyDataCollectorClass.inputFile.Remove(MyDataCollectorClass.inputFile.LastIndexOf("\\") + 1) + "History.csv";
                    //check if file exist
                    if (!File.Exists(historyFile))
                    {
                        //File.Copy(MyDataCollectorClass.inputFile, historyFile);
                        File.Create(historyFile).Close();
                        newHistoryFile = true;
                    }
                    //Save the image file names to the historyFile
                    //Replace the Item.imageList property in column "Images"
                    //by a List<string> imageFileNameList property of Item
                    string historyCSV = "";
                    DataTable historyDataTable = myPropDataTable.Clone();
                    historyDataTable.Columns[0].DataType = typeof(List<string>);

                    foreach (DataRow dr in myPropDataTable.Rows)
                    {
                        //Item temp; // = new Item("");
                        //temp = (Item)dr["Parameter"];
                        Item temp = (Item)dr["Images"];
                        DataRow historyDr = historyDataTable.NewRow();
                        foreach (DataColumn dc in myPropDataTable.Columns)
                        {
                            if (dc.ColumnName == "Images")
                            {
                                historyDr["Images"] = temp.imageFileNameList;
                            }
                            else
                            {
                                historyDr[dc.ColumnName] = dr[dc.ColumnName];
                            }
                        }
                        historyDataTable.Rows.Add(historyDr);
                        //dr["Images"] = temp.imageFileNameList;
                    }
                    historyCSV = Functions.ToCSV(historyDataTable, "historyDataTable");
                    if (newHistoryFile)
                        {
                        File.AppendAllText(historyFile, historyCSV);
                    }
                    else
                    {
                        File.AppendAllText(historyFile, Environment.NewLine + historyCSV);
                    }

                    //reset myPropDataTable to myDataTable to get rid of the time stamp
                    myPropDataTable = MyDataCollectorClass.myDataTable;
                    ShowParams(OnOff);//closes the CSVControl and sets the On property to false
                    RaisePropertyChanged("OnOff"); //sets the OnOff button to red
                    //you should reset everything so next hit of OnOff button shows only new changes

                    MyDataCollectorClass.formPopulate = false;
                    //since formPopulate is false you will read the csv file. But if it was just created
                    //there are no items
                    MyDataCollectorClass.openCSV();
                    MyDataCollectorClass.addNewPararemeters();
                    oldCSV = csv;
                    MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
                }
                catch (System.Exception e)
                {
                    MessageBox.Show("Exception source: {0}", e.Message);
                    MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
                }
                //pSHAREcontrol.myButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));               

            }
        }
        private void History(object obj)
        {
            //show History.csv
            if (MyDataCollectorClass.inputFile == null)
            {
                MessageBox.Show("Please connect file path to pSHARE and run the solution ...");
            }
            else
            {
                if (!HistoryOn)//HistoryOn is true when you show the History file, false if you hit the button to hide
                {
                    string HistoryFile = MyDataCollectorClass.inputFile.Remove(MyDataCollectorClass.inputFile.LastIndexOf("\\") + 1) + "History.csv";
                    if (!File.Exists(HistoryFile))
                    {
                        MessageBox.Show("There is no history file (yet). Please hit the Share button first ...");
                        //reset the buttons
                        //HistoryOn = true;
                        //History(new Object());
                        return;
                    }
                    MyDataCollectorClass.formPopulate = false;
                    //first set myDataTable to the History file by changing the inputFile property.
                    MyDataCollectorClass.inputFile = HistoryFile;
                    MyDataCollectorClass.openCSV();
                    //apperently MyPropDataTable is not the same as myDataTable...
                    this.MyPropDataTable = MyDataCollectorClass.myDataTable;
                    HistoryOn = true;
                }
                else
                {
                    //Show the csv file and add new parameters
                    MyDataCollectorClass.inputFile = MyDataCollectorClass.ShareInputFile;
                    //Re-open csv is avoided because formPopulate is true
                    MyDataCollectorClass.openCSV();
                    MyDataCollectorClass.addNewPararemeters();
                    Compare();
                    this.MyPropDataTable = MyDataCollectorClass.myDataTable;
                    HistoryOn = false;
                }
            }
        }
        private void Cancel(object obj)
        {
            //closeCSVControl();
            ShowParams(OnOff);
            RaisePropertyChanged("OnOff");
        }
        private void CheckAll(object obj)
        {
            CheckAllButton = true;
            this._isChecked = true;
            this.isChecked = true;
            CheckAllButton = false;
        }
        private void UnCheckAll(object obj)
        {
            UncheckAllButton = true;
            this._isChecked = false;
            this.isChecked = false;
            UncheckAllButton = false;
        }
        private void Compare()
        {
            //compare csv and copy of csv here. Now only works after saving first time!!!
            //compare the comment, New Value and importance values of the two tables
            //but not for the History file
            ////for this to work you will have to fill myDataTable with new Items, 
            ////because INotifyPropertyChanged is not implemented there.
            //for (int i = 0; i < MyDataCollectorClass.myDataTable.Columns.Count; i++)
            //{
            //    for (int j = 0; j < MyDataCollectorClass.myDataTable.Rows.Count; j++)
            //    {
            //        MyDataCollectorClass.myDataTable.Rows[j][i] = new Item(MyDataCollectorClass.myDataTable.Rows[j][i].ToString());
            //    }
            //}

            if (MyDataCollectorClass.inputFile != null && !MyDataCollectorClass.inputFile.Contains("History"))
            {
                if (MyDataCollectorClass.copyDataTable.Rows.Count < 1)
                {
                    return;
                }
                DataRow drc = MyDataCollectorClass.copyDataTable.Rows[0];
                for (int i = 0; i < MyDataCollectorClass.myDataTable.Rows.Count; i++)
                {
                    //normally copyDataTable has less rows then myDataTable
                    //all extra rows in myDataTable should be red, so add strange strings
                    if (i < MyDataCollectorClass.copyDataTable.Rows.Count)
                    {
                        drc = MyDataCollectorClass.copyDataTable.Rows[i];
                    }
                    else
                    {
                        drc = MyDataCollectorClass.copyDataTable.NewRow();
                        for (int k = 0; k < MyDataCollectorClass.copyDataTable.Columns.Count; k++)
                        {
                            //if (MyDataCollectorClass.copyDataTable.Columns[k].ColumnName=="Images")
                            //{
                            //    drc[k] = new List<MyImage>();
                            //}
                            //else
                            //{
                            drc[k] = new Item("@#$%!");
                            //}
                            //don't add the row to the table because then you get trouble with primarykey
                        }
                    }
                    DataRow dr = MyDataCollectorClass.myDataTable.Rows[i];
                    for (int j = 0; j < MyDataCollectorClass.myDataTable.Columns.Count; j++)
                    {
                        string cn = MyDataCollectorClass.myDataTable.Columns[j].ColumnName;
                        //if pCOLLECT adds an attribute the column does not exist yet in copyDataTable
                        if (!MyDataCollectorClass.copyDataTable.Columns.Contains(cn))
                        {
                            MyDataCollectorClass.copyDataTable.Columns.Add(cn, typeof(Item));
                        }
                        if (!Object.Equals(drc[cn], dr[cn]))
                        {
                            //Item x = new Item(dr[cn].ToString());
                            //x.IsChanged = true;
                            //no idea why this can happen...
                            if (dr[cn] as MyDataCollector.Item == null)
                            {
                                dr[cn] = new Item("");
                            }
                            (dr[cn] as MyDataCollector.Item).SetChanged();
                        }
                        else
                        {
                            if (dr[cn] as MyDataCollector.Item == null)
                            {
                                dr[cn] = new Item("");
                            }
                            (dr[cn] as MyDataCollector.Item).SetSame();
                        }
                    }

                    //if comments are different and editted by somebody else
                    //then you should do something!!!

                    //if (!Object.Equals(drc["Comments"], dr["Comments"]))
                    //{
                    //    (dr["Comments"] as MyDataCollector.Item).SetChanged();
                    //}
                    ////else
                    ////{
                    ////    (dr["Comments"] as MyDataCollector.Item).SetSame();
                    ////}
                    //if (!Object.Equals(drc["New Value"], dr["New Value"]))
                    //{
                    //    (dr["New Value"] as MyDataCollector.Item).SetChanged();
                    //}
                    ////else
                    ////{
                    ////    (dr["New Value"] as MyDataCollector.Item).SetSame();
                    ////}
                    //if (!Object.Equals(drc["Importance"], dr["Importance"]))
                    //{
                    //    (dr["Importance"] as MyDataCollector.Item).SetChanged();
                    //}
                    ////else
                    ////{
                    ////    (dr["Importance"] as MyDataCollector.Item).SetSame();
                    ////}
                }
                myPropDataTable = MyDataCollectorClass.myDataTable;
            }
        }
        #endregion
    }
}
