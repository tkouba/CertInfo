using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CertInfo
{
    public partial class DialogPassword : Form
    {
        public string Password { get; set; }

        public DialogPassword()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Password = textPassword.Text;
        }
    }
}
