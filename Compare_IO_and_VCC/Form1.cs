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
using System.IO.Compression;

namespace Compare_IO_and_VCC
{
    public partial class Form1 : Form
    {

        DirectoryInfo di;
        FileInfo fileinfo;

        public Form1()
        {
            InitializeComponent();
        }

        private void Run_Button_Click(object sender, EventArgs e)
        {
            
        }

        private void Load_VCC_file_Button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "VCC Files (.VCC)|*.vcc|All Files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String filename in openFileDialog1.FileNames)
                {
                    openFileDialog1.FileName = filename;
                    Stopwatch sw = new Stopwatch();

                    //Async Extract of XML files from VCC
                    sw.Start();
                    Extract_VCC(openFileDialog1, "tempfolder");
                    sw.Stop();
                    textBox1.Text = "Extracting time [miliseconds]: " + sw.ElapsedMilliseconds.ToString();
                    IEnumerable<string> XMLfiles = Directory.EnumerateFiles(di.FullName);

                    IEnumerable<string> serveresetupnodes_query =
                        from name in XMLfiles
                        where name.Contains("ServerSetupNode")
                        select name;

                    //Process files
                    try
                    {
                        XDocument Xdoc = new XDocument();
                        List<XElement> myList = new List<XElement>();
                        Xdoc = XDocument.Load(serveresetupnodes_query.First<string>());
                        XNamespace XMLnamespace = "http://schemas.microsoft.com/clr/nsassem/BKVibro.Compass.Server/Dbsimulator%2C%20Version%3D0.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull";
                        IEnumerable<XElement> tags = Xdoc.Descendants(XMLnamespace + "ChannelServerSetupNode");

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

                        TreeNode rootnode = treeView1.Nodes.Add(openFileDialog1.SafeFileName);

                        foreach (XElement x in tagQuery)
                        {
                            try
                            {
                                TreeNode node = rootnode.Nodes.Add(x.Element("NodeName").Value);
                                node.Nodes.Add(x.Element("NodeId").Value, "Node Id");
                                node.Nodes.Add(x.Element("NodeId").Value, "Range");
                                TreeNode node_limit = node.Nodes.Add(x.Element("NodeId").Value, "Limits");
                                node_limit.Nodes.Add(x.Element("NodeId").Value, "Alert High");
                                node_limit.Nodes.Add(x.Element("NodeId").Value, "Danger High");
                                node_limit.Nodes.Add(x.Element("NodeId").Value, "Alert Low");
                                node_limit.Nodes.Add(x.Element("NodeId").Value, "Danger Low");
                            }
                            catch (Exception exc)
                            {
                                Debug.WriteLine(exc.Message);
                            }
                        }
                        treeView1.Update();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                    //Delete temp folder
                    Directory.Delete(di.FullName, true);
                }
            }
        }

        private void Extract_VCC(OpenFileDialog fd, string s)
        {
            //Extract XML files in VCC to disc
            fileinfo = new FileInfo(fd.FileName);
            di = Directory.CreateDirectory(fileinfo.DirectoryName + "\\" + s);
            backgroundWorker1.RunWorkerAsync(fileinfo);
            while (this.backgroundWorker1.IsBusy)
            {
                Application.DoEvents();
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = e.Node.Name;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(fileinfo.FullName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.ExtractToFile(Path.Combine(di.FullName, entry.Name), true);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.GetType());
                Debug.WriteLine(exc.Message);
            }
        }
    }
}
