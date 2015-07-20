using System.Collections.Generic;
using System.Windows;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Wpf;
using System;
using Dynamo.Nodes;
using pCOLADnamespace;

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
        private List<string> _outputListProp;
        public List<string> outputListProp
        {
            get { return _outputListProp; }

            set { _outputListProp = value; } 
        }
        
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
            InPortData.Add(new PortData("P", "Parameter name as int."));
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

            // Make a list from the inputs, using the MakeInputList class.
            // In fact not used in pCOLLECT but needed in pSHARE

            List<string> InputList = MakeInputList.InputList(InputNodes);
            _outputListProp = InputList;
            //Dynamo.CustomNodeDefinition pCOLLECTdefinition = (Dynamo.CustomNodeDefinition)InputList;


            //List<string> InputList = new List<string>();
            //for (int i = 0; i < Inputs.Count ; i++)
            //{
            //    var item = Inputs[i];
            //    Dynamo.Nodes.CodeBlockNodeModel itemValue = (Dynamo.Nodes.CodeBlockNodeModel)item.Item2;
            //    string s = itemValue.Code;
            //    //for some reason Dynamo puts "\" and \";" around the string
            //    string sCleaned = s.Remove(s.Length - 2).Remove(0, 1);
            //    InputList.Add(sCleaned);
            //}

            // Using the AstFactory class, we can build AstNode objects
            // that assign doubles, assign function calls, build expression lists, etc.

            //build a new output List<AssociativeNode>consisting of fieldnames seperated by ';' and
            // on next inputAstNodes with ';' added
            List<AssociativeNode> pCOLLECTtempList = new List<AssociativeNode>();
            // the headings should become flexible in future!!!!!!!!!!
            var headings = AstFactory.BuildStringNode("Parameter;Value;Importance;Comments;Owner");

            // tables merge if headings correspond!!!!!!!!!!
            //var headings = AstFactory.BuildStringNode("Comments;Parameter;New Value;Obstruction;Old Value");
            var semiColon = AstFactory.BuildStringNode(";");
            foreach (AssociativeNode InputItem in inputAstNodes)
            {
                List<AssociativeNode> arguments = new List<AssociativeNode>();
                arguments.Add(InputItem);
                arguments.Add(semiColon);
                var funcNode = AstFactory.BuildFunctionCall("%add", arguments);
                //don't add ';' to the last one
                if (inputAstNodes.IndexOf(InputItem) == inputAstNodes.Count - 1)
                {
                    pCOLLECTtempList.Add(InputItem);
                }
                else
                {
                    pCOLLECTtempList.Add(funcNode);
                }
            }
            // now pCOLLECTtempList has the inputs followed by ';'
            // but it should become one string so add the items together
            List<AssociativeNode> pCOLLECToutputList = new List<AssociativeNode>();
            AssociativeNode A = pCOLLECTtempList[0];
            for (int i = 0; i < pCOLLECTtempList.Count - 1; i++)
            {
                List<AssociativeNode> arguments = new List<AssociativeNode>();
                arguments.Add(A);
                arguments.Add(pCOLLECTtempList[i + 1]);
                var funcNode = AstFactory.BuildFunctionCall("%add", arguments);
                A = funcNode;
            }
            pCOLLECToutputList.Add(A);
            pCOLLECToutputList.Insert(0, headings);


            //var test4 = TryGetInput(0, out System.Tuple < 0, NodeModel);
            //var test3 = GetValue(0);
            //string test = pCOLLECTtempList[0];
            //also gives a temp and large number.........
            // var test2 = AstFactory.BuildStringNode(pCOLLECTtempList[0].ToString()).value;
            //var funcNode = AstFactory.BuildFunctionCall("%add", pCOLLECTtempList);
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

                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0), AstFactory.BuildExprList(pCOLLECToutputList)),
                
                
                //AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)

                
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
            /// <summary>
            /// At run-time, this method is called during the node 
            /// creation. Here you can create custom UI elements and
            /// add them to the node view, but we recommend designing
            /// your UI declaratively using xaml, and binding it to
            /// properties on this node as the DataContext.
            /// </summary>
            /// <param name="model">The NodeModel representing the node's core logic.</param>
            /// <param name="nodeView">The NodeView representing the node in the graph.</param>

            public void CustomizeView(pCOLLECT model, NodeView nodeView)
            {
                // The view variable is a reference to the node's view.
                // In the middle of the node is a grid called the InputGrid.
                // We reccommend putting your custom UI in this grid, as it has
                // been designed for this purpose.

                // Create an instance of our custom UI class (defined in xaml),
                // and put it into the input grid.
                var _pCOLLECTcontrol = new pCOLLECTcontrol();
                nodeView.inputGrid.Children.Add(_pCOLLECTcontrol);

                // Set the data context for our control to be this class.
                // Properties in this class which are data bound will raise 
                // property change notifications which will update the UI.
                _pCOLLECTcontrol.DataContext = model;
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
