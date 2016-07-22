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
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Media.Imaging;
using Dynamo.Graph.Nodes.CustomNodes;

namespace pCOLADnamespace
{
    #region some node settings
    /// pSHARE takes care of the communication of parameter changes through a shared *.csv file.
    [NodeName("pSHARE")]
    [NodeCategory("pCOLAD")]
    [NodeDescription("Load and share changes to parameters.")]
    [IsDesignScriptCompatible]
    //[InPortNamesAttribute("N", "I", "L", "U")]
    //[InPortDescriptionsAttribute(
    //    "Input (a List.CreateList) of pCOLLECT output(s)",
    //    "Input a FilePath for the shared csv files.",
    //    "Input a FilePath for the local copy of the csv file.",
    //    "Input a the user namen (Code Block).")]
    //[InPortTypesAttribute ("string", "string", "string", "string")]
    //[OutPortNamesAttribute ("O")]
    //[OutPortDescriptionsAttribute ("Output of parameter name and value on next line; two by two.")]
    //[OutPortTypesAttribute ("string")]

    #endregion
    public class pSHARE : NodeModel
    {
        #region properties
        public static string selectedImagePath;
        public static string searchFolder;
        public static string ttt;
        private string historyFile = "";
        private string oldCSV = "";
        private bool On = false;
        public static bool historyOn;
        public static SolidColorBrush changeColour = System.Windows.Media.Brushes.Pink;
        public bool HistoryOn
        {
            get { return historyOn; }
            set
            {
                historyOn = value;
                RaisePropertyChanged("HistoryOn");
            }
        }
        private bool CheckAllButton = false;
        private bool UncheckAllButton = false;
        private string _OnOffButton = "";
        public DataTable latestlocalDataTable;
        public CSVControl _CSVControl;
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
        public static Dynamo.ViewModels.DynamoViewModel dvm;
        public static NodeView nv;
        public static DynamoView dv;

        public void MessageHandler(object o, TextArgs e)
        {
            dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                System.Windows.MessageBox.Show(pSHARE.dv, e.Message);
            }));
        }
        [STAThread]
        public void CSVUpdateHandler(object o, EventArgs e)
        {
            //compares localDataTable with oldDataTable
            Compare();
            //updating MyPropDataTable is now inside Compare()
            //MyPropDataTable = MyDataCollectorClass.localDataTable;
            //RaisePropertyChanged("MyPropDataTable");//is done automatically if you set MyPropDataTable
            //update the solution
            //check if runtype is Automatic or Manual. This should be done before OnChange
            if (dm!=null)
            {
            runtype(dm);
            }
            //in automatic mode forceExecute causes recursion
            if (!MyDataCollectorClass.AutoMaticMode)
            {
                this.OnNodeModified(forceExecute: true);
            }
            //switch off the csv display if somebody changed shared file externally
            //you can not do it there, because detection of change is in MyDataCollector
            //and you don't have access to properties of pSHARE there, also related to watch()
            //which is also there
            if (!MyDataCollectorClass.formPopulate)
            {
                ShowParams(OnOff);//closes the CSVControl and sets the On property to false
                RaisePropertyChanged("OnOff"); //sets the OnOff button to red
            }
        }
        private int _rowIndex;
        public int RowIndex
        {
            get { return _rowIndex; }
            set
            {
                _rowIndex = value;
                //if (_rowIndex < 0)
                //{
                //    _rowIndex = 0;
                //}
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
                    foreach (DataRow dr in MyDataCollectorClass.localDataTable.Rows)
                    {
                        //DataRow MyPropDr = MyPropDataTable.Rows.Find(dr["Parameter"]);
                        string cellContent = dr["Obstruction"].ToString();
                        if (cellContent.Contains(MyDataCollectorClass.userName))
                        {
                            // remove username from the cell
                            cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                            //remove double and end commas
                            cellContent = Regex.Replace(cellContent, "/{2/}", "/").Trim('/');
                            checkOldDataTable(cellContent, dr);
                        }
                    }
                    RaisePropertyChanged("isChecked");
                }
                else
                {
                    if (UncheckAllButton)
                    {
                        foreach (DataRow dr in MyDataCollectorClass.localDataTable.Rows)
                        {
                            string cellContent = dr["Obstruction"].ToString();
                            if (cellContent == "")
                            {
                                cellContent = MyDataCollectorClass.userName;
                                checkOldDataTable(cellContent, dr);
                            }
                            else
                            {
                                if (!cellContent.Contains(MyDataCollectorClass.userName))
                                {
                                    cellContent += "/" + MyDataCollectorClass.userName;
                                    checkOldDataTable(cellContent, dr);
                                }
                            }
                        }
                        RaisePropertyChanged("isChecked");
                    }
                    else
                    {
                        _isChecked = value;
                        if (_rowIndex.Equals(-1))
                        {
                            return;
                            //_rowIndex = _CSVControl.drIndex;
                        }
                        DataRow dr = MyDataCollectorClass.localDataTable.Rows[_rowIndex];
                        string cellContent = dr["Obstruction"].ToString();
                        if (_cellInfo != null && !_isChecked) //add the userName
                        {
                            if (cellContent == "")
                            {
                                cellContent = MyDataCollectorClass.userName;
                                checkOldDataTable(cellContent, dr);
                            }
                            else
                            {
                                cellContent += "/" + MyDataCollectorClass.userName;
                                checkOldDataTable(cellContent, dr);
                            }
                        }
                        else
                        {
                            // remove username from the cell
                            cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                            //remove double and end commas
                            cellContent = Regex.Replace(cellContent, "/{2/}", "/").Trim('/');
                            checkOldDataTable(cellContent, dr);
                        }
                    }
                }
            }
        }
        private void checkOldDataTable(string _cellContent, DataRow _dr)
        {
            Item it = new Item(_cellContent.Trim());
            DataRow oldDr = MyDataCollectorClass.oldDataTable.Rows.Find(_dr["Parameter"]);
            //when you start oldDataTable has no rows
            if (oldDr == null)
            {
                oldDr = MyDataCollectorClass.oldDataTable.NewRow();
            }
            var set1 = new HashSet<string>(_cellContent.Split('/').Select(t => t.Trim()));
            var set2 = new HashSet<string>(oldDr["Obstruction"].ToString().Split('/').Select(t => t.Trim()));
            set1.Remove("");
            set2.Remove("");
            bool setsEqual = set1.SetEquals(set2);
            if (!setsEqual)
            {
                it.SetMyChanged();
            }
            _dr["Obstruction"] = it;
            DataRow myPropDr = myPropDataTable.Rows.Find(_dr["Parameter"]);
            myPropDr["Obstruction"] = it;
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
        public DelegateCommand menuActionCopy { get; set; }
        public DelegateCommand menuActionAddFromClipBoard { get; set; }
        public DelegateCommand menuActionDelete { get; set; }


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
            //you get here as soon as you start your Dynamo file with a pSHARE node in it.
            MyDataCollectorClass.UpdateCSVControl += CSVUpdateHandler;
            MyDataCollectorClass.Message += MessageHandler;
            InPortData.Add(new PortData("N", "Input (a List.CreateList) of pCOLLECT output(s)"));
            InPortData.Add(new PortData("PN", "Input a project name (Code Bolck)"));
            InPortData.Add(new PortData("I", "Input a Directory Path for the shared csv files."));
            InPortData.Add(new PortData("L", "Input a Directory Path for the local copy of the csv file."));
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

            menuActionCopy = new DelegateCommand(_menuActionCopy, CanCopy);
            menuActionAddFromClipBoard = new DelegateCommand(_menuActionAddFromClipBoard, CanAddFromClipBoard);
            menuActionDelete = new DelegateCommand(_menuActionDelete, CanDelete);

            //CommentsCommand = new DelegateCommand(Comments, CanComments);
            //newCommentsCommand = new DelegateCommand(NewComments, CanNewComments);
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
            //you get here everytime that solution runs
            //var e = new Func<object, string, string, string, string, List<string>>(MyDataCollectorClass.pSHAREemptyInput);
            var t = new Func<List<List<object>>, string, string, string, string, List<string>>(MyDataCollectorClass.pSHAREinputs);
            //this it to prepare a function for the pSHARE custom node. It runs at the start. You can not debug during
            //after running the solution.
            //string testj = MyDataCollectorClass.sharedFile;
            //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
            var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);
            //get the localDataTable in MyPropDataTable which is bound to the CSVControl
            //probably too soon to get the merged parameters in!!!
            //if (MyDataCollectorClass.localDataTable != null)
            //{
            //    MyPropDataTable = MyDataCollectorClass.localDataTable.Copy();
            //}
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
                //checking the processes doesn't work. Only Revit is found.
                //Dynamo.ViewModels.DynamoViewModel dv;
                //dv = 
                //Dynamo.UI.Prompts.EditWindow thisDynamo = new Dynamo.UI.Prompts.EditWindow();


                bool isCSVControlOpen = false;
                if (_CSVControl != null)
                {
                    _CSVControl.Close();
                    _CSVControl = null;
                }
                //Application.Current.Windows throws a null error in Revit/Dynamo

                //if (Application.Current != null)
                //{
                //    foreach (Window w in Application.Current.Windows)
                //    {
                //        if (w is CSVControl)
                //        {
                //            isCSVControlOpen = true;
                //            w.Show();
                //            w.Activate();
                //        }
                //    }

                //}                
                if (!isCSVControlOpen)
                {
                    //the CSVControl should be created only once
                    //Dynamo.ViewModels.NodeViewModel vm = pSHARE.nv.ViewModel;
                    //NodeModel nm = vm.NodeModel;
                    //Dynamo.ViewModels.DynamoViewModel dvm = vm.DynamoViewModel;  
                    //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                    //_CSVControl = new CSVControl();
                    //});
                    //avoid creating CSVControl from other thread than UI, because it has to be STA (single-threaded apartment)
                    //but you get here also via watcher, CSVupdateHandler, ShowParams... So use switching as field.
                    //if (MyDataCollectorClass.switching)
                    //{
                    //    return;
                    //}
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
                    Compare();//updating MyPropDataTable is now inside Compare()
                    //niet nodig?
                    //this.MyPropDataTable = MyDataCollectorClass.localDataTable;
                    //bind the datatable to the xaml datagrid
                    //_CSVControl.myXamlTable.ItemsSource = this.MyPropDataTable.DefaultView;
                    ////Binding CSVControlBinding = new Binding("localDataTableProp");
                    ////CSVControlBinding.Mode = BindingMode.TwoWay;
                    //_CSVControl.myXamlTable.ItemsSource = MyDataCollectorClass.localDataTable.DefaultView;
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
                dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    System.Windows.MessageBox.Show(dv, "Exception: {0}", e.Message);
                }));
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
                pSHARE.nv = nodeView;
                nodeView.inputGrid.Children.Add(pSHAREControl);
                pSHAREControl.DataContext = model;
                Dynamo.ViewModels.NodeViewModel vm = nodeView.ViewModel;
                //NodeModel nm = vm.NodeModel;                
                pSHARE.dvm = vm.DynamoViewModel;
                //you need the DynamoModel to check the runtype
                pSHARE.dm = dvm.Model;
                MyDataCollectorClass.dm = pSHARE.dm;
                //looking for a window to use as owner for messages and _CSVcontrol
                pSHARE.dv = FindUpVisualTree<DynamoView>(nv);
                MyDataCollectorClass.dv = pSHARE.dv;
                //subscribe to the shutdown event in order to avoid _CSVcontrol being left behind
                //first you need an instance of the DynamoModel//doesn't work and now because
                //DynamoView is owner of _CSVcontrol is closes automatically
                //dm.ShutdownStarted += closeCSVcontrolFrom_dm(dm);

            }
            /// <summary>
            /// Here you can do any cleanup you require if you've assigned callbacks for particular 
            /// UI events on your node.
            /// </summary>
            //private pSHARE parent;
            //public pSHARENodeViewCustomization()
            //{
            //}
            //public pSHARENodeViewCustomization(pSHARE parent)
            //{
            //    this.parent = parent;
            //}
            //public DynamoModelHandler closeCSVcontrolFrom_dm(DynamoModel dm)
            //{
            //    if (parent != null || parent._CSVControl != null)
            //    {
            //        parent._CSVControl.Close();
            //    }
            //    return null;
            //}

            private static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
            {
                DependencyObject current = initial;

                while (current != null && current.GetType() != typeof(T))
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                return current as T;
            }

            public void Dispose()
            {
            }

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
                        MyDataCollectorClass.AutoMaticMode = true;
                        //dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        //{
                        //    System.Windows.MessageBox.Show(pSHARE.dv, "Sorry, Automatic is not supported at this moment...");
                        //}));
                        //hm.RunSettings.RunType = RunType.Manual;//is needed to avoid hanging when filesystemwatcher fires
                    }
                    else
                    {
                        MyDataCollectorClass.AutoMaticMode = false;
                    }
                }
            }
        }
        /// <summary>
        /// close the CSV display
        /// </summary>
        public void closeCSVControl()
        {
            if (_CSVControl != null)
            {
                //if the user clicks the close button top right the .Close method gives an error!!!
                _CSVControl.Close();
                _CSVControl = null;                
            }
            //foreach (Window w in Application.Current.Windows)
            //{
            //    if (w is CSVControl)
            //    {
            //        //w.Hide();
            //        //MyDataCollectorClass.localDataTable = MyDataCollectorClass.csvDataTable.Copy();
            //        //MyPropDataTable = MyDataCollectorClass.localDataTable;
            //        w.Close();
            //    }
            //}
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
        private bool CanAddFromClipBoard(object obj)
        {
            return true;
        }
        private bool CanCopy(object obj)
        {
            return true;
        }
        private bool CanDelete(object obj)
        {
            return true;
        }
        private void ShowParams(object obj)
        {
            //switch the On boolean to show or not the *.csv file
            if (On == false)
            {
                //and show the *.csv file if the solution ran before
                if (MyDataCollectorClass.localDataTable != null)
                {
                    MyPropDataTable = MyDataCollectorClass.localDataTable.Copy();
                    On = true;
                    ShowCSV();
                    MyDataCollectorClass.firstRun = false;
                }
                else
                {
                    //MyPropDataTable = MyDataCollectorClass.localDataTable;
                    if (!MyDataCollectorClass.AutoMaticMode)
                    {
                    dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        System.Windows.MessageBox.Show(pSHARE.dv, "Please hit the Run button first...");
                    }));
                    //set the button to red again
                    RaisePropertyChanged("OnOff");
                    }
                }
            }
            else
            {
                On = false;
                //close *.csv display
                //but if user hit red X button top right than already closing and would give error
                //to avoid running CancelCommand again set .Canceling to true
                if (_CSVControl != null && !_CSVControl.ClosingStarted)
                {
                    _CSVControl.Canceling = true;
                    closeCSVControl();
                }
            }

        }
        //private bool CanNewComments(object obj)
        //{
        //    return true;
        //}
        //private void NewComments(object obj)
        //{
        //    //when the text in a comments field changed you get here
        //    //can I find out which cell was changed? What is obj?
        //    var x = obj;

        //}
        private void Share(object obj)
        {
            //write localDataTable to the csv files if something changed
            Boolean newHistoryFile = false;
            //myPropDataTable = MyDataCollectorClass.localDataTable;
            //When sharing myPropDataTable should be the same as MyDataCollectorClass.localDataTable
            //But comparing two datatables is time consuming (the == or Equals are always reference comparing)
            //So just make sure they are the same by copying. Copy() makes an independent copy.
            myPropDataTable = MyDataCollectorClass.localDataTable.Copy();

            //now check if New Values are different and if so change the Old Values
            foreach (DataRow dr in myPropDataTable.Rows)
            {
                DataRow odr = MyDataCollectorClass.oldDataTable.Rows.Find(dr["Parameter"]);
                if (odr != null)
                {
                    if (!(dr["New Value"].Equals(odr["New Value"])) &&
                (dr["Owner"] as MyDataCollector.Item).textValue == MyDataCollectorClass.userName)
                    {
                        dr["Old Value"] = odr["New Value"];
                    }
                }
            }
            //but now New Value and Old Value are always the same in csv!!!
            //in pCOLAD for Grasshopper you simply copy the sharedFile to the oldLocalCopy...
            //but there the Old Value gets updated every time you change the New Value.
            //If you do that in between shares then the Old Value is not the one in the oldLocalCopy...

            //Item oldNewItem, newItem;
            //for (int i = 0; i < oldTable.Rows.Count; i++)
            //{
            //    oldNewItem = (Item)oldTable.Rows[i]["New Value"];
            //    newItem = (Item)myPropDataTable.Rows[i]["New Value"];
            //    if (oldNewItem.textValue!=newItem.textValue)
            //    {
            //        MyPropDataTable.Rows[i]["Old Value"] = newItem;
            //    }
            //}
            string csv = Functions.ToCSV(myPropDataTable, "myPropDataTable");
            //but if you put the date in myPropDataTable after this, oldCSV will always be different from csv
            //so make comparison without the date. Maybe easier with tables. Check if this is not a new start.
            bool theSame = false;
            if (MyDataCollectorClass.oldDataTable.Rows.Count > 0)
            {
                DataTable newTable = myPropDataTable.Copy();
                DataTable oldTable = MyDataCollectorClass.oldDataTable.Copy();
                newTable.Rows[0]["Date"] = new MyDataCollector.Item("check");
                oldTable.Rows[0]["Date"] = new MyDataCollector.Item("check");
                newTable.Rows[0]["Author"] = new MyDataCollector.Item("check");
                oldTable.Rows[0]["Author"] = new MyDataCollector.Item("check");
                if (AreTablesTheSame(newTable, oldTable))
                {
                    theSame = true;
                    dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        System.Windows.MessageBox.Show(pSHARE.dv, "Nothing changed with last share ...");
                    }));
                }
            }
            if (!theSame)
            {
                try
                {
                    //add a timestamp and owner name to the author column for the History file
                    //int lastrow = MyDataCollectorClass.localDataTable.Rows.Count - 1;
                    DateTime time = DateTime.UtcNow;
                    myPropDataTable.Rows[0]["Date"] = new Item("utc " + time.ToString("dd/MM/yyyy HH:mm:ss"));
                    myPropDataTable.Rows[0]["Author"] = new Item(MyDataCollectorClass.userName);
                    csv = Functions.ToCSV(myPropDataTable, "myPropDataTable");
                    //myPropDataTable = MyDataCollectorClass.localDataTable;
                    //avoid SystemFileWatcher to fire when you save the csv file yourself.
                    MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = false;
                    File.WriteAllText(MyDataCollectorClass.sharedFile, csv);
                    //in Grasshopper save to local is not done, but there you run automatically and reload
                    //and copy local the sharedFile every time. Here only at start of a session with pSHARE
                    //and OnChange when somebody else changed the project csv file in DropBox
                    //but since you work with DataTables new and old no need to write either
                    //but how can there be a difference then to localFile and oldLocalFile?!!!

                    // not to local file otherwise you don't get difference between New Value and Old Value
                    // localFile will be copied when you hit Run
                    File.WriteAllText(MyDataCollectorClass.localFile, csv);
                    File.WriteAllText(MyDataCollectorClass.oldLocalFile, csv);
                    //in order to have OpenCSV(file path) process the local file 
                    MyDataCollectorClass.formPopulate = false;
                    //MyDataCollectorClass.makeOldDataTable();

                    ////but the date should be equal to that in sharedFile
                    ////otherwise files will never be eaqual!!!or keep date out of comparison!!!
                    //csv = Functions.ToCSV(myPropDataTable, "myPropDataTable");
                    historyFile = MyDataCollectorClass.sharedFile.Remove(MyDataCollectorClass.sharedFile.LastIndexOf("\\") + 1) + "History.csv";
                    //check if file exist
                    if (!File.Exists(historyFile))
                    {
                        //File.Copy(MyDataCollectorClass.sharedFile, historyFile);
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
                    #region old
                    ////in order to see the changes in the display of the parameters you must update 
                    ////the dependency property MyPropDataTable
                    ////or simply set isChanged of all items to false
                    ////no that doesn't go to the converter
                    ////ClearChanged();
                    ////myPropDataTable = MyDataCollectorClass.localDataTable;
                    ////closeCSVControl();
                    ////RaisePropertyChanged("MyPropDataTable");
                    ////foreach (DataRow dr in myPropDataTable.Rows)
                    ////{
                    ////    foreach (DataColumn dc in myPropDataTable.Columns)
                    ////    {
                    ////        (dr[dc.ColumnName] as MyDataCollector.Item).IsChanged = false;
                    ////    }
                    ////}
                    //MyDataCollectorClass.makeOldDataTable();
                    //Compare();
                    //ShowParams(OnOff);//closes the CSVControl and sets the On property to false
                    //                  //RaisePropertyChanged("OnOff");
                    //                  //CSVUpdateHandler compares localDataTable with oldDataTable
                    //                  //updates the solution
                    //                  //makes sure that runtype is manual
                    //                  //switches pSHARE button to off the and closes the CSVcontrol
                    //                  //CSVUpdateHandler(null, EventArgs.Empty);

                    //////reset myPropDataTable to localDataTable to get rid of the time stamp
                    ////myPropDataTable = MyDataCollectorClass.localDataTable;

                    //////reset everything so next hit of OnOff button shows only new changes
                    ////MyDataCollectorClass.formPopulate = false;
                    //////since formPopulate is false you will read the csv file. But if it was just created
                    //////there are no items
                    ////MyDataCollectorClass.openCSV(MyDataCollectorClass.sharedFile);
                    //////now MyDataCollectorClass.localDataTable is filled with the csv file (sharedFile)
                    //////and MyDataCollectorClass.oldDataTable is filled with the oldLocalCopy
                    ////MyDataCollectorClass.addNewPararemeters();
                    //////now MyDataCollectorClass.localDataTable contains also the output of pSHARE if different
                    ////Compare(); 
                    #endregion
                    //checked if this is necessary are they different? No they are the same.
                    //myPropDataTable = MyDataCollectorClass.localDataTable.Copy();
                    //if you compare with oldDataTable, when was that updated? Should be the same as myPropDataTable at this moment...
                    //check if they are differnt!!! They are: the date is different.
                    if (!MyDataCollectorClass.oldDataTable.Equals(myPropDataTable))
                    {
                        MyDataCollectorClass.oldDataTable = myPropDataTable.Copy();
                    }
                    //after a share no colours
                    ClearChanged();
                    MyPropDataTable = myPropDataTable.Copy();
                    //ClearChangedMyPropDataTable();
                    //close the CSVcontrol and recreate it so you get correct red backgrounds
                    ////ShowParams(OnOff);//closes the CSVControl and sets the On property to false
                    ////ShowParams(OnOff);//creates a new CSVControl and sets the On property to true
                    oldCSV = String.Copy(csv);
                    ///oldCSV = csv;
                    MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
                    //find a way to close it automatically or make your own AutoMessageXaml
                    TempMessage tm = new TempMessage();
                    tm.MessageString = "CSV file successfully saved...";
                    MyDataCollector.TempMessageXAML tma = new TempMessageXAML();
                    tma.DataContext = tm;
                    tma.Show();

                    //dm.ForceRun();
                    ////for Obstruction to show correctly you have to run twice. No idea why!!!
                    ////But Dynamo doesn't like this
                    ////dm.ForceRun();


                    //ShowParams(OnOff);//opens the CSVControl and sets the On property to true
                    //ShowCSV();
                    //MessageBox.Show(pSHARE.dv,"CSV file successfully saved...","pCOLAD",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.None);

                }
                catch (System.Exception e)
                {
                    dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        System.Windows.MessageBox.Show(pSHARE.dv, "Exception: {0}", e.Message);
                        MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
                    }));
                }
                //pSHAREcontrol.myButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));               

            }
        }
        private void History(object obj)
        {
            //show History.csv
            if (MyDataCollectorClass.sharedFile == null)
            {
                dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    System.Windows.MessageBox.Show(pSHARE.dv, "Please connect file path to pSHARE and run the solution ...");
                }));
            }
            else
            {
                if (historyOn)//historyOn is false when you show the History file
                {
                    string HistoryFile = MyDataCollectorClass.sharedFile.Remove(MyDataCollectorClass.sharedFile.LastIndexOf("\\") + 1) + "History.csv";
                    if (!File.Exists(HistoryFile))
                    {
                        dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            System.Windows.MessageBox.Show(pSHARE.dv, "There is no History.csv file (yet). Please hit the Share button first ...");
                        }));
                        //reset the buttons.
                        HistoryOn = false;
                        //History(HistoryCommand);
                        return;
                    }
                    MyDataCollectorClass.formPopulate = false;
                    MyDataCollectorClass.openCSV(HistoryFile);
                    //apperently MyPropDataTable is not the same as localDataTable...
                    this.MyPropDataTable = MyDataCollectorClass.localDataTable.Copy();
                    HistoryOn = true;
                }
                else
                {
                    //Show the csv file and add new parameters
                    //Re-open csv is avoided because formPopulate is true, and maybe own parameters are changed
                    MyDataCollectorClass.openCSV(MyDataCollectorClass.localFile);
                    MyDataCollectorClass.addNewPararemeters();
                    Compare();//updating MyPropDataTable is now inside Compare() but depending on somechange
                    MyPropDataTable = MyDataCollectorClass.localDataTable;
                    //CSVUpdateHandler(null, EventArgs.Empty);//contains Compare()
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
        private void ClearChanged()
        {
            foreach (DataRow localDataRow in MyDataCollectorClass.localDataTable.Rows)
            {
                foreach (DataColumn localDataColumn in MyDataCollectorClass.localDataTable.Columns)
                {
                    if ((localDataRow[localDataColumn] as MyDataCollector.Item) != null)
                    {
                        (localDataRow[localDataColumn] as MyDataCollector.Item).SetSame();
                    }
                }
            }
        }

        private void _menuActionCopy(object obj)
        {
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = false;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = false;
            List<string> files = new List<string>();
            string[] filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            List<MyImage> ImageList = new List<MyImage>();
            DataRow r0 = MyPropDataTable.Rows[RowIndex];
            Item i0 = (Item)r0["Images"];
            MyImage dummy = Functions.dummyFunction();
            string parName = r0["Parameter"].ToString();
            string dir = Path.GetDirectoryName(MyDataCollectorClass.sharedFile);
            searchFolder = dir + "\\Images\\" + parName;

            List<string> sourcePaths;
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = true;
            fd.Filter = "Image files (*.jpg; *.jpeg; *. png; *. gif; *. tiff; *. bmp) | *.jpg; *.jpeg; *. png; *. gif; *. tiff; *. bmp";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                sourcePaths = fd.FileNames.ToList();
                if (sourcePaths.Count == 0)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (searchFolder.Equals("empty"))
            #region fill files and Imagelist
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Please choose a folder where you want to add this image...";
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    searchFolder = fbd.SelectedPath;
                }
                else
                {
                    return;
                }
                //since searchFolder equals "empty" files is also empty
                //next function returns a list of strings with only the path names of files, with extension in filters
                files = Functions.GetFilesFrom(searchFolder, filters, false);
                //remove the button like image
                ImageList.Clear();
                foreach (var imagePath in files)
                {
                    MyImage im = new MyImage(imagePath);
                    ImageList.Add(im);
                }
            }
            #endregion
            //check if there are files with the same name in the target directory
            //store the copy actions in a dictionary and execute it only if there was no cancel
            Dictionary<string, string> CopyDict = new Dictionary<string, string>();
            List<string> targetPaths = new List<string>();
            foreach (MyImage pI in i0.ImageList)
            {
                targetPaths.Add(pI.MyImagePath);
            }
            //iterate over sourcePaths
            for (int sPC = sourcePaths.Count - 1; sPC >= 0; sPC--)
            {
                bool fileNamesSame = false;
                string sourceFileName = Path.GetFileName(sourcePaths[sPC]);
                //iterate over targetPaths to check if fileNames are the same
                for (int j = targetPaths.Count - 1; j >= 0; j--)
                {
                    string targetFileName = Path.GetFileName(targetPaths[j]);
                    if (sourceFileName == targetFileName)
                    {
                        fileNamesSame = true;
                        break;
                    }
                }
                if (!fileNamesSame)
                {
                    CopyDict.Add(sourcePaths[sPC], searchFolder + "\\" + sourceFileName);
                }
                else
                {
                    Dialogue1 D1 = new Dialogue1();
                    string a1 = "";
                    D1.Topmost = true;
                    D1.Question.Content = sourceFileName + " already exists. OK to replace, or please choose another name.";
                    D1.Answer.Text = sourceFileName;
                    D1.Answer.Focus();
                    D1.Answer.SelectAll();
                    var result = D1.ShowDialog();
                    //wait for the answer and store it or cancel
                    if ((bool)result)
                    {
                        a1 = D1.Answer.Text;
                    }
                    //D1.Closing += (sender, e) =>
                    //{
                    //    var d = sender as Dialogue1;
                    //    if (d.Canceled == false)
                    //    {
                    //        a1 = D1.Answer.Text;

                    //    }
                    //    else
                    //    {
                    //        return;
                    //    }
                    //};
                    if (a1 != "")
                    {
                        var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        imageExtensions.Add(".jpg");
                        imageExtensions.Add(".jpeg");
                        //imageExtensions.Add(".jpe");
                        //imageExtensions.Add(".jfif");
                        imageExtensions.Add(".png");
                        imageExtensions.Add(".bmp");
                        //imageExtensions.Add(".dib");
                        //imageExtensions.Add(".rle");
                        imageExtensions.Add(".gif");
                        //imageExtensions.Add(".tif");
                        imageExtensions.Add(".tiff");
                        //check if it is a valide image name
                        string extension = Path.GetExtension(a1);
                        if (!imageExtensions.Contains(extension))
                        {
                            System.Windows.MessageBox.Show("The file extension is not a valid image. Please try again...");
                            return;
                        }
                        //replace the fileName in dictionary CopyDict
                        CopyDict.Add(sourcePaths[sPC], searchFolder + "\\" + a1);
                        //i0.Iml[i] = new MyImage(a1);//no because you can't cancel later
                    }
                }
            }
            //now use the dictionary CopyDict to copy the files and update the PropDataTable
            //if the user Canceled one of the renaming you will not get here because of the return;s
            foreach (KeyValuePair<string, string> entry in CopyDict)
            {
                //don't have the watcher give an alert
                File.Copy(entry.Key, entry.Value, true);
                while (!File.Exists(entry.Value))
                {
                    Thread.Sleep(1000);
                }
                //check if this filename is already in the ImageList
                if (!i0.ImageFileNameList.Contains(Path.GetFileName(entry.Value)))
                {
                    i0.ImageList.Add(new MyImage(entry.Value));
                }
            }
            //if there is a dummy in the first row, remove it.
            if (i0.ImageList[0].myImageFileName == dummy.myImageFileName)
            {
                i0.ImageList.RemoveAt(0);
            }
            MyPropDataTable.AcceptChanges();
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = true;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
        }
        private void _menuActionAddFromClipBoard(Object obj)
        {
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = false;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = false;
            List<string> files = new List<string>();
            string[] filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            List<MyImage> ImageList = new List<MyImage>();
            DataRow r0 = MyPropDataTable.Rows[RowIndex];
            string parName = r0["Parameter"].ToString();
            string dir = Path.GetDirectoryName(MyDataCollectorClass.sharedFile);
            searchFolder = dir + "\\Images\\" + parName;
            Item i0 = (Item)r0["Images"];
            MyImage dummy = Functions.dummyFunction();

            if (searchFolder.Equals("empty"))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Please choose a folder where you want to add this image...";
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    searchFolder = fbd.SelectedPath;
                }
                else
                {
                    return;
                }
                //since searchFolder equals "empty" files is also empty
                //next function returns a list of strings with only the path names of files, with extension in filters
                files = Functions.GetFilesFrom(searchFolder, filters, false);
                //remove the button like image
                ImageList.Clear();
                foreach (var imagePath in files)
                {
                    MyImage im = new MyImage(imagePath);
                    ImageList.Add(im);
                }
            }
            if (System.Windows.Clipboard.GetDataObject() != null)
            {
                System.Windows.IDataObject data = System.Windows.Clipboard.GetDataObject();
                if (data.GetDataPresent(System.Windows.DataFormats.Bitmap))
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    if (!searchFolder.Equals("empty"))
                    {
                        saveFileDialog1.CustomPlaces.Add(searchFolder);
                        saveFileDialog1.InitialDirectory = searchFolder;
                    }
                    saveFileDialog1.AddExtension = true;
                    saveFileDialog1.DefaultExt = "Png";
                    saveFileDialog1.ValidateNames = true;
                    saveFileDialog1.Filter = "Png image (*.Png)|*.Png";
                    saveFileDialog1.OverwritePrompt = true;
                    saveFileDialog1.Title = "Please give a name for this file...";
                    saveFileDialog1.ShowDialog();
                    //string toBeDeleted = "";
                    string fp = saveFileDialog1.FileName;
                    if (fp == "")
                    {
                        //searchFolder = "empty";//this is risky in pCOLAD it should be set to the default
                        //unless it is the pCOLADdummy.bmp button like image. In fact you want the Parameter name etc.
                        //string parName = r0["Parameter"].ToString();
                        //string dir = Path.GetDirectoryName(MyDataCollectorClass.sharedFile);
                        searchFolder = dir + "\\Images\\" + parName;
                        //searchFolder = Path.GetDirectoryName(selectedImagePath);
                        return;
                    }
                    //if the overwriting a file is chosen, you get an IO error because the file is in use in the display
                    //so you first have to put the image in memory (through converter). 
                    //You also have to update the PropDataTable
                    //because the cells are bound to that and through a converter to the imageList
                    //so first find the right row in PropDataTable, then the Item, and then the image in the Item.Iml (imageList)
                    //hmm, the other way around
                    //but if from the start there is no searchfolder, then files is empty

                    if (files.Contains(fp))
                    {
                        for (int p = ImageList.Count - 1; p >= 0; p--)
                        {
                            if (ImageList[p].MyImagePath.Equals(fp))
                            {
                                //find this Item in PropDataTable
                                int ri = 0;
                                for (int i = 0; i < MyPropDataTable.Rows.Count; i++)
                                {
                                    if (MyPropDataTable.Rows[i][0] == i0)
                                    {
                                        ri = i;
                                        continue;
                                    }
                                }
                                //change the imageList property of this Item
                                i0.ImageList[p] = new MyImage(fp);
                                //change the PropDataTable so it updates in the xaml control
                                MyPropDataTable.AcceptChanges();//this sets the PropDataTable and runs the propertyChanged notifier
                            }
                        }
                    }
                    if (!fp.Equals(""))
                    {
                        var image = System.Windows.Clipboard.GetImage();
                        using (var fileStream = new FileStream(fp, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(fileStream);
                        }
                        while (!File.Exists(fp))
                        {
                            Thread.Sleep(1000);
                        }
                        //add the image to the display if it was not a replacement
                        if (!files.Contains(fp))
                        {
                            i0.ImageList.Add(new MyImage(fp));
                            //if there is a dummy in the first row, remove it.
                            if (i0.ImageList[0].myImageFileName == dummy.myImageFileName)
                            {
                                i0.ImageList.RemoveAt(0);
                            }
                            //change the PropDataTable so it updates in the xaml control
                            MyPropDataTable.AcceptChanges();//this sets the PropDataTable and runs the propertyChanged notifier
                        }
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("No image in Clipboard !!");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Clipboard Empty !!");
            }
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = true;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
        }
        private void _menuActionDelete(Object obj)
        {
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = false;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = false;
            List<string> files = new List<string>();
            string[] filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            List<MyImage> ImageList = new List<MyImage>();
            DataRow r0 = MyPropDataTable.Rows[RowIndex];
            Item i0 = (Item)r0["Images"];
            MyImage dummy = Functions.dummyFunction();

            string fp = selectedImagePath;//is set in code behind
            if (searchFolder.Equals("empty"))//then fp is the buttonlike immage. Abort.
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Please choose a folder where you want to delete an image, and try again...";
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    searchFolder = fbd.SelectedPath;
                    return;
                }
                else
                {
                    return;
                }
            }
            //since you might have added files that you want to delete you have to make sure all the displayed files are in the list 
            //next function returns a list of strings with only the path names of files, with extension in filters
            files = Functions.GetFilesFrom(searchFolder, filters, false);
            //remove the button like image
            ImageList.Clear();
            foreach (var imagePath in files)
            {
                MyImage im = new MyImage(imagePath);
                ImageList.Add(im);
            }
            //if deleting a file, you get an IO error because the file is in use in the display
            //so you first have to put the image in memory (through converter). 
            //You also have to update the PropDataTable
            //because the cells are bound to that and through a converter to the imageList
            //so first find the right row in PropDataTable, then the Item, and then the image in the Item.Iml (imageList)
            //hmm, the other way around
            //but if from the start there is no searchfolder, then files is empty
            if (files.Contains(fp))//it doesn't if user chose Cancel all the time
            {
                for (int p = i0.ImageList.Count - 1; p >= 0; p--)
                //why? To find the right image with selectyedImagePath.
                //Why not show a file selector, so you can delete several files at once and use common shortcuts
                //to permanently delete or put in recycle bin? Because in History you would like to be able to find
                //the files and most of the time you only want to delete 1 file. It should be just one click.
                {
                    if (i0.ImageList[p].MyImagePath.Equals(fp))
                    {
                        //find this Item in PropDataTable
                        int ri = 0;
                        for (int i = 0; i < MyPropDataTable.Rows.Count; i++)
                        {
                            if (MyPropDataTable.Rows[i][0] == i0)
                            {
                                ri = i;
                                break;
                            }
                        }
                        //File.Delete(fp);//this peremanently deletes the file 
                        string deletedImagesDir = Path.GetDirectoryName(fp).Replace("Images", "Deleted Images");
                        if (!Directory.Exists(deletedImagesDir))
                        {
                            Directory.CreateDirectory(deletedImagesDir);
                        }
                        string movedFp = fp.Replace("Images", "Deleted Images");
                        //int n = 1;
                        while (File.Exists(movedFp))
                        #region addNumberToFile
                        {
                            //just add '(nr)' to filepath. No need to bather the user with dialogues. Also useful for History
                            string delDir = Path.GetDirectoryName(movedFp) + "\\";
                            string fn = Path.GetFileNameWithoutExtension(movedFp);
                            string ext = Path.GetExtension(movedFp);
                            //check if there is a file with same name that already ends with (number)

                            int end = fn.LastIndexOf(")");
                            int begin = fn.LastIndexOf("(");
                            if (end != -1 && begin != -1)
                            {
                                string number = fn.Substring(begin + 1, end - begin - 1);
                                int n;
                                bool isNumeric = int.TryParse(number, out n);
                                if (isNumeric)
                                {
                                    //replace the (number) by (number + 1)
                                    number = (n + 1).ToString();
                                    fn = fn.Substring(0, begin + 1) + number + ")";
                                    movedFp = delDir + fn + ext;
                                }
                                else
                                {
                                    movedFp = delDir + fn + "(1)" + ext;
                                }
                            }
                            else
                            {
                                movedFp = delDir + fn + "(1)" + ext;
                            }
                        }
                        #endregion
                        //if this was the last file in the folder, you have to put pCOLADdummy.bmp back in
                        if (files.Count == 1)
                        {
                            dummy = Functions.dummyFunction();
                            i0.ImageList[0] = dummy;
                        }
                        else
                        {
                            //change the imageList property of this Item
                            i0.ImageList.RemoveAt(p);
                        }
                        //change the PropDataTable so it updates in the xaml control
                        MyPropDataTable.AcceptChanges();//this sets the PropDataTable and runs the propertyChanged notifier                            
                        File.Move(fp, movedFp);
                    }
                }
            }
            MyDataCollectorClass.ImagesWatcher.EnableRaisingEvents = true;
            MyDataCollectorClass.CSVwatcher.EnableRaisingEvents = true;
        }
        public static bool AreTablesTheSame(DataTable tbl1, DataTable tbl2)
        {
            if (tbl1.Rows.Count != tbl2.Rows.Count || tbl1.Columns.Count != tbl2.Columns.Count)
                return false;


            for (int i = 0; i < tbl1.Rows.Count; i++)
            {
                for (int c = 0; c < tbl1.Columns.Count; c++)
                {
                    if (!Equals(tbl1.Rows[i][c], tbl2.Rows[i][c]))
                        return false;
                }
            }
            return true;
        }
        private void Compare()
        {
            bool someChange = false;
            //compare csv and copy of csv here. Now only works after saving first time!!!
            //if a cell in localDataTable is different from a corresponing cell in oldDataTable, make it red
            //you get an error if the corresponding cell does not exist
            //onother thing is that you can not rely on the same order of columns, so use names
            //also the Parameter column is primary, so must have unique values. You can not just add empty rows!!!

            //During processing initially localDataTable contains copy of shared csv file + own parameters. 
            //If you compare a cell that doesn’t exist in one or the other data table you get an error. 
            //For rows you can check existance if the Rows.Find(“key value”) returns null. 
            //For columns check existance if the oldDataTable.Column[localDataTable.Column.ColumnName] == null. 
            //So with a foreach loop through all rows of localDataTable and 
            //If Rows.Find (“key value”) != null  and If oldDataTable.Column[localDataTable.Column.ColumnName] != null 
            //a foreach sub loop through the columns ensures that you can compare the Items in the cells. 
            //Else and Else run SetChanged() on the Item in the localDataTable.

            //But what about deleted columns or rows? For later!!!
            //Best would be to mark them with '?' in front of the value and delete them if everybody removes his/her objection
            //How to mark them in the display? Can the text get strike through? Yes with converter (made it already).

            #region newCompare
            // first reset all items
            //ClearChanged();
            if (MyDataCollectorClass.localDataTable == null)
            {
                return;
            }
            foreach (DataRow localDataRow in MyDataCollectorClass.localDataTable.Rows)
            {
                DataRow oldDataRow = MyDataCollectorClass.oldDataTable.Rows.Find(localDataRow["Parameter"]);
                if (oldDataRow != null)
                {
                    foreach (DataColumn localDataColumn in MyDataCollectorClass.localDataTable.Columns)
                    {
                        //Old Value never turn red because it only changes after hitting the Share Button, or
                        //if somebody else changes a New Value and hit the Share Button, but then New Value is red.
                        //In fact it would be good to have different colour if you change your self...
                        if (localDataColumn.ColumnName == "Old Value")
                        {
                            continue;
                        }
                        //Item in localDataColumn can be null WHY? if there is no oldLocalFile yet??
                        //then you can not compare it, but won't you get an error when sharing?
                        //So add an empty Item
                        MyDataCollector.Item test1 = (localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item);
                        if (test1 == null)
                        {
                            localDataRow[localDataColumn.ColumnName] = new MyDataCollector.Item("");
                            someChange = true;
                        }
                        //MyDataCollector.Item test2 = (oldDataRow[localDataColumn.ColumnName] as MyDataCollector.Item);
                        if (MyDataCollectorClass.oldDataTable.Columns[localDataColumn.ColumnName] != null)
                        {
                            //use .Equals() instead of != because otherwise you get a reference and not a value comparison
                            if (!(localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).textValue.Equals((oldDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).textValue))
                            {
                                if ((localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item) != null)
                                {
                                    //If you changed the value in New Value, then put the old New Value in Old Value
                                    //No, keep the Old Value as in the old csv file, so if you change back it is not red.
                                    //if (localDataColumn.ColumnName == "New Value")
                                    //{
                                    //    (localDataRow["Old Value"] as MyDataCollector.Item).textValue = String.Copy((oldDataRow["New Value"] as MyDataCollector.Item).textValue);
                                    //}
                                    //(localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).SetChanged();
                                    bool WasMyChanged = (localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).IsMyChanged;
                                    SetColour(localDataRow, localDataColumn.ColumnName, WasMyChanged);
                                    someChange = true;
                                }
                            }
                            else
                            {
                                if ((localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item) != null)
                                {
                                    (localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).SetSame();
                                }
                            }
                        }
                        else
                        {
                            //a column is missing in oldDataTable, but that doesn't matter, you don't have to add it, just
                            //run SetChanged() on Item in this cell in localDataTable 
                            if ((localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item) == null)
                            {
                                localDataRow[localDataColumn.ColumnName] = new MyDataCollector.Item("");
                                someChange = true;
                            }
                            //(localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).SetChanged();
                            SetColour(localDataRow, localDataColumn.ColumnName, false);
                        }
                    }
                }
                else
                {
                    // a row is missing
                    foreach (DataColumn localDataColumn in MyDataCollectorClass.localDataTable.Columns)
                    {
                        if ((localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item) == null)
                        {
                            localDataRow[localDataColumn.ColumnName] = new MyDataCollector.Item("");
                        }
                        //(localDataRow[localDataColumn.ColumnName] as MyDataCollector.Item).SetChanged();
                        SetColour(localDataRow, localDataColumn.ColumnName, false);
                    }
                    someChange = true;
                }
            }

            #endregion
            #region oldCompare
            //if (MyDataCollectorClass.sharedFile != null) // && !MyDataCollectorClass.inputFile.Contains("History"))
            //{
            //    if (MyDataCollectorClass.oldDataTable.Rows.Count < 1)
            //    {
            //        return;
            //    }
            //    DataRow drc = MyDataCollectorClass.oldDataTable.Rows[0];
            //    for (int i = 0; i < MyDataCollectorClass.localDataTable.Rows.Count; i++)
            //    {
            //        //often oldDataTable has less rows then localDataTable
            //        //all extra rows in localDataTable should be red, so add strange strings
            //        //but why does this happen if somebody else changes the csv file?
            //        //is localDataTable not updated then?!!! 
            //        //note that Rows.Count is always one more than available Rows[i] because
            //        //you start at 0. Therefore check if i < not i <=

            //        if (i < MyDataCollectorClass.oldDataTable.Rows.Count)
            //        {
            //            drc = MyDataCollectorClass.oldDataTable.Rows[i];
            //        }
            //        else
            //        {
            //            drc = MyDataCollectorClass.oldDataTable.NewRow();
            //            for (int k = 0; k < MyDataCollectorClass.oldDataTable.Columns.Count; k++)
            //            {
            //                //if (MyDataCollectorClass.oldDataTable.Columns[k].ColumnName=="Images")
            //                //{
            //                //    drc[k] = new List<MyImage>();
            //                //}
            //                //else
            //                //{
            //                drc[k] = new Item("@#$%!");
            //                //}
            //                //don't add the row to the table because then you get trouble with primarykey
            //            }
            //        }
            //        DataRow dr = MyDataCollectorClass.localDataTable.Rows[i];
            //        for (int j = 0; j < MyDataCollectorClass.localDataTable.Columns.Count; j++)
            //        {
            //            string cn = MyDataCollectorClass.localDataTable.Columns[j].ColumnName;
            //            //if pCOLLECT adds an attribute the column does not exist yet in oldDataTable
            //            if (!MyDataCollectorClass.oldDataTable.Columns.Contains(cn))
            //            {
            //                MyDataCollectorClass.oldDataTable.Columns.Add(cn, typeof(Item));
            //            }
            //            if (!Object.Equals(drc[cn], dr[cn]))
            //            {
            //                //Item x = new Item(dr[cn].ToString());
            //                //x.IsChanged = true;
            //                //no idea why this can happen...
            //                if (dr[cn] as MyDataCollector.Item == null)
            //                {
            //                    dr[cn] = new Item("");
            //                }
            //                //write the New Value to the Old Value. 
            //                if (cn == "New Value")
            //                {
            //                    if (drc[cn] as MyDataCollector.Item.TextValue != "@#$%!")
            //                    {
            //                        dr["Old Value"] = drc[cn];
            //                    }
            //                }
            //                (dr[cn] as MyDataCollector.Item).SetChanged();
            //            }
            //            else
            //            {
            //                if (dr[cn] as MyDataCollector.Item == null)
            //                {
            //                    dr[cn] = new Item("");
            //                }
            //                (dr[cn] as MyDataCollector.Item).SetSame();
            //            }
            //        }
            //    }
            //    myPropDataTable = MyDataCollectorClass.localDataTable;
            #endregion
            //if (someChange)
            //{
            MyPropDataTable = MyDataCollectorClass.localDataTable.Copy();
            //in order to avoid reloading the csv files next time if it was not me that made the changes
            MyDataCollectorClass.extShare = false;
            //}
        }
        private void SetColour(DataRow dr, String dcn, bool WasMyChanged)
        {
            //if this row contains my own parameter data then set the background to green
            //except if the cell is Obstruction (you don't obstruct your own value - just change it)
            //except the cell Comments, because comments to my parameters can be made by others!!!(later)
            if ((MyDataCollectorClass.extShare || MyDataCollectorClass.firstRun || !WasMyChanged) && dcn.ToString().Equals("Comments"))
            {
                (dr[dcn] as MyDataCollector.Item).SetChanged();
                return;
                //MyDataCollectorClass.extShare = false; //no otherwise next rows will nog be red
            }
            if (dcn.ToString().Equals("Date") || dcn.ToString().Equals("Author"))
            {
                return;
            }
            if (dr["Owner"].ToString().Equals(MyDataCollectorClass.userName))
            {
                if (dcn.Equals("Obstruction"))
                {
                    (dr[dcn] as MyDataCollector.Item).SetChanged();
                }
                else
                {
                    (dr[dcn] as MyDataCollector.Item).SetMyChanged();
                }
            }
            else
            {
                if (dcn.Equals("Obstruction"))
                {
                    (dr[dcn] as MyDataCollector.Item).SetMyChanged();
                }
                else
                {
                    (dr[dcn] as MyDataCollector.Item).SetChanged();
                }
            }
        }
        #endregion
    }
}
