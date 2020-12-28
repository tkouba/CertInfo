using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace CertInfo
{

    public partial class FormMain : Form
    {
        // P/Invoke constants
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;
        // ID for the About item on the system menu
        private const int IDM_ABOUT = 1000;

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        public string FileName { get; private set; }
        public string Password { get; private set; }

        public FormMain(string fn, string pwd)
        {
            InitializeComponent();
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ResizeColumns();
            FileName = fn;
            Password = pwd;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Get a handle to a copy of this form's system (window) menu
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

            // Add a separator
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);

            // Add the About menu item
            AppendMenu(hSysMenu, MF_STRING, IDM_ABOUT, "&About…");
        }

        protected override void WndProc(ref Message m)
        {
            // Test if the About item was selected from the system menu
            if (m.Msg == WM_SYSCOMMAND)               
            {
                switch (m.WParam.ToInt32())
                {
                    case IDM_ABOUT:
                        DialogAbout.Show(this);
                        return;
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                if (String.IsNullOrEmpty(FileName))
                    OpenFile();
                if (!String.IsNullOrEmpty(FileName))
                    LoadFile();
                else
                    Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }

        private void listView_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopyToClipboard();
        }

        private void ResizeColumns()
        {
            column2.Width = listView.ClientSize.Width - column1.Width;
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
                CopyToClipboard();
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
                Close();
        }

        void CopyToClipboard()
        {
            if (listView.FocusedItem != null &&
                listView.FocusedItem.SubItems.Count > 1)
            {
                Clipboard.SetText(listView.FocusedItem.SubItems[1].Text);
            }
        }

        void OpenFile()
        {
            using (var ofd = new OpenFileDialog()
            {
                //DefaultExt = "pfx",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "PFX - PKCS#12|*.p12;*.pfx|P7B - PKCS#7|*.p7b;*.p7c|DER|*.der;*.crt;*.cer|PEM - x509|*.pem;*.crt;*.cer;*.key;*.txt|All files|*.*"                
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                }
            }
        }

        void LoadFile()
        {
            X509Certificate2 x509 = new X509Certificate2();
            try
            {
                x509.Import(FileName);
            }
            catch (CryptographicException ex) when ((uint)Marshal.GetHRForException(ex) == 0x80070056)
            {
                if (String.IsNullOrEmpty(Password))
                    Password = GetPassword();
                x509.Import(FileName, Password, X509KeyStorageFlags.DefaultKeySet);
            }
            Text = String.Format("{0} - {1}", Text, Path.GetFileName(FileName));

            ListViewGroup grp = AddGroup("Certificate");

            AddItem(grp, "Subject", x509.Subject);
            AddItem(grp, "Issuer", x509.Issuer);
            AddItem(grp, "Version", x509.Version.ToString());
            AddItem(grp, "Valid Date", x509.NotBefore.ToShortDateString())
                .ChangeForeColor(x509.NotBefore > DateTime.Today, Color.Red);
            AddItem(grp, "Expiry Date", x509.NotAfter.ToShortDateString())
                .ChangeForeColor(x509.NotAfter < DateTime.Today, Color.Red);
            AddItem(grp, "Thumbprint", x509.Thumbprint);
            AddItem(grp, "Serial Number", x509.SerialNumber);
            AddItem(grp, "Raw Data Length", x509.RawData.Length.ToString());
            AddItem(grp, "Friendly Name", x509.FriendlyName);
            AddItem(grp, "Signature Algorithm", x509.SignatureAlgorithm.FriendlyName);

            AddItem(grp, "Cerificate Hash", x509.GetCertHashString());

            AddItem(grp, "DNS Name", x509.GetNameInfo(X509NameType.DnsName, true));
            AddItem(grp, "DNS From Alternative Name", x509.GetNameInfo(X509NameType.DnsFromAlternativeName, true));
            AddItem(grp, "Email Name", x509.GetNameInfo(X509NameType.EmailName, true));
            AddItem(grp, "Simple Name", x509.GetNameInfo(X509NameType.SimpleName, true));
            AddItem(grp, "UPN Name", x509.GetNameInfo(X509NameType.UpnName, true));
            AddItem(grp, "URL Name", x509.GetNameInfo(X509NameType.UrlName, true));

            AddItem(grp, "Format", x509.GetFormat());

            AddItem(grp, "Key Algorithm Parameters", x509.GetKeyAlgorithmParametersString());

            AddItem(grp, "Archived", x509.Archived.ToString());

            AddItem(grp, "Certificate to string", x509.ToString(true));
            //AddItem(grp, "Certificate to XML String", x509.PublicKey.Key.ToXmlString(false));

            grp = AddGroup("Public key");
            AddItem(grp, "Friendly Name", x509.PublicKey.Oid.FriendlyName);
            AddItem(grp, "Public Key Format", x509.PublicKey.EncodedKeyValue.Format(true));
            AddItem(grp, "Key Size", x509.PublicKey.Key.KeySize.ToString());
            AddItem(grp, "Signature Algorithm", x509.PublicKey.Key.SignatureAlgorithm);
            AddItem(grp, "Key Exchange Algorithm", x509.PublicKey.Key.KeyExchangeAlgorithm);

            if (x509.HasPrivateKey)
            {
                grp = AddGroup("Private key");
                AddItem(grp, "Key Size", x509.PrivateKey.KeySize.ToString());
                AddItem(grp, "Signature Algorithm", x509.PrivateKey.SignatureAlgorithm);
            }

            if (x509.Extensions.Count > 0)
            {
                grp = AddGroup("Extensions");
                foreach (var item in x509.Extensions)
                {
                    AddItem(grp, item.Oid.FriendlyName, item.Oid.Value);
                }
            }

        }

        string GetPassword()
        {
            using (var d = new DialogPassword())
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    return d.Password;
                }
            }
            return null;
        }

        ListViewItem AddItem(ListViewGroup group, string label, string content)
        {
            if (!String.IsNullOrEmpty(content))
                return listView.Items.Add(new ListViewItem(new string[] { label, content }, group));
            return null;
        }

        ListViewGroup AddGroup(string label)
        {
            ListViewGroup group = new ListViewGroup(label);
            listView.Groups.Add(group);
            return group;
        }

    }
}
