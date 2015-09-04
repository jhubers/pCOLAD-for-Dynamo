//This method is not used by all pCOLAD nodes.
//pCOLLECT uses it to build an outputListProp which is used by pSHARE.
//pSHARE uses it to build an input list from a (list of) PCOLLECT's output(s). NOT
//pPARAM uses it to build an input list of the output of pSHARE
using System.Collections.Generic;
using Dynamo.Models;
using System.Windows;
using ProtoCore.AST.AssociativeAST;
using System.Collections.ObjectModel;
namespace pCOLADnamespace
{
    public static class MakeInputList
    {
        public static List<string> InputList(IDictionary<int, System.Tuple<int, NodeModel>> _Inputs)
        {
            List<string> ReturnList = new List<string>();
            for (int i = 0; i < _Inputs.Count; i++)
            {
                //the Inputs object is a Dictionary< int, System.Tuple<int,NodeModel>>
                System.Tuple<int, NodeModel> item = _Inputs[i];
                NodeModel nm = item.Item2;
                System.Type testType = nm.GetType();
                string typeName = testType.Name;
                switch (typeName)
                {
                    case "CreateList":
                        IDictionary<int, System.Tuple<int, NodeModel>> item1 = nm.InputNodes;
                        List<string> l1 = MakeInputList.InputList(item1); ;
                        ReturnList.AddRange(l1);
                        break;
                    case "pCOLLECT":
                        pCOLLECT pC = (pCOLLECT)item.Item2;
                        List<string> l2 = pC.outputListProp;
                        ReturnList.AddRange(l2);
                        break;
                    case "Filename":
                        //we do nothing with the filename; catch it with the inputAstNodes!!!have to work on this
                        // no have to open the file in code so need the path
                        break;
                    case "CodeBlockNodeModel":
                        // If the Inputs are from pCOLLECT then this is OK
                        // If the Inputs are from pSHARE you should make it recognizable in the ReturnList
                        // so you can make a difference with the inputs from pCOLLECTs!!!
                        // or maybe make a case for the paths?
                        Dynamo.Nodes.CodeBlockNodeModel cbnm = (Dynamo.Nodes.CodeBlockNodeModel)nm;
                        string s = cbnm.Code;
                        if (s.Length > 1 && s[0] == '\"')
                        {
                            //for some reason Dynamo puts "\" and \";" around the string
                            string sCleaned = s.Remove(s.Length - 2).Remove(0, 1);
                            ReturnList.Add(sCleaned);
                        }
                        else
                        {
                            // If it is a number Dynamo puts ; behind it.
                            ReturnList.Add(s.Remove(s.Length - 1));
                        }
                        break;
                    default:
                        MessageBox.Show("Please connect only output of pCOLLECT or List.CreateList to N input of pSHARE.");
                        ReturnList.Clear();
                        break;
                }
                //try
                //{
                //    Dynamo.Nodes.CodeBlockNodeModel itemValue = (Dynamo.Nodes.CodeBlockNodeModel)item.Item2;
                //        string s = itemValue.Code;
                //        if (s.Length > 1 && s[0] == '\"')
                //    {
                //        //for some reason Dynamo puts "\" and \";" around the string
                //        string sCleaned = s.Remove(s.Length - 2).Remove(0, 1);
                //        ReturnList.Add(sCleaned);
                //    }
                //    else
                //    {
                //        // If it is a number Dynamo puts ; behind it.
                //            ReturnList.Add(s.Remove(s.Length-1));
                //    }
                //}
                //catch (System.Exception)
                //{
                //    MessageBox.Show("Please connect only strings or numbers.");
                //    ReturnList.Clear();
                //    return ReturnList;
                //}

            }
            return ReturnList;
        }
    }
}
