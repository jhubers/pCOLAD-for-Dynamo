using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyDataCollector
{
    public static class MyDataCollectorClass
    {
        //[IsVisibleInDynamoLibrary(false)]
        public static List<object> pSHAREinputs(List<string> _Ninputs, string _IfilePath, string _LfilePath, string _owner)
        {
            System.Windows.MessageBox.Show("The file path = " + _IfilePath);
            List<object> _pSHAREinputs = new List<object> { _Ninputs, _IfilePath, _LfilePath, _owner };
            return _pSHAREinputs;
        }
    }
}

