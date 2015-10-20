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
using System.Windows.Input;
using Dynamo.Wpf;
using System.Reflection;
using ProtoCore.SyntaxAnalysis;
using MyDataCollector;
using System.Windows.Data;


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
        private string historyFile = "";
        private string oldCSV = "";
        private bool On = false;
        private bool HistoryOn = false;
        private bool CheckAllButton = false;
        private bool UncheckAllButton = false;
        private string _OnOffButton = "";
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
                RaisePropertyChanged("MyPropDataTable");
            }
        }
        public void CSVUpdateHandler(object o, EventArgs e)
        {
            Compare();
            myPropDataTable = MyDataCollectorClass.myDataTable;
            RaisePropertyChanged("MyPropDataTable");
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
                            cellContent = Regex.Replace(cellContent, ",{2,}", ",").Trim(',');
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
                                    dr["Obstruction"]=new Item(cellContent + "," + MyDataCollectorClass.userName);
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
                                dr["Obstruction"] = new Item(cellContent + "," + MyDataCollectorClass.userName);
                            }
                        }
                        else
                        {
                            // remove username from the cell
                            cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                            //remove double and end commas
                            cellContent = Regex.Replace(cellContent, ",{2,}", ",").Trim(',');
                            dr["Obstruction"] = new Item(cellContent.Trim());
                        }
                        //this causes endless loop
                        //MyDataCollectorClass.myDataTable.AcceptChanges();
                        //MyDataCollectorClass.myDataTable = MyPropDataTable;
                        myPropDataTable = MyDataCollectorClass.myDataTable;
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
            var t = new Func<List<List<string>>, string, string, string, List<string>>(MyDataCollectorClass.pSHAREinputs);
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
                    //compare the csv file with the copy
                    Compare();
                    this.MyPropDataTable = MyDataCollectorClass.myDataTable;
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
                    //_CSVControl.DataContext = MyDataCollectorClass.myDataTable;
                    // works a bit better, but why should you set the ItemsSource again!!!


                    //_CSVControl.myXamlTable.ItemsSource = MyDataCollectorClass.myDataTable.DefaultView;
                    //_CSVControl.DataContext = this;
                    this.MyPropDataTable = MyDataCollectorClass.myDataTable;
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
                    //MyDataCollectorClass.myDataTable = MyDataCollectorClass.loadedDataTable.Copy();
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
        private void ShowParams(object obj)
        {
            //switch the On boolean to show or not the *.csv file
            if (On == false)
            {
                //and show the *.csv file if the solution was run
                if (MyPropDataTable!=null)
                {
                On = true;
                ShowCSV();
                }
                else
                {
                    MessageBox.Show("Please hit the Run button first...");
                    //set the button to red again!!!
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
        private bool CanShare(object obj)
        {
            return true;
        }
        private void Share(object obj)
        {
            //MessageBox.Show("Share button is Clicked");
            //write myDataTable to the csv files if something changed
            string csv = Functions.ToCSV(myPropDataTable);
            if (oldCSV == csv)
            {
                MessageBox.Show("Nothing changed with last share ...");
            }
            else
            {
                try
                {
                    File.WriteAllText(MyDataCollectorClass.inputFile, csv);
                    File.WriteAllText(MyDataCollectorClass.inputFileCopy, csv);
                    //apend myDataTable to the history file, if there are changes!!!
                    historyFile = MyDataCollectorClass.inputFile.Remove(MyDataCollectorClass.inputFile.LastIndexOf("\\") + 1) + "History.csv";
                    //check if file exist
                    if (!File.Exists(historyFile))
                    {
                        File.Create(historyFile).Close();
                        File.AppendAllText(historyFile, csv);
                    }
                    else
                    {
                        File.AppendAllText(historyFile, Environment.NewLine + csv);

                    }
                    //close the CSVControl and set the On butto to Off (red)!!! still does not work correctly
                    //think it is not the onoff commeand that should be triggerd but also the Status of the button
                    ShowParams(OnOff);
                    RaisePropertyChanged("OnOff");
                    //RaisePropertyChanged("OnOffButton"); //makes no difference
                    //closeCSVControl();
                    //On = false; //makes no difference
                    oldCSV = csv;
                }
                catch (System.Exception e)
                {
                    MessageBox.Show("Exception source: {0}", e.Source);
                }
                //pSHAREcontrol.myButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));               

            }
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
        private void History(object obj)
        {
            //show History.csv!!!
            //first set myDataTable to the History file by changing the inputFile property.
            if (MyDataCollectorClass.inputFile == null)
            {
                MessageBox.Show("Please connect file path to pSHARE and run the solution ...");
            }
            else
            {
                if (!HistoryOn)//is on when you show the History file
                {
                    string HistoryFile = MyDataCollectorClass.inputFile.Remove(MyDataCollectorClass.inputFile.LastIndexOf("\\") + 1) + "History.csv";
                    if (!File.Exists(HistoryFile))
                    {
                        File.Create(HistoryFile).Close();
                    }
                    MyDataCollectorClass.formPopulate = false;
                    MyDataCollectorClass.inputFile = HistoryFile;
                    MyDataCollectorClass.openCSV();
                    this.MyPropDataTable = MyDataCollectorClass.myDataTable;
                    HistoryOn = true;
                }
                else
                {
                    //MyDataCollectorClass.formPopulate = false;
                    MyDataCollectorClass.inputFile = MyDataCollectorClass.ShareInputFile;
                    //Re-open csv might be avoided if you store myDatTable in loadedDataTable
                    //MyDataCollectorClass.openCSV();
                    //just set myDataTable to a copy of loaded DataTable!!!
                    MyDataCollectorClass.myDataTable = MyDataCollectorClass.loadedDataTable.Copy();
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
            //gaat iets fout...!!!
            UncheckAllButton = true;
            this._isChecked = false;
            this.isChecked = false;
            UncheckAllButton = false;
        }
        #endregion
        private void Compare()
        {
            //compare csv and copy of csv here!!!
            //compare the comment, New Value and importance values of the two tables
            //but not for the History file
            if (MyDataCollectorClass.inputFile != null && !MyDataCollectorClass.inputFile.Contains("History"))
            {
                for (int i = 0; i < MyDataCollectorClass.myDataTable.Rows.Count; i++)
                {
                    DataRow drc = MyDataCollectorClass.copyDataTable.Rows[0];
                    if (i < MyDataCollectorClass.copyDataTable.Rows.Count)
                    {
                        drc = MyDataCollectorClass.copyDataTable.Rows[i];
                    }
                    DataRow dr = MyDataCollectorClass.myDataTable.Rows[i];
                    if (!Object.Equals(drc["Comments"],dr["Comments"]))
                    {
                        (dr["Comments"] as MyDataCollector.Item).SetChanged();
                    }
                    if (!Object.Equals(drc["New Value"], dr["New Value"]))
                    {
                        (dr["New Value"] as MyDataCollector.Item).SetChanged();
                    }
                    if (!Object.Equals(drc["Importance"], dr["Importance"]))
                    {
                        (dr["Importance"] as MyDataCollector.Item).SetChanged();
                    }

                    //Importance = dr["Importance"].ToString();
                    //if (!Importance.Equals(drc["Importance"].ToString()))
                    //{
                    //    newImportance = true;
                    //}
                    //else
                    //{
                    //    newImportance = false;
                    //}
                }
            }

        }
    }
}
