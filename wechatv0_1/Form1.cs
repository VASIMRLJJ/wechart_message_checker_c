using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace wechatv0_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private void button1_Click(object sender, EventArgs e)
        {
            //连接到的目标IP

            IPAddress ip = IPAddress.Parse(txtIP.Text);

            //IPAddress ip = IPAddress.Any;

            //连接到目标IP的哪个应用(端口号！)

            IPEndPoint point = new IPEndPoint(ip, int.Parse(txtPort.Text));

            try

            {

                //连接到服务器

                client.Connect(point);

                ShowMsg("连接成功");

                ShowMsg("服务器" + client.RemoteEndPoint.ToString());

                ShowMsg("客户端:" + client.LocalEndPoint.ToString());

                //连接成功后，就可以接收服务器发送的信息了

                Thread th = new Thread(ReceiveMsg);

                th.IsBackground = true;

                th.Start();

            }

            catch (Exception ex)

            {

                ShowMsg(ex.Message);

            }
        }

        //接收服务器的消息

        void ReceiveMsg()

        {

            while (true)

            {

                try

                {

                    byte[] buffer = new byte[1024 * 1024];

                    int n = client.Receive(buffer);

                    string s = Encoding.UTF8.GetString(buffer, 0, n);

                    if (!s.Equals(""))
                    {
                        ShowMsg(s);
                    }


                }

                catch (Exception ex)

                {

                    ShowMsg(ex.Message);

                    break;

                }

            }

        }
        void ShowMsg(string msg)

        {

            txtInfo.AppendText(msg + "\r\n");
            string[] sArray = msg.Split(new char[2] { '\n', ':' });
            if (sArray.Length > 1 && sArray[0].Contains("有备注"))
            {
                string s = sArray[1];
                string t = msg.Substring(msg.Length - 20);
                listBox1.Items.Add(s + '|' + t);
                foreach (object i in listBox3.Items)
                {
                    if (s.Contains(i.ToString()))
                    {
                        listBox3.Items.Remove(i);
                        break;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //客户端给服务器发消息

            if (client != null)

            {

                try

                {
                    ShowMsg(txtMid.Text);
                    byte[] buffer = Encoding.UTF8.GetBytes(txtMid.Text);
                    client.Send(buffer);
                }

                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(Settings1.Default.names == null)
                Settings1.Default.names = new StringCollection();
            Control.CheckForIllegalCrossThreadCalls = false;
            txtIP.Text = Settings1.Default.ip;
            txtPort.Text = Settings1.Default.port;
            foreach (string name in Settings1.Default.names)
                    listBox3.Items.Add(name);

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                Brush mybsh = Brushes.Black;                // 判断是什么类型的标签    
                string s = listBox1.Items[e.Index].ToString();
                DateTime dateTime = Convert.ToDateTime(s.Substring(s.Length - 20));
                if (DateTime.Compare(dateTime, dateTimePicker1.Value) > 0)
                {
                    mybsh = Brushes.Red;
                }
                else
                {
                    mybsh = Brushes.Black;
                }                // 焦点框                
                e.DrawFocusRectangle();                //文本                 
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, mybsh, e.Bounds, StringFormat.GenericDefault);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                StreamReader sr = new StreamReader(fileName, Encoding.Default);
                string line;
                listBox3.Items.Clear();
                Settings1.Default.names.Clear();
                while ((line = sr.ReadLine()) != null)
                {
                    listBox3.Items.Add(line);
                    Settings1.Default.names.Add(line);
                }
                if (sr != null)
                    sr.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings1.Default.ip = txtIP.Text;
            Settings1.Default.port = txtPort.Text;
            Settings1.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }
    }
}

