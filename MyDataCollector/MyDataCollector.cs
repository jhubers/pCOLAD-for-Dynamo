using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class MyDataCollectorClass
    {
        //[IsVisibleInDynamoLibrary(false)]
        public static List<List<string>> output;
        public static List<List<string>> pSHAREinputs(List<List<string>> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            //The inputs of the pCOLLECTs must be added to the content of the csv file.
            //So first load the csv file!!!!!!!!!!!!!here



            //Make an output of pSHARE consisting of a list of alternating parameter names and new values.
            //Unless a value is obstructed. Then add "Obstructed" as new value.


            List<List<string>> pSHAREoutputs = new List<List<string>>();
            foreach (List<string> item in _Ninputs)
            {
                pSHAREoutputs.Add(item);
            }
            output = pSHAREoutputs;
            return pSHAREoutputs;
        }
    }
}

