using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NBG_Helper
{
    public partial class frmAutoCategorization : Form
    {
        public frmAutoCategorization()
        {
            InitializeComponent();

            if (General.AutoCategorization == null)
                General.AutoCategorization = new List<AutoCategorization>();

            fill_list();
            fill_combo();
        }

        private void fill_combo()
        {
            comboBox1.DataSource = General.categories.OrderBy(x => x.Title).Select(x => x.Title).ToList();
        }

        private void fill_list()
        {
            lst.Items.Clear();

            foreach (var x in General.AutoCategorization.OrderBy(x => x.Description))
            {
                ListViewItem item = new ListViewItem(x.Description);
                item.SubItems.Add(x.CategoryTitle);
                lst.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0 || comboBox1.Text == "")
            {
                MessageBox.Show("Please enter something!");
                return;
            }

            General.AutoCategorization.Add(new AutoCategorization { Description = textBox1.Text.Trim(), CategoryTitle = comboBox1.Text });

            XmlHelper.ToXmlFile(General.AutoCategorization, General.AutoCategorizationXML);

            fill_list();

            textBox1.Text = "";
            comboBox1.Text = "";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lst.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select something!");
                return;
            }

            if (MessageBox.Show("You will delete item :\r\n\r\n" + lst.SelectedItems[0].Text + "\r\n\r\nAre you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            General.AutoCategorization.RemoveAll(x => x.Description == lst.SelectedItems[0].Text && x.CategoryTitle == lst.SelectedItems[0].SubItems[1].Text);

            XmlHelper.ToXmlFile(General.AutoCategorization, General.AutoCategorizationXML);

            fill_list();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
