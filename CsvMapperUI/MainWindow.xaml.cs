using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Win32;

namespace CsvMapperUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Mapper = System.IO.Path.Combine(Environment.CurrentDirectory, "CsvMapper.exe");
                if (!File.Exists(Mapper))
                {
                    MessageBox.Show($"{Mapper} not there.");
                }

                if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSource) && File.Exists(Properties.Settings.Default.LastSource))
                {
                    SourceFile_tb.Text = Properties.Settings.Default.LastSource;
                }
                if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastTarget))
                {
                    TargetFile_tb.Text = Properties.Settings.Default.LastTarget;
                }
                MapPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "FieldMap.xml");
                if (System.IO.File.Exists(MapPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(MapPath);
                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings xws = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "    ",
                        NewLineChars = "\r\n",
                        NewLineHandling = NewLineHandling.Replace
                    };
                    using(XmlWriter xtr = XmlWriter.Create(sb,xws))
                    {
                        doc.Save(xtr);
                        Map_tb.Text = sb.ToString();
                    }
                }
                else
                {
                    Map_tb.Text = Properties.Settings.Default.DefaultMap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        string MapPath { get; set; }
        string Mapper { get; set; }

        private void SourceFile_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.DefaultExt = ".csv";
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSource) && File.Exists(Properties.Settings.Default.LastSource))
            {
                dlg.FileName = Properties.Settings.Default.LastSource;
            }
            if ((bool)dlg.ShowDialog())
            {
                SourceFile_tb.Text = dlg.FileName;
                Properties.Settings.Default.LastSource = dlg.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void TargetFile_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = false;
            dlg.DefaultExt = ".csv";
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastTarget))
            {
                TargetFile_tb.Text = Properties.Settings.Default.LastTarget;
            }
            if ((bool)dlg.ShowDialog())
            {
                TargetFile_tb.Text = dlg.FileName;
                Properties.Settings.Default.LastTarget = dlg.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void Run_btn_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(Map_tb.Text);
                if (System.IO.File.Exists(MapPath))
                {
                    System.IO.File.Delete(MapPath);
                }
                doc.Save(MapPath);
                using (Process p = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = Mapper,
                        Arguments = $"\"{SourceFile_tb.Text}\" \"{TargetFile_tb.Text}\" \"{MapPath}\""
                    };
                    p.StartInfo = psi;
                    p.Start();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
