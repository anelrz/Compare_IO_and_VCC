using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Compare_IO_and_VCC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Run_Button_Click(object sender, EventArgs e)
        {

        }

        private void Load_VCC_file_Button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XDocument Xdoc = new XDocument();
                    try
                    {
                        Xdoc = XDocument.Load(openFileDialog1.OpenFile());
                        XNamespace XMLnamespace = "http://schemas.microsoft.com/clr/nsassem/BKVibro.Compass.Server/Dbsimulator%2C%20Version%3D0.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull";
                        IEnumerable<XElement> tags = Xdoc.Descendants(XMLnamespace + "ChannelServerSetupNode");

                        foreach (XElement x in tags)
                            listBox1.Items.Add(x.Element("NodeName").Value.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
