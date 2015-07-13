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
//just te see if GitHub updates correctly


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
        // later replace with an input
        string inputFile = "D:\\Temp\\test2.csv";
        string userName = "Hans";
        DataTable myDataTable;

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
                DataRow dr = myDataTable.Rows[_rowIndex];
                //also change the value in the hidden column "Accepted"
                dr["Accepted"] = value;
                string cellContent = dr["Obstruction"].ToString();
                if (_cellInfo != null && !_isChecked) //add the userName
                {
                    if (cellContent == "")
                    {
                        dr["Obstruction"] = userName;
                    }
                    else
                    {
                        dr["Obstruction"] += "," + userName;
                    }
                }
                else
                {
                    // remove username from the cell
                    cellContent = cellContent.Replace(userName, "");
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
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            Assembly myDataCollectorLoad = Assembly.LoadFrom("C:\\Program Files\\Dynamo 0.8\\nodes\\MyDataCollector.dll");
            System.Type myDataCollectorType = myDataCollectorLoad.GetType("MyDataCollectorClass");
            //myDataCollectorType.GetMethod("pSHAREinputs");
            object myDataCollectorInstance = Activator.CreateInstance(myDataCollectorType);
            //myDataCollectorInstance.GetType("MyDataCollectorClass").GetMethod("pSHAREinputs");
            MethodInfo myStatic = myDataCollectorType.GetMethod("pSHAREinputs");

            var t = new Func<List<string>, string, string, string, List<string>>(MyDataCollector.MyDataCollectorClass.pSHAREinputs);
            //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
            var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);
            //To be done: take out only the parameter names and on the next line the value

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)};
        }
        ///// <summary>
        ///// the PropertyChange event is used to update the binding to CSV display
        ///// but it turns out that it was inherited by the NodeModel class
        ///// and you can simply use the RaisPropertyChanged method
        ///// </summary>

        /// <summary>
        /// opens the csv file, turns it into a dataTable, and shows the CSV display
        /// </summary>
        public void openCSV()
        {
            //make sure that the table doesn't exist. Otherwise just show it.
            //however!!!!! if through the control it was changed...
            if (myDataTable == null)
            {
                myDataTable = new DataTable();
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
                        string[] words = line.Split(new Char[] { ';' });
                        if (i == 0) //this contains the headers. Also might be used to create properties for Parameter class.
                        {
                            // add a checbox column for easy setting obstruction field
                            myDataTable.Columns.Add("Accepted", typeof(bool));
                            foreach (var word in words)
                            {
                                //add a column for every header with (name, text)
                                myDataTable.Columns.Add(word, typeof(string));
                            }
                        }
                        else
                        {
                            //add a row to the datatable
                            DataRow row = myDataTable.NewRow();
                            int x = 1;
                            foreach (var word in words)
                            {
                                row[x] = word;
                                if (myDataTable.Columns.IndexOf("Obstruction") == x)
                                {
                                    if (word == "")
                                    {
                                        row["Accepted"] = true;
                                    }
                                    else
                                    {
                                        row["Accepted"] = false;
                                    }
                                }
                                x += 1;
                            }
                            myDataTable.Rows.Add(row);
                        }
                        i += 1;
                    }
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
                //make sure that control doesn't exist.
            }
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
                    this.myPropDataTable = myDataTable;
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
                openCSV();
            }
            else
            {
                On = false;
                //close *.csv display
                //myDataTable = null;
                closeCSVControl();
                //in order to not check the last selected row
                //!!!!!!!!!!!!!still not right
                if (_rowIndex == 0)
                {
                    _rowIndex = 1;
                }
                else
                {
                    _rowIndex = 0;
                }
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
