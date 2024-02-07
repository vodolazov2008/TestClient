using Microsoft.VisualBasic;
using Scada.Client;
using Scada.Data.Models;
using Scada.Lang;
using Scada.Log;
using Scada.Protocol;
using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd/MM/yyyy HH:mm:ss";
            //comboBox1.SelectedItem = null;
        }

        private static ScadaClient client;
        public static string exeDir;
        public static DateTime dateTime;
        public static int inCnlNum;
        public static string inCnlVal;
        public static long SessionID;
        public static string ServerName;
        public ArchiveMask SelectedIndex;
        /*
        class ListArchive
        {
            public int Id { get; set; }
            public ArchiveMask Mask { get; set; }
        }
        List<ListArchive> phones = new List<ListArchive>
        {
            new ListArchive { Id=1, Mask=ArchiveMask.Default},
            new ListArchive { Id=2, Mask=All},
            new ListArchive { Id=3, Mask="iPhone 6"},
            new ListArchive { Id=4, Mask="Microsoft Lumia 435"},
            new ListArchive { Id=5, Mask="Xiaomi Mi 5"}
        };
        */

        static ConnectionOptions LoadConfig()
        {
            ConnectionOptions connectionOptions = new();
            XmlDocument xmlDocument = new();
            xmlDocument.Load(Path.Combine(exeDir, "Config", "ScadaTestClient.xml"));
            if (xmlDocument.DocumentElement.SelectSingleNode("ConnectionOptions") is XmlNode xmlNode)
                connectionOptions.LoadFromXml(xmlNode);
            return connectionOptions;
        }
        private void GetStatus()
        {
            bool serverIsReady;
            bool userIsLoggedIn;
            client.GetStatus(out serverIsReady, out userIsLoggedIn);
            label4.Enabled = userIsLoggedIn;
            label5.Enabled = serverIsReady;
            btnWrite.Enabled = serverIsReady;
        }

        static void WriteHistoricalData()
        {
            DateTime timestamp = dateTime;
            int cnlNum = inCnlNum;
            double cnlVal = double.Parse(inCnlVal);

            Slice slice = new(
                timestamp,
                new int[] { cnlNum },
                new CnlData[] { new CnlData(cnlVal, 1) });

            client.WriteHistoricalData(ArchiveMask.Default, slice, WriteDataFlags.Default);
            // client.WriteHistoricalData(SelectedIndex.Value, slice, WriteDataFlags.Default);
            //client.WriteHistoricalData(archiveBit, slice, WriteDataFlags.Default);            
        }
        private void BtnWrite_Click(object sender, EventArgs e)
        {
            WriteHistoricalData();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTime = dateTimePicker1.Value.ToUniversalTime();
            //
            //MessageBox.Show(dateTime.ToString());
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            inCnlNum = (int)numericUpDown1.Value;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as System.Windows.Forms.TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            inCnlVal = textBox1.Text;
        }
        static void Client_Progress(object sender, ProgressEventArgs e)
        {
            //Console.CursorLeft = 0;
            //Console.Write("{0} blocks of {1}", e.BlockNumber, e.BlockCount);
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            CommonPhrases.Init();
            exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ILog commLog = new LogFile(LogFormat.Simple)
            {
                FileName = Path.Combine(exeDir, "Log", "ScadaTestClient.log"),
                TimestampFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ff"
            };
            ConnectionOptions connectionOptions = LoadConfig();
            client = new ScadaClient(connectionOptions) { CommLog = commLog };
            client.Progress += Client_Progress;
            GetStatus();
            SessionID = client.SessionID;
            ServerName = client.ServerName;
            label6.Text = connectionOptions.Host.ToString();
            label7.Text = SessionID.ToString();
            label8.Text = client.ServerName.ToString();
            //MessageBox.Show(DateTime.UtcNow.ToLongDateString);
            //MessageBox.Show(DateTimeOffset.UtcNow.DateTime.ToString());
        }

    }
}
