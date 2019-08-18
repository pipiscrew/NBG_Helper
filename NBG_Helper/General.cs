using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
