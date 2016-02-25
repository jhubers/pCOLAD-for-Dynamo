using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Wpf;
using MyDataCollector;
using System;
using Dynamo.Nodes;
using Dynamo.Graph.Nodes;

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
    [NodeName("pPARAM")]

    // The NodeCategory attribute determines how your
    // node will be organized in the library. You can
    // specify your own category or use one of the 
    // built-ins provided in BuiltInNodeCategories.
    [NodeCategory("pCOLAD")]

    // The description will display in the tooltip
    // and in the help window for the node.
    [NodeDescription("Collects parameters and their values from pSHARE.")]

    [IsDesignScriptCompatible]

    public class pPARAM : NodeModel
    {

        #region constructor

        /// <summary>
        /// The constructor for a NodeModel is used to create
        /// the input and output ports and specify the argument
        /// lacing.
        /// </summary>
        /// <param name="workspace"></param>
        public pPARAM()
        {
            // When you create a UI node, you need to do the
            // work of setting up the ports yourself. To do this,
            // you can populate the InPortData and the OutPortData
            // collections with PortData objects describing your ports.
            InPortData.Add(new PortData("P", "Parameter name as string."));
            InPortData.Add(new PortData("O", "Output of pSHARE."));
            // Nodes can have an arbitrary number of inputs and outputs.
            // If you want more ports, just create more PortData objects.
            OutPortData.Add(new PortData("V", "Value of the P input parameter"));
            //OutPortData.Add(new PortData("some awesome", "A result."));

            // This call is required to ensure that your ports are
            // properly created.
            RegisterAllPorts();

            // The arugment lacing is the way in which Dynamo handles
            // inputs of lists. If you don't want your node to
            // support argument lacing, you can set this to LacingStrategy.Disabled.
            //ArgumentLacing = LacingStrategy.CrossProduct;
            ArgumentLacing = LacingStrategy.Disabled;

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

            // WARNING!
            // Do not throw an exception during AST creation. If you
            // need to convey a failure of this node, then use
            // AstFactory.BuildNullNode to pass out null.

            // Find the parameter 

            var t = new Func<string, List<string>, string>(MyDataCollectorClass.pPARAMoutputs);
            //var t = new Func<List<string>, string, string, string, List<string>>(myStatic);
            var funcNode = AstFactory.BuildFunctionCall(t, inputAstNodes);

            // Using the AstFactory class, we can build AstNode objects
            // that assign doubles, assign function calls, build expression lists, etc.

            return new[]
            {
                // In these assignments, GetAstIdentifierForOutputIndex finds 
                // the unique identifier which represents an output on this node
                // and 'assigns' that variable the expression that you create.
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)             
            };
        }
        /// <summary>
        ///     View customizer for HelloDynamo Node Model.
        /// </summary>
        public class pPARAMNodeViewCustomization : INodeViewCustomization<pPARAM>
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

            public void CustomizeView(pPARAM model, NodeView nodeView)
            {
                // The view variable is a reference to the node's view.
                // In the middle of the node is a grid called the InputGrid.
                // We reccommend putting your custom UI in this grid, as it has
                // been designed for this purpose.

                // Create an instance of our custom UI class (defined in xaml),
                // and put it into the input grid.
                var _pPARAMcontrol = new pPARAMcontrol();
                nodeView.inputGrid.Children.Add(_pPARAMcontrol);

                // Set the data context for our control to be this class.
                // Properties in this class which are data bound will raise 
                // property change notifications which will update the UI.
                _pPARAMcontrol.DataContext = model;
            }
            /// <summary>
            /// Here you can do any cleanup you require if you've assigned callbacks for particular 
            /// UI events on your node.
            /// </summary>
            public void Dispose() { }

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
