using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NBG_Helper
{
    public partial class Form1 : Form
    {
        private DataGridViewCellStyle cell_style;


        public Form1()
        {
            InitializeComponent();

            this.Text = Application.ProductName + " v" + Application.ProductVersion;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            if (File.Exists(General.CategoriesXML))
            {
                General.categories = XmlHelper.FromXmlFile<List<Category>>(General.CategoriesXML);
                General.categories.Add(new Category { Title = "" });
                col4.DataSource = General.categories.OrderBy(x => x.Title).Select(x => x.Title).ToList();
            }

            if (File.Exists(General.AutoCategorizationXML))
                General.AutoCategorization = XmlHelper.FromXmlFile<List<AutoCategorization>>(General.AutoCategorizationXML);


            //nothing else work - http://csharphelper.com/blog/2014/09/set-datagridview-column-styles-in-c/
            cell_style = new DataGridViewCellStyle();
            cell_style.Font = new Font("Consolas", 9.75F);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MatchCollection match;
            string p = General.GetFromClipboard();

            toolStripDropDownButton1.Visible = false;
            //** EXPENSES ** [START]

            //Αγορά ποσού 5,20 EUR από  (defalt = 0)
            string pattern = @"Αγορά ποσού(.*?)από(.*?)με χρήση(.*?)\*(.*?)\s(.*?)\s";

            //Μεταφορά ποσού 150,00 EUR από τον λογ/σμό  (68 = sent money + prepaid recharge)
            string pattern2 = @"Μεταφορά ποσού(.*?)από(.*?)τον(.*?)\*(.*?)\s(.*?)\s";

            //Ανάληψη μετρητών ποσού 50,00 EUR από τον λογ/σμό  (20 = ATM withdrawal)
            string pattern3 = @"Ανάληψη μετρητών ποσού(.*?)από(.*?)τον(.*?)\*(.*?)\s(.*?)\s";



            match = Regex.Matches(p, pattern, RegexOptions.IgnoreCase);
            clipboard_translate_transcation(match, true, 0);

            match = Regex.Matches(p, pattern2, RegexOptions.IgnoreCase);
            clipboard_translate_transcation(match, true, 68);

            match = Regex.Matches(p, pattern3, RegexOptions.IgnoreCase);
            clipboard_translate_transcation(match, true, 20);
            //** EXPENSES ** [END]


            //** INCOME ** [START]
            string pattern4 = @"Μεταφορά ποσού(.*?)στο(.*?)λογ(.*?)\*(.*?)\s(.*?)\s";
            match = Regex.Matches(p, pattern4, RegexOptions.IgnoreCase);
            clipboard_translate_transcation(match, false, 64);


            format_grid();

            label1.Text = "Total transactions : " + dg.Rows.Count.ToString();
        }


        private void clipboard_translate_transcation(MatchCollection m, bool isExpense, int transaction_no)
        {
            string dat;
            string descr;
            string amount;

            foreach (Match c in m)
            {
                //
                dat = c.Groups[5].Value.Trim();
                descr = c.Groups[2].Value.Replace("ΑΓΟΡΑ ", "").Trim();

                if (string.IsNullOrEmpty(descr))
                {
                    if (transaction_no == 20)
                        descr = "ATM withdrawal";
                    else
                        descr = "transaction";
                }


                amount = c.Groups[1].Value.Replace("EUR", "").Trim();
                //

                add_transcaton(dat, descr, amount, transaction_no);
            }

        }

        private void add_transcaton(string dat, string descr, string amount, int transaction_no)
        {
            DataGridViewRow dr;

            dr = new DataGridViewRow();

            dr.DefaultCellStyle = cell_style;

            dr.CreateCells(dg, dat, descr, (transaction_no != 64 ? "-" : "") + amount);

            //
            if (transaction_no > 0)
            {
                /*
                 * 20 = ATM withdrawal
                 * 40 = expense (POS)
                 * 64 = income
                 * 68 = sent money + prepaid recharge 
                 * 
                 */
                toolStripDropDownButton1.Visible = true;

                if (transaction_no == 68)
                    colorize_cells(dr, Color.FromArgb(245, 219, 219));
                else if (transaction_no == 64)
                    colorize_cells(dr, Color.FromArgb(222, 242, 219));
                else if (transaction_no == 20)
                    colorize_cells(dr, Color.FromArgb(195, 220, 244));
            }

            dg.Rows.Add(dr);

        }

        private void colorize_cells(DataGridViewRow dr, Color c)
        {
            dr.Cells[0].Style.BackColor = dr.Cells[1].Style.BackColor = dr.Cells[2].Style.BackColor = c;
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
            if (General.categories == null || General.categories.Count() == 0)
            {
                MessageBox.Show("You must have categories to enter to this form!");
                return;
            }

            frmAutoCategorization y = new frmAutoCategorization();
            y.ShowDialog();
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
                        //Add the Data rows
                        csv += cell.Value.ToString().Replace(".", ",").Replace(";", ",") + ';';
                    }
                    // break;
                }

                //add new line
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
            {
                dg.Rows.Clear();
                label1.Text = "";
                toolStripDropDownButton1.Visible = true;
            }
        }

        private void dg_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //handle the error
            //when a row has a category selected and user remove it from categories dialog, dg throws error
        }


        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "CSV (Comma delimited) (*.csv)|*.csv";
            if (o.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;

            }

            DataTable dt;

            try
            {
                dt = ImportDelimitedFile(o.FileName, ";", true);

                if (dt == null || dt.Rows.Count == 0)
                {
                    throw new Exception("Cant find rows");
                }

                toolStripDropDownButton1.Visible = false;

                foreach (DataRow item in dt.Rows)
                {
                    add_transcaton(item["Ημερομηνία"].ToString(),
                                        item["Περιγραφή"].ToString(),
                                        item["Ποσό"].ToString(),
                                        int.Parse(item["Συναλλαγή"].ToString()));
                }


                format_grid();

            }
            catch (Exception ex)
            {
                string error = ex.Message;

                if (error.Contains("does not belong to table"))
                    error = "CSV doesnt contain the needed columns";

                label1.Text = "";
                MessageBox.Show(error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            label1.Text = "Total transactions : " + dg.Rows.Count.ToString();
        }

        private void format_grid()
        {
            autofillcategory();


            dg.AutoResizeColumns();

            //limit description col
            if (dg.Columns[1].Width > 184)
                dg.Columns[1].Width = 184;
        }

        public DataTable ImportDelimitedFile(string filename, string delimiter, bool first_is_column_names)
        {
            DataTable dt = new DataTable();


            using (StreamReader file = new StreamReader(filename, Encoding.Default))
            {
                //read the first line
                string line = file.ReadLine();

                //split the first line to create columns to datatable!
                string[] columns = line.Split(Convert.ToChar(delimiter));// Regex.Split(line, "|");

                for (int i = 0; i < columns.Count(); i++)
                {
                    if (first_is_column_names)
                        dt.Columns.Add(columns[i].Replace("\"", ""));
                    else
                        dt.Columns.Add("no" + i.ToString());
                }

                if (!first_is_column_names)
                {
                    //rewind reader to start!
                    file.DiscardBufferedData();
                    file.BaseStream.Seek(0, SeekOrigin.Begin);
                    file.BaseStream.Position = 0;
                }

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Trim().Length > 0)
                    {
                        line = line.Replace("\"", "");
                        string[] rows = line.Split(Convert.ToChar(delimiter));//Regex.Split(line, delimiter);
                        dt.Rows.Add(rows);

                    }
                }


            }

            return dt;
        }

        private void dg_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            label1.Text = "Total transactions : " + dg.Rows.Count.ToString();
        }

        private void restore_rows()
        {
            dg.Rows.OfType<DataGridViewRow>().ToList().ForEach(row => { row.Visible = true; });
        }

        private void toolStripFilterIncomes_Click(object sender, EventArgs e)
        {
            restore_rows();

            //show only green
            dg.Rows.OfType<DataGridViewRow>().ToList().ForEach(row => { if (!(row.Cells[0].Style.BackColor == Color.FromArgb(222, 242, 219))) row.Visible = false; });

            label1.Text = "Income transactions : " + dg.Rows.GetRowCount(DataGridViewElementStates.Visible);
        }

        private void toolStripFilterExpenses_Click(object sender, EventArgs e)
        {
            restore_rows();

            //show only red
            dg.Rows.OfType<DataGridViewRow>().ToList().ForEach(row => { if (!(row.Cells[0].Style.BackColor == Color.FromArgb(245, 219, 219))) row.Visible = false; });

            label1.Text = "Expense transactions : " + dg.Rows.GetRowCount(DataGridViewElementStates.Visible);
        }

        private void toolStripFilterATM_Click(object sender, EventArgs e)
        {
            restore_rows();

            //show only blue
            dg.Rows.OfType<DataGridViewRow>().ToList().ForEach(row => { if (!(row.Cells[0].Style.BackColor == Color.FromArgb(195, 220, 244))) row.Visible = false; });

            label1.Text = "ATM transactions : " + dg.Rows.GetRowCount(DataGridViewElementStates.Visible);
        }

        private void toolStripFilterClear_Click(object sender, EventArgs e)
        {
            restore_rows();

            label1.Text = "Total transactions : " + dg.Rows.Count.ToString();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            comboBox1.Visible = chart1.Visible = false;
            toolStrip1.Enabled = true;

            //restore cursor position ;)
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(button3.Location.X + this.Location.X + 50, button3.Location.Y + this.Location.Y - 15);
        }


        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (!chart1.Visible)
            {
                if (comboBox1.Items.Count == 0)
                {
                    Array values = Enum.GetValues(typeof(SeriesChartType));
                    comboBox1.Items.AddRange(values.Cast<object>().ToArray());
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(SeriesChartType.Doughnut);
                }


                List<string> cats = new List<string>();
                List<decimal> cats_sum = new List<decimal>();


                foreach (Category item in General.categories.OrderBy(x => x.Title))
                {


                    var total = dg.Rows.OfType<DataGridViewRow>().ToList().Where(rows => rows.Cells[3].Value != null && rows.Cells[3].Value.ToString() == item.Title).Sum(rows => decimal.Parse(rows.Cells[2].Value.ToString()));

                    if (total == 0)
                        continue;

                    cats.Add(item.Title + " (" + total + ")");
                    cats_sum.Add(total);

                }

                var uncategorize = dg.Rows.OfType<DataGridViewRow>().ToList().Where(rows => rows.Cells[3].Value == null).Sum(rows => decimal.Parse(rows.Cells[2].Value.ToString()));
                if (uncategorize != 0)
                {
                    cats.Add("Uncategorized (" + uncategorize + ")");
                    cats_sum.Add(uncategorize);
                }

                if (cats.Count == 0)
                {
                    MessageBox.Show("Please add transactions", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    chart1.Visible = true;

                    toolStrip1.Enabled = false;
                }

                this.WindowState = FormWindowState.Maximized;

                chart1.Series[0]["PieLabelStyle"] = "Outside";

                chart1.Series[0].Points.DataBindXY(cats, cats_sum);
                chart1.Dock = DockStyle.Fill;


                comboBox1.Top = button2.Top;
                comboBox1.Left = button2.Left;
                comboBox1.Visible = true;
                comboBox1.BringToFront();

            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog o = new SaveFileDialog();
            o.Filter = "PNG Format (*.png)|*.png";
            if (o.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            try
            {
                this.chart1.SaveImage(o.FileName, ChartImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //act as 
            chart1_Click(null, null);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && chart1.Visible)
            {
                chart1.Series[0].ChartType = (SeriesChartType)comboBox1.SelectedItem;

                toolStripButton5_Click(null, null);
            }
        }




    }
}
