using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CertInfo
{
    static class ListViewHelper
    {
        public static ListViewItem ChangeForeColor(this ListViewItem item, bool condition, Color foreColor)
        {
            if (condition)
                item.ForeColor = foreColor;
            return item;
        }
    }
}
