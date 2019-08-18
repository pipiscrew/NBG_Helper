using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NBG_Helper
{
    public partial class frmCategories : Form
    {
        public frmCategories()
        {
            InitializeComponent();

            if (General.categories == null)
                General.categories = new List<Category>();


            fill_list();
        }

        private void fill_list()
        {
            lst.DataSource = General.categories.Where(x => x.Title != "").OrderBy(x => x.Title).Select(x => x.Title).ToList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter something!");
                return;
            }

            General.categories.Add(new Category { Title = textBox1.Text.Trim() });

            XmlHelper.ToXmlFile(General.categories.Where(x => x.Title != "").ToList(), General.CategoriesXML);

            fill_list();

            textBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lst.SelectedItem == null)
            {
                MessageBox.Show("Please select something!");
                return;
            }

            string item = lst.SelectedItem.ToString();

            if (MessageBox.Show("You will delete item :\r\n\r\n" + item + "\r\n\r\nAre you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }


            General.categories.RemoveAll(x => x.Title == item);

            XmlHelper.ToXmlFile(General.categories.Where(x => x.Title != "").ToList(), General.CategoriesXML);

            fill_list();
        }
    }
}
