using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NBG_Helper
{
    class General
    {

        public static List<Category> categories;
        public static List<AutoCategorization> AutoCategorization;
        public static string CategoriesXML = Application.StartupPath + "\\categories.xml";
        public static string AutoCategorizationXML = Application.StartupPath + "\\autocategorization.xml";


        #region " CLIPBOARD OPERATIONS"

        public static void Copy2Clipboard(string val)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetDataObject(val, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Application.ProductName);
            }
        }

        public static string GetFromClipboard()
        {
            try
            {
                return Clipboard.GetText().Trim();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Application.ProductName);
                return "";
            }
        }

        #endregion

        public static DataTable ImportDelimited(string data, string delimiter, bool first_is_column_names, out int errors)
        {
            errors = 0;

            List<string> lines = Regex.Split(data, "\r\n").ToList();

            if (lines.Count == 0)
                return null;

            DataTable dt = new DataTable();
            bool cols_set = false;
            string line2;
            //int errors = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (!cols_set)
                {
                    //split the first line to create columns to datatable!
                    string[] columns = line.Split(Convert.ToChar(delimiter));// Regex.Split(line, "|");


                    for (int i = 0; i < columns.Count(); i++)
                    {
                        if (first_is_column_names)
                            dt.Columns.Add(columns[i].Replace("\"", ""));
                        else
                            dt.Columns.Add("no" + i.ToString());
                    }

                    if (first_is_column_names)
                    {
                        cols_set = true;
                        continue;
                    }
                }

                cols_set = true;


                line2 = line.Replace("\"", "");
                string[] rows = line2.Split(Convert.ToChar(delimiter));//Regex.Split(line, delimiter);

                try
                {
                    dt.Rows.Add(rows);
                }
                catch (Exception x)
                {
                    errors++;
                }


            }


            return dt;
        }
    }
}
