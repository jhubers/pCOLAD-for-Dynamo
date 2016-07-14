﻿using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Functions
    {
        public static List<string> buildCommonMultiple(List<List<string>> listOfListOfStrings)
        {
            // builds the common multiple of parameter formats of several pCOLLECT outputs
            List<string> arFormat;
            List<string> arUnion = new List<string>();
            foreach (List<string> ls in listOfListOfStrings)
            {
                //the first line in the list ls consists of the headers
                arFormat = ls[0].Split(new Char[] { ';' }).ToList();
                arUnion = arUnion.Union(arFormat).ToList();
            }
            return arUnion;
        }
        public static List<string> reformatList(List<string> LS, List<string> format)
        {
            List<string> returnList = new List<string>();
            //do something with the returnList
            return returnList;
        }
        public static DataTable ListToTable(List<string> Ls, string csvS)
        {
            //this function uses the first line of the list as ; seperated headers
            //the next lines are the ; seperated values
            DataTable returnTable = new DataTable();
            int i = 0;
            foreach (string s in Ls)
            {
                string[] words = s.Split(new Char[] { ';' });
                if (i == 0) //this contains the headers. Also might be used to create properties for Parameter class.
                {
                    // add a checbox column for easy setting obstruction field
                    //returnTable.Columns.Add("Accepted", typeof(bool));
                    foreach (string word in words)
                    {
                        //add a column for every header with (name, content)
                        string newWord = word;
                        ////if you add the images column you have to make it of type List<myImage>
                        ////because we want a list of imagepaths there
                        //if (word == "Images")
                        //{
                        //    returnTable.Columns.Add(newWord, typeof(List<MyImage>));
                        //}
                        //else
                        //{
                        //change the pCOLLECT "Value" column into "New Value"
                        if (word == "Value") { newWord = "New Value"; }
                        returnTable.Columns.Add(newWord, typeof(Item));
                        //}
                    }
                }
                else
                {
                    //add a row to the datatable
                    DataRow row = returnTable.NewRow();
                    int x = 0;
                    foreach (var word in words)
                    {
                        //for some reason if you connect same input twice, you get extra empty words...
                        //also if you add a parameter attribute in pCOLLECT and thus a column in a later
                        //share, then returnTable doesn't have enough columns. Since you start at 0
                        //when x is the number of columns you have to add a column
                        if (x == returnTable.Columns.Count)
                        {
                            returnTable.Columns.Add(word, typeof(Item));
                        }
                        if (x <= returnTable.Columns.Count)
                        {

                            //in case of Images column set the ImageList prop of Item to a new List<MyImage> 
                            //else new Item
                            if (returnTable.Columns[x].ColumnName == "Images")
                            {
                                //fill a list with image paths which might be seperated by \n
                                List<string> lis = new List<string>(word.Split(new string[] { "\n" }, StringSplitOptions.None));
                                List<MyImage> li = new List<MyImage>();
                                foreach (string imp in lis)
                                {
                                    MyImage it = new MyImage(imp);
                                    li.Add(it);
                                }
                                Item ni = new Item(word);
                                ni.imageFileNameList = lis;
                                ni.ImageList = li;
                                row[x] = ni;
                            }
                            else
                            {
                                row[x] = new Item(word);
                            }
                        }
                        #region old
                        //if (returnTable.Columns.IndexOf("Obstruction") == x)
                        //{
                        //if (word == "")
                        //{
                        //    row["Accepted"] = true;
                        //}
                        //else
                        //{
                        //    row["Accepted"] = false;
                        //}
                        //}

                        #endregion
                        x += 1;
                    }
                    //object value = row["Accepted"];
                    //if (value == DBNull.Value)
                    //{
                    //    row["Accepted"] = true;//make sure that in any case this field is not empty
                    //}
                    returnTable.Rows.Add(row);
                }
                i += 1;

            }
            //set the primary key of the table so you can easily merge tables. The key is an array of columns
            //but we use only the column with the Parameter name
            //don't use primary key for History.csv file
            if (!Path.GetFileName(csvS).Equals("History.csv"))
            {
                returnTable.PrimaryKey = new DataColumn[] { returnTable.Columns["Parameter"] };
            }
            return returnTable;
        }
        public static DataTable MergeAll(IList<DataTable> tables, String primaryKeyColumn)
        {
            //you can call this function as follows:
            //var tables = new[] { tblA, tblB, tblC };
            //DataTable TblUnion = tables.MergeAll("c1");//where "c1" is the name of the primaryKedyColumn
            if (!tables.Any())
                throw new ArgumentException("Tables must not be empty", "tables");
            if (primaryKeyColumn != null)
                foreach (DataTable t in tables)
                    if (!t.Columns.Contains(primaryKeyColumn))
                        throw new ArgumentException("All tables must have the specified primarykey column " + primaryKeyColumn, "primaryKeyColumn");

            if (tables.Count == 1)
                return tables[0];

            DataTable table = new DataTable("TblUnion");

            //table.BeginLoadData(); // Turns off notifications, index maintenance, and constraints while loading data
            foreach (DataTable t in tables)
            {
                DataTable toMerge = t;
                table.Merge(toMerge, false, MissingSchemaAction.Add); // same as table.Merge(t, false, MissingSchemaAction.Add);
            }
            //table.EndLoadData();

            if (primaryKeyColumn != null)
            {
                // since we might have no real primary keys defined, the rows now might have repeating fields
                // so now we're going to "join" these rows ...
                var pkGroups = table.AsEnumerable()//this needs a reference to System.Data.DataSetExtensions.dll
                    .GroupBy(r => r[primaryKeyColumn]);
                var dupGroups = pkGroups.Where(g => g.Count() > 1);
                foreach (var grpDup in dupGroups)
                {
                    // use first row and modify it
                    DataRow firstRow = grpDup.First();
                    foreach (DataColumn c in table.Columns)
                    {
                        if (firstRow.IsNull(c))
                        {
                            DataRow firstNotNullRow = grpDup.Skip(1).FirstOrDefault(r => !r.IsNull(c));
                            if (firstNotNullRow != null)
                                firstRow[c] = firstNotNullRow[c];
                        }
                    }
                    // remove all but first row
                    var rowsToRemove = grpDup.Skip(1);
                    foreach (DataRow rowToRemove in rowsToRemove)
                        table.Rows.Remove(rowToRemove);
                }
            }

            return table;
        }
        public static string ToCSV(DataTable tbl, string tblName)
        {
            StringBuilder strb = new StringBuilder();

            //column headers
            strb.AppendLine(tbl.Columns.Cast<DataColumn>().Aggregate(
                (object x, object y) => x + ";" + y).ToString());

            //rows But have to create multiline string if several lines in one cell
            //make difference if myPropDataTable is used or historyDataTable
            foreach (DataRow dr in tbl.Rows)
            {
                string S = "";
                string columnType = "";
                foreach (var i in dr.ItemArray)
                {
                    columnType = (i.GetType().ToString());
                    //columnType = (i.GetType() == typeof(string)) ? "string" : "";//default
                    #region replace with Switch
                    //if (i.GetType() == typeof(Item))
                    //{
                    //    Item y = (Item)i;
                    //    if (y.textValue.Contains("\n"))//it does when saving the History file
                    //    {

                    //        S = y.textValue.Replace("\n", Environment.NewLine);
                    //        S = "\"" + S + "\"";
                    //    }
                    //    else
                    //    {
                    //        S += y.textValue;
                    //    }
                    //}
                    //else
                    //{
                    //    S += i;
                    //}

                    #endregion
                    switch (columnType)
                    {
                        case "MyDataCollector.Item":
                            Item y = (Item)i;
                            S += y.textValue;
                            break;
                        case "System.Collections.Generic.List`1[System.String]":
                            //only if historyDataTable is passed
                            if (tblName == "historyDataTable")
                            {
                                foreach (string s in (List<string>)i)
                                    if (s.Length > 0)
                                    {
                                        {
                                            S += s + Environment.NewLine;
                                        }
                                    }
                                //remove the last new line
                                if (S.Length > 1)
                                {
                                    S = S.Remove(S.Length - 2);
                                }
                                //build the multiline for csv
                                S = "\"" + S + "\"";
                            }
                            break;

                        default:
                            S += i;
                            break;
                    }
                    S += ";";
                }
                //remove last ;
                S = S.Remove(S.Length - 1);
                strb.AppendLine(S);
            }
            //tbl.AsEnumerable().Select(s => strb.AppendLine(
            //    s.ItemArray.Aggregate((x, y) => x + ";" + y).ToString())).ToList();
            return strb.ToString().TrimEnd();
        }
        public static void FileExist(string filename)
        {
            if (!File.Exists(filename))
            {
                MyDataCollectorClass.dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBoxResult result = MessageBox.Show(MyDataCollectorClass.dv, "We couldn't find the file: " + filename + ". Would you like me to create it?", "pCOLAD", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            File.WriteAllText(filename, "Images;Comments;Parameter;New Value;Obstruction;Old Value;Owner;Importance;Date;Author");
                            return;
                        case MessageBoxResult.No:
                            MessageBox.Show(MyDataCollectorClass.dv, "Please connect the right file path and run the application again...", "pCOLAD");
                            return;
                    }
                }));
            }
        }

        public static List<string> CSVtoList(string filename)
        {
            List<string> csvList = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line = "";
                    while (sr.Peek() >= 0)
                    {
                        line = ReadNextMultiline(sr);
                        csvList.Add(line);
                    }
                    sr.Dispose();
                    return csvList;
                }
            }
            catch (FileNotFoundException)
            {
                MyDataCollectorClass.dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBox.Show(MyDataCollectorClass.dv, "We couldn't find the file: " + filename + ". Are you sure it exists?");
                }));
            }
            catch (DirectoryNotFoundException)
            {
                MyDataCollectorClass.dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBox.Show(MyDataCollectorClass.dv, "We couldn't find the file. Are you sure the directory exists?");
                }));
                return csvList;
            }
            catch (Exception e)
            {
                MyDataCollectorClass.dv.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    MessageBox.Show(string.Format("We found a problem: {0}", e));//instance not set to a etc.
                }));
                return csvList;
            }
            return csvList;
        }
        public static List<string> imagePaths(string paramName)
        {
            List<string> returnList = new List<string>();
            string directory = MyDataCollectorClass.sharedFile.Remove(MyDataCollectorClass.sharedFile.LastIndexOf("\\") + 1);
            directory += paramName;
            if (!Directory.Exists(directory))
            {
                //returnList.Add("");
                return returnList;
            }
            foreach (string myFile in Directory.GetFiles(directory))
            {
                returnList.Add(myFile);
            }
            return returnList;
        }
        public static string ReadNextMultiline(StreamReader mlReader)
        {
            bool MultilineDetected;
            string res = "", mLine = "";
            do
            {
                MultilineDetected = false;
                mLine = mlReader.ReadLine();
                res = String.Concat(
                                        res,
                                        (res.Length > 0 ? "\n" : ""),    // add a return where there was a linebreak.
                                        mLine);
                string[] broken = res.Split(';');
                // if the RES features unfinished multiliner, then the LAST element will contain exactly 1 " symbol:
                if ((broken[broken.Length - 1].IndexOf('\"') >= 0) &&               // there's some " symbol
                    (broken[broken.Length - 1].IndexOf('\"') == broken[broken.Length - 1].LastIndexOf('\"'))    // there's exactly 1 " on that row.
                   )
                {
                    MultilineDetected = true;
                }
            } while (MultilineDetected);
            //get rid of quotes
            return res.Replace("\"", "");
        }
        public static List<string> GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            if (Directory.Exists(searchFolder))
            {
                var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var filter in filters)
                {
                    filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
                }
            }
            return filesFound;
        }
        public static MyImage dummyFunction()
        {
            //in case of Grasshopper next line appFolderPath = C:\Users\jhubers\AppData\Roaming\Grasshopper\Libraries
            string appFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = appFolderPath + "\\pCOLADdummy.bmp";
            if (!File.Exists(path))
            {
                Bitmap b = MyDataCollector.Properties.Resources.pCOLADdummy;
                b.Save(path);
            }
            return new MyImage(path);
        }
        public static string ConvertToString(object obj)
        {
            if (obj !=null)
            {
                return obj.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
