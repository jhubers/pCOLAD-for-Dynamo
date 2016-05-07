using System.Collections.Generic;
using System.Windows;

using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;

using ProtoCore.AST.AssociativeAST;
using Dynamo.Wpf;


namespace pCOLADnamespace
{
    /// <summary>

    /// Dynamo uses the MVVM model of programming, 
    /// in which the UI is data-bound to the view model, which
    /// exposes data from the underlying model. Custom UI nodes 
    /// are a hybrid because NodeModel objects already have an
    /// associated NodeViewModel which you should never need to
    /// edit. So here we will create a data binding between 
    /// properties on our class and our custom UI.
    /// 
    /// </summary>
    /// 
    // The NodeName attribute is what will display on 
    // top of the node in Dynamo
    [NodeName("pCOLLECT")]

    // The NodeCategory attribute determines how your
    // node will be organized in the library. You can
    // specify your own category or use one of the 
    // built-ins provided in BuiltInNodeCategories.
    [NodeCategory("pCOLAD")]

    // The description will display in the tooltip
    // and in the help window for the node.
    [NodeDescription("Collects parameters and their attributes for pSHARE.")]

    [IsDesignScriptCompatible]
    public class pCOLLECT : NodeModel
    {
        #region private members

        //private string message;
        //private double awesome;

        #endregion

        #region properties

        /// <summary>
        /// A value that will be bound to our
        /// custom UI's awesome slider.
        /// </summary>
        //public double Awesome
        //{
        //    get { return awesome; }
        //    set
        //    {
        //        awesome = value;
        //        RaisePropertyChanged("Awesome");

        //        RequiresRecalc = true;
        //    }
        //}

        /// <summary>
        /// A message that will appear on the button
        /// on our node.
        /// </summary>
        //public string Message
        //{
        //    get { return message; }
        //    set
        //    {
        //        message = value;

        //        // Raise a property changed notification
        //        // to alert the UI that an element needs
        //        // an update.
        //        RaisePropertyChanged("NodeMessage");
        //    }
        //}

        /// <summary>
        /// DelegateCommand objects allow you to bind
        /// UI interaction to methods on your data context.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        //public DelegateCommand MessageCommand { get; set; }

        #endregion

        #region constructor

        /// <summary>
        /// The constructor for a NodeModel is used to create
        /// the input and output ports and specify the argument
        /// lacing.
        /// </summary>
        /// <param name="workspace"></param>
        public pCOLLECT()
        {
            // When you create a UI node, you need to do the
            // work of setting up the ports yourself. To do this,
            // you can populate the InPortData and the OutPortData
            // collections with PortData objects describing your ports.
            InPortData.Add(new PortData("P", "Parameter name as string."));
            InPortData.Add(new PortData("V", "Value as string."));
            InPortData.Add(new PortData("I", "Importance as string."));
            InPortData.Add(new PortData("C", "Comment as string."));
            InPortData.Add(new PortData("O", "Owner as string."));
            // Nodes can have an arbitrary number of inputs and outputs.
            // If you want more ports, just create more PortData objects.
            OutPortData.Add(new PortData("N", "List of strings."));
            //OutPortData.Add(new PortData("some awesome", "A result."));

            // This call is required to ensure that your ports are
            // properly created.
            RegisterAllPorts();

            // The arugment lacing is the way in which Dynamo handles
            // inputs of lists. If you don't want your node to
            // support argument lacing, you can set this to LacingStrategy.Disabled.
            //ArgumentLacing = LacingStrategy.CrossProduct;
            ArgumentLacing = LacingStrategy.Disabled;

            // We create a DelegateCommand object which will be 
            // bound to our button in our custom UI. Clicking the button 
            // will call the ShowMessage method.
            //MessageCommand = new DelegateCommand(ShowMessage, CanShowMessage);

            // Setting our property here will trigger a 
            // property change notification and the UI 
            // will be updated to reflect the new value.
            //Message = "Say 'Hello Dynamo!'";

            //Awesome = 1;
        }

        #endregion
        
        #region public methods

        /// <summary>
        /// If this method is not overriden, Dynamo will, by default
        /// pass data through this node. But we wouldn't be here if
        /// we just wanted to pass data through the node, so let's 
        /// try using the data.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // When you create your own UI node you are responsible
            // for generating the abstract syntax tree (AST) nodes which
            // specify what methods are called, or how your data is passed
            // when execution occurs.

            // WARNING!!!
            // Do not throw an exception during AST creation. If you
            // need to convey a failure of this node, then use
            // AstFactory.BuildNullNode to pass out null.

            // Using the AstFactory class, we can build AstNode objects
            // that assign doubles, assign function calls, build expression lists, etc.
            List<AssociativeNode> pCOLLECToutputList = new List<AssociativeNode>();
            var semiColon = AstFactory.BuildStringNode(";");
            foreach (AssociativeNode InputItem in inputAstNodes)
            {
                pCOLLECToutputList.Add(InputItem);
                pCOLLECToutputList.Add(semiColon);
            }
            //remove the last semiColon
            if (pCOLLECToutputList.Count > 0)
            {
                pCOLLECToutputList.RemoveAt(pCOLLECToutputList.Count - 1);
            }
            //var testing = Inputs;
            //foreach (KeyValuePair<int,System.Tuple<int,NodeModel>> InputItem in Inputs)
            //{
            //    var ttt = InputItem.Value.Item2;
            //}
            //var test4 = TryGetInput(0, out System.Tuple < 0, NodeModel);
            //var test3 = GetValue(0);
            //string test = pCOLLECToutputList[0];
            //also gives a temp and large number.........
            var test2 = AstFactory.BuildStringNode(pCOLLECToutputList[0].ToString()).value;
            var funcNode = AstFactory.BuildFunctionCall("%add", pCOLLECToutputList);
            return new[]
            {
                // In these assignments, GetAstIdentifierForOutputIndex finds 
                // the unique identifier which represents an output on this node
                // and 'assigns' that variable the expression that you create.
                
                //// For the first node, we'll just pass through the 
                //// input provided to this node.
                //AstFactory.BuildAssignment(
                //    GetAstIdentifierForOutputIndex(0), AstFactory.BuildExprList(inputAstNodes)),
                // we output the headers and on next line the values seperated by ';'

                //AstFactory.BuildAssignment(
                //    GetAstIdentifierForOutputIndex(0), AstFactory.BuildExprList(pCOLLECToutputList)),
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)

                
                //// For the second node, we'll build a double node that 
                //// passes along our value for awesome.
                //AstFactory.BuildAssignment(
                //    GetAstIdentifierForOutputIndex(1),
                //    AstFactory.BuildDoubleNode(awesome))
            };
        }
    /// <summary>
    ///     View customizer for HelloDynamo Node Model.
    /// </summary>
    public class pCOLLECTNodeViewCustomization : INodeViewCustomization<pCOLLECT>
    {

    }
    /// <summary>
    /// At run-time, this method is called during the node 
    /// creation. Here you can create custom UI elements and
    /// add them to the node view, but we recommend designing
    /// your UI declaratively using xaml, and binding it to
    /// properties on this node as the DataContext.
    /// </summary>
    /// <param name="model">The NodeModel representing the node's core logic.</param>
    /// <param name="nodeView">The NodeView representing the node in the graph.</param>

        public void CustomizeView(dynNodeView view)
        {
            // The view variable is a reference to the node's view.
            // In the middle of the node is a grid called the InputGrid.
            // We reccommend putting your custom UI in this grid, as it has
            // been designed for this purpose.

            // Create an instance of our custom UI class (defined in xaml),
            // and put it into the input grid.
            var _pCOLLECTcontrol = new pCOLLECTcontrol();
            view.inputGrid.Children.Add(_pCOLLECTcontrol);

            // Set the data context for our control to be this class.
            // Properties in this class which are data bound will raise 
            // property change notifications which will update the UI.
            _pCOLLECTcontrol.DataContext = this;
        }

        #endregion

        #region command methods

        //private bool CanShowMessage(object obj)
        //{
        //    // I can't think of any reason you wouldn't want to say Hello Dynamo!
        //    // so I'll just return true.
        //    return true;
        //}

        //private void ShowMessage(object obj)
        //{
        //    MessageBox.Show("Hello Dynamo!");
        //}

        #endregion
    }

}
