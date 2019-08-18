using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NBG_Helper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if (File.Exists(General.CategoriesXML))
            {
                General.categories = XmlHelper.FromXmlFile<List<Category>>(General.CategoriesXML);
                General.categories.Add(new Category { Title = "" });
                col4.DataSource = General.categories.OrderBy(x => x.Title).Select(x => x.Title).ToList();
            }

            if (File.Exists(General.AutoCategorizationXML))
                General.AutoCategorization = XmlHelper.FromXmlFile<List<AutoCategorization>>(General.AutoCategorizationXML);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            //dg.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            string p = General.GetFromClipboard();

            string pattern = @"Αγορά ποσού(.*?)από(.*?)με χρήση(.*?)\*(.*?)\s(.*?)\s";


            MatchCollection match = Regex.Matches(p, pattern, RegexOptions.IgnoreCase);

            //MatchCollection match = regex.Matches(p,);
            string dat;
            string descr;
            string amount;
            foreach (Match c in match)
            {
                dat = c.Groups[5].Value.Trim();
                descr = c.Groups[2].Value.Replace("ΑΓΟΡΑ ", "").Trim();
                amount = c.Groups[1].Value.Replace("EUR", "").Trim();

                dg.Rows.Add(dat, descr, amount);
                //DataGridViewRow row = (DataGridViewRow)dg.Rows[0].Clone();
                //row.Cells[0].Value = "XYZ";
                //row.Cells[1].Value = 50.2;
                //dg.Rows.Add(row);

                //Console.WriteLine(c.Groups[1].Value);
                //Console.WriteLine(c.Groups[2].Value);
                //Console.WriteLine(c.Groups[3].Value);
                //Console.WriteLine(c.Groups[4].Value);
                //Console.WriteLine(c.Groups[5].Value);
            }


            autofillcategory();


            dg.AutoResizeColumns();
        }

        private void dg_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            bool validClick = (e.RowIndex != -1 && e.ColumnIndex != -1); //Make sure the clicked row/column is valid.
            var datagridview = sender as DataGridView;

            // Check to make sure the cell clicked is the cell containing the combobox 
            if (datagridview.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn && validClick)
            {
                datagridview.BeginEdit(true);
                ((ComboBox)datagridview.EditingControl).DroppedDown = true;
            }
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            frmCategories y = new frmCategories();
            y.ShowDialog();

            col4.DataSource = General.categories.OrderBy(x => x.Title).Select(x => x.Title).ToList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (General.categories == null)
            {
                MessageBox.Show("You must have categories to enter to this form!");
                return;
            }
            frmAutoCategorization y = new frmAutoCategorization();
            y.ShowDialog();
            //col4.DataSource = General.categories.OrderBy(x => x.Title).Select(x => x.Title).ToList();
        }

        private void autofillcategory()
        {
            if (General.AutoCategorization == null)
                return;

            foreach (DataGridViewRow row in dg.Rows)
            {
                string company = row.Cells[1].Value.ToString();

                foreach (var item in General.AutoCategorization)
                {
                    if (company.ToUpper().Contains(item.Description.ToUpper()))
                    {
                        row.Cells[3].Value = item.CategoryTitle;
                    }

                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (dg.RowCount == 0)
                return;

            autofillcategory();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //force cell edit end, so the last edited cell appear @ export
            dg.EndEdit();

            string csv = "Date;Description;Amount;Category;Comment\r\n";

            foreach (DataGridViewRow row in dg.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        if (cell.ColumnIndex == 2)
                            csv += "-" + cell.Value.ToString().Replace(",", ".").Replace(";", ".") + ';';
                        else
                            //Add the Data rows.
                            csv += cell.Value.ToString().Replace(",", ".").Replace(";", ".") + ';';
                    }
                    // break;
                }
                //Add new line.
                csv += "\r\n";
            }

            string location = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            File.WriteAllText(location, csv, new UTF8Encoding(false));

            Process.Start(Application.StartupPath);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dg.RowCount == 0)
                return;

            if (MessageBox.Show("Are you sure?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                dg.Rows.Clear();
        }

        private void dg_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //handle the error
            //when a row has a category selected and user remove it from categories dialog, dg throws error.
        }
    }
}
