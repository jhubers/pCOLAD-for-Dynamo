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
using pCOLADnamespace;


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
        bool On = false;
        private string _OnOffButton;
        //string userName = "Hans";
        //DataTable myDataTable;

        /// <summary>
        /// the property pSHARE.myPropDataTable is used as itemsSource for the datagrid
        /// </summary>
        public DataTable myPropDataTable { get; set; }
        private int _rowIndex;
        public int RowIndex
        {
            get { return _rowIndex; }
            set
            {
                _rowIndex = value;
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
        private bool _isChecked;
        /// <summary>
        /// property of pSHARE telling if a row is checked and so a value obstructed
        /// </summary>
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                //OnPropertyChanged("isChecked"); //this sets all checkboxes to checked...
                DataRow dr = MyDataCollectorClass.myDataTable.Rows[_rowIndex];
                //also change the value in the hidden column "Accepted"
                dr["Accepted"] = value;
                string cellContent = dr["Obstruction"].ToString();
                if (_cellInfo != null && !_isChecked) //add the userName
                {
                    if (cellContent == "")
                    {
                        dr["Obstruction"] = MyDataCollectorClass.userName;
                    }
                    else
                    {
                        dr["Obstruction"] += "," + MyDataCollectorClass.userName;
                    }
                }
                else
                {
                    // remove username from the cell
                    cellContent = cellContent.Replace(MyDataCollectorClass.userName, "");
                    //remove double and end commas
                    cellContent = Regex.Replace(cellContent, ",{2,}", ",").Trim(',');
                    dr["Obstruction"] = cellContent.Trim();
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
            }
        }
        /// DelegateCommand objects allow you to bind UI interaction to methods on your data context.
        [IsVisibleInDynamoLibrary(false)]
        public DelegateCommand OnOff { get; set; }
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
            InPortData.Add(new PortData("N", "Input (a List.CreateList) of pCOLLECT output(s)"));
            InPortData.Add(new PortData("I", "Input a FilePath for the shared csv files."));
            InPortData.Add(new PortData("L", "Input a FilePath for the local copy of the csv file."));
            InPortData.Add(new PortData("U", "Input a the user namen (Code Block)."));
            OutPortData.Add(new PortData("O", "Output of parameter name and value on next line; two by two."));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.CrossProduct;
            OnOff = new DelegateCommand(ShowParams, CanShowParams);
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

        //public override AssociativeAstVisitor
        //{
        //    AssociativeAstVisitor visitor;
        //}

        //public override void Accept(AssociativeAstVisitor visitor)
        //{

        //    var t = new Func<List<string>, string, string, string, List<string>>(MyDataCollector.MyDataCollectorClass.pSHAREinputs);
        //    //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
        //    var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);
        //    visitor.Visit(funcNode);
        //    //visitor.VisitThisPointerNode(this);
        //}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //Assembly myDataCollectorLoad = Assembly.LoadFrom("C:\\Program Files\\Dynamo 0.8\\nodes\\MyDataCollector.dll");
            //System.Type myDataCollectorType = myDataCollectorLoad.GetType("MyDataCollectorClass");
            //myDataCollectorType.GetMethod("pSHAREinputs");
            //object myDataCollectorInstance = Activator.CreateInstance(myDataCollectorType);
            //myDataCollectorInstance.GetType("MyDataCollectorClass").GetMethod("pSHAREinputs");
            //MethodInfo myStatic = myDataCollectorType.GetMethod("pSHAREinputs");
            var t = new Func<List<List<string>>, string, string, string, List<List<string>>>(MyDataCollectorClass.pSHAREinputs);
            //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
            var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);
            //List<string> a = new List<string>();
            //string b = string.Empty;
            //string c = string.Empty;
            //string d = string.Empty;
            //MyDataCollector.MyDataCollectorClass.pSHAREinputs(a,b,c,d);

            //ProtoCore.SyntaxAnalysis.AssociativeAstVisitor _visitor;
            //_visitor.DefaultVisit(funcNode);
            //funcNode.Accept(_visitor);



            //Why this only works second run? Because the function is only called during node construction            
            //List<string> test = MyDataCollector.MyDataCollectorClass.output;
            return new[] 
            { 
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)
                //everything below this is not reachable
                //List<string> test = MyDataCollector.MyDataCollectorClass.output
             };
        }

        ///// <summary>
        ///// the PropertyChange event is used to update the binding to CSV display
        ///// but it turns out that it was inherited by the NodeModel class
        ///// and you can simply use the RaisPropertyChanged method
        ///// </summary>

        /// <summary>
        /// opens the csv file, turns it into a dataTable, and shows the CSV display
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
                        w.Activate();
                    }
                }
                if (!isCSVControlOpen)
                {
                    this.myPropDataTable = MyDataCollectorClass.myDataTable;
                    CSVControl _CSVControl = new CSVControl();
                    //bind the datatable to the xaml datagrid
                    _CSVControl.myXamlTable.ItemsSource = this.myPropDataTable.DefaultView;
                    _CSVControl.DataContext = this;
                    _CSVControl.Show();
                }
            }

            catch (System.Exception e)
            {
                MessageBox.Show("Exception source: {0}", e.Source);
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
                On = true;
                //and show the *.csv file
                
                //putting the CSV-file in myDataTable should be done before you build pSHARE, because the parameters
                //should be in its output. But you also have to add the inputs of the pCOLLECTs and with the actual method
                //you get them only at the end of building the pSHARE node. So this is impossible.
                //OR.... we can try to put it in MyDataCollector class. But... I need to access myDataTable when
                // I change some values trough pCOLLECT or when selecting a check box in the display
                // well I guess I can access myDataTable then as property in MyDataCollector!!!!!!!!!!!!
                MyDataCollectorClass.openCSV();// getData = new MyDataCollectorClass();
                //getData.openCSV();
                
                //openCSV();
                //add the parameters from pCOLLECTs -- but in fact all the parameters should be output of pSHARE!!!!!
                //maybe easier to make DataTables of the pCOLLECT outputs, and then merge them together, and then merge
                //with myDataTable that contains the CSV file, and then later checkt the differences
                // with the old copy of the CSV file and display in red the differeces
                //use a function to turn a list of strings consisting of ; seperated values, into a table
                // while the first line contains the header names
                //    //but you can not add the inputs of pCOLLECTS when building the output of pSHARE!!!!!!!!!!
                //    //the inputs are only added after pSHARE is build
                //    // I could maybe build a hidden node?????

                // newParameters consist of lists that consist of a list with 1 line with headers and 1 line with ;-seperated values
                List<List<string>> newParameters = MyDataCollectorClass.output;
                //turn list of list of strings into List of DataTables
                List<DataTable> newParamTables = new List<DataTable>();

                newParamTables.Add(MyDataCollectorClass.myDataTable);
                foreach (List<string> ls in newParameters)
                {
                    DataTable newParamTable = pCOLADnamespace.Functions.ListToTable(ls);
                    newParamTables.Add(newParamTable);
                }
                DataTable TblUnion = pCOLADnamespace.Functions.MergeAll(newParamTables, "Parameter");
                MyDataCollectorClass.myDataTable = TblUnion;
                ShowCSV();
            }
            else
            {
                On = false;
                //close *.csv display
                //myDataTable = null;
                closeCSVControl();
                //in order to not check the last selected row
                //!!!!!!!!!!!!!still not right
                //if (_rowIndex == 0)
                //{
                //    _rowIndex = 1;
                //}
                //else
                //{
                //    _rowIndex = 0;
                //}
            }
        }

        private ICommand _ShareClicked;
        public ICommand ShareClicked
        {
            get
            {
                if (_ShareClicked == null)
                {
                    _ShareClicked = new RelayCommand(
                        param => this.Share(),
                        param => this.CanShare()
                               );
                }
                return _ShareClicked;
            }
        }
        private bool CanShare()
        {
            return true;
        }
        private void Share()
        {
            MessageBox.Show("Share button is Clicked");
        }
        #endregion

    }
}
