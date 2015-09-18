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
                    Stopwatch sw = new Stopwatch();
                    XDocument Xdoc = new XDocument();
                    List<XElement> myList = new List<XElement>();
                    Xdoc = XDocument.Load(openFileDialog1.OpenFile());
                    XNamespace XMLnamespace = "http://schemas.microsoft.com/clr/nsassem/BKVibro.Compass.Server/Dbsimulator%2C%20Version%3D0.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull";
                    IEnumerable<XElement> tags = Xdoc.Descendants(XMLnamespace + "ChannelServerSetupNode");
                    sw.Start();
                    IEnumerable<XElement> tagQuery =
                    from tag in tags
                    let s = tag.Element("NodeName").Value
                    where !(s.Contains("Relay") ||
                                s.Contains("Trigger Channel") ||
                                s.Contains("Channel var.") ||
                                s.Contains("Binary Input") ||
                                s.Contains("Rod Drop") ||
                                s.Contains("TM Input") ||
                                s.Contains("DC Output") ||
                                s.Contains("PS_Monitor") ||
                                s.Contains("Tachometer"))
                    select tag;
                    sw.Stop();
                    textBox1.Text = "Query time [ticks]: " + sw.ElapsedTicks.ToString();

                    foreach(XElement x in tagQuery)
                    {
                        try
                        {
                            listBox1.Items.Add(x.Element("NodeName").Value + " " + x.Element("NodeId").Value);
                        }
                        catch(Exception exc)
                        {
                            Debug.WriteLine(exc.Message);
                        }
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
