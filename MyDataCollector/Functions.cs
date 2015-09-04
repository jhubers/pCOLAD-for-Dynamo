﻿using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

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

        public static DataTable ListToTable(List<string> Ls)
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
                    returnTable.Columns.Add("Accepted", typeof(bool));
                    foreach (string word in words)
                    {
                        //add a column for every header with (name, text)
                        string newWord = word;
                        //change the pCOLLECT "Value" column into "New Value"
                        if (word == "Value") { newWord = "New Value"; }
                        returnTable.Columns.Add(newWord, typeof(string));
                    }
                }
                else
                {
                    //add a row to the datatable
                    DataRow row = returnTable.NewRow();
                    int x = 1;
                    foreach (var word in words)
                    {
                        row[x] = word;
                        if (returnTable.Columns.IndexOf("Obstruction") == x)
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
                    object value = row["Accepted"];
                    if (value == DBNull.Value)
                    {
                        row["Accepted"] = true;//make sure that in any case this field is not empty
                    }
                    returnTable.Rows.Add(row);
                }
                i += 1;

            }
            //set the primary key of the table so you can easily merge tables. The key is an array of columns
            //but we use only the column with the Parameter name, which is the second displayed
            //but remember that you hide the "Accepted" column.
            returnTable.PrimaryKey = new DataColumn[] { returnTable.Columns["Parameter"] };
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
    }
}