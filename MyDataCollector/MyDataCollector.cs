using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyDataCollector
{
    public static class MyDataCollectorClass
    {
        //[IsVisibleInDynamoLibrary(false)]
        public static List<string> pSHAREinputs(List<string> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            //System.Windows.MessageBox.Show("The file path = " + _IfilePath);
            //string convertList = _Ninputs.Concat();
            //List<object> _pSHAREinputs = new List<object> { convertList, _IfilePath, _LfilePath, _owner };

            //The inputs of the pCOLLECTs must be added to the content of the csv file.
            //Make an output of pSHARE consisting of a list of alternating parameter names and new values.
            //Unless a value is obstructed. Then add "Obstructed" as new value.


            List<string> _pSHAREinputs = new List<string>();
            foreach (string item in _Ninputs)
            {
               
                _pSHAREinputs.Add(item);
            }
            return _pSHAREinputs;
        }
        //public static string Concat(this IEnumerable<string> source)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (string s in source)
        //    {
        //        sb.Append(s + Environment.NewLine);
        //    }
        //    return sb.ToString();
        //}

    }
}

