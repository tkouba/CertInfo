using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace CertInfo
{
    partial class DialogAbout : Form
    {

        /// <summary>
        ///  Regular expression built for C# on: po, pro 28, 2020, 02:59:56 odp.
        ///  Using Expresso Version: 3.1.6224, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  Select from 2 alternatives
        ///      Match expression but don't capture it. [(?<text>.*?)\[(?<link>[^][()]*?)\]\((?<url>[^][()]*?)\)]
        ///          (?<text>.*?)\[(?<link>[^][()]*?)\]\((?<url>[^][()]*?)\)
        ///              [text]: A named capture group. [.*?]
        ///                  Any character, any number of repetitions, as few as possible
        ///              Literal [
        ///              [link]: A named capture group. [[^][()]*?]
        ///                  Any character that is NOT in this class: [][()], any number of repetitions, as few as possible
        ///              Literal ]
        ///              Literal (
        ///              [url]: A named capture group. [[^][()]*?]
        ///                  Any character that is NOT in this class: [][()], any number of repetitions, as few as possible
        ///              Literal )
        ///      [text]: A named capture group. [.*]
        ///          Any character, any number of repetitions
        ///  
        ///
        /// </summary>
        static Regex urlTokenizerRegex = new Regex(
              "(?:(?<text>.*?)\\[(?<link>[^][()]*?)\\]\\((?<url>[^][()]*?)\\)" +
              ")|(?<text>.*)",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );

        //// Replace the matched text in the InputText using the replacement pattern
        // string result = regex.Replace(InputText,regexReplace);

        //// Split the InputText wherever the regex matches
        // string[] results = regex.Split(InputText);

        //// Capture the first Match, if any, in the InputText
        // Match m = regex.Match(InputText);

        //// Capture all Matches in the InputText
        // MatchCollection ms = regex.Matches(InputText);

        //// Test to see if there is a match in the InputText
        // bool IsMatch = regex.IsMatch(InputText);

        //// Get the names of all the named and numbered capture groups
        // string[] GroupNames = regex.GetGroupNames();

        //// Get the numbers of all the named and numbered capture groups
        // int[] GroupNumbers = regex.GetGroupNumbers();




        public static void Show(Form owner)
        {
            using (DialogAbout d = new DialogAbout())
            {
                d.ShowDialog(owner);
            }
        }

        public DialogAbout()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            FillLinkLabel(this.linkLabelDescription, AssemblyDescription);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void DialogAbout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                Close();
                e.Handled = true;
            }
        }

        void FillLinkLabel(LinkLabel label, string text)
        {
            label.Text = String.Empty;
            MatchCollection matches = urlTokenizerRegex.Matches(text);
            foreach (Match item in matches)
            {
                if (item.Success)
                {
                    if (!String.IsNullOrEmpty(item.Groups["text"].Value))
                    {
                        label.Text += item.Groups["text"].Value.Replace("\r", Environment.NewLine);
                    }
                    if (!String.IsNullOrEmpty(item.Groups["link"].Value))
                    {
                        label.Links.Add(label.Text.Length, item.Groups["link"].Value.Length, item.Groups["url"].Value);
                        label.Text += item.Groups["link"].Value;
                    }
                }
            }
        }

        private void linkLabelDescription_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = e.Link.LinkData as string;
            if (!String.IsNullOrWhiteSpace(url))
                Process.Start(new ProcessStartInfo(url));
        }
    }
}
