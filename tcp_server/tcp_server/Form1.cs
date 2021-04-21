using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetFrame.Net.TCP.Listener.Asynchronous;
using System.Configuration;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace tcp_server
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        private string strFilePath = Application.StartupPath + "\\FileConfig.ini";//获取INI文件路径
        private string strSec = "section1"; //INI文件名
        //private static int all_recv_num = 0;
        public Form1()
        {
            InitializeComponent();
            var str = Console.ReadLine();
            Thread workThread = new Thread(new ParameterizedThreadStart(ThreadWork_start));
            workThread.Start(str);
            label2.Text = recv_data_globa_len.ToString();
            label6.Text = "192.168.0.112:6800";
            all_button_set(false);
            this.Text = "200k pc tools--版本号1.01.04";

            if (File.Exists(strFilePath))//读取时先要判读INI文件是否存在
            {

                StringBuilder temp = new StringBuilder(1024);
                int ret = GetPrivateProfileString(strSec, "Name", "", temp, 1024, strFilePath);
                textBox1.Text = temp.ToString();


            }
            //textBox1.Text = ConfigurationManager.ConnectionStrings["time"].ToString();
        }
        /* Initializes the Listener */

        static AsyncTCPServer tttp = new AsyncTCPServer(IPAddress.Parse("192.168.0.112"), 6800);
        static TCPClientState client_fd;
        static byte[] recv_data_globa = new byte[1000 * 400 * 240];//240s,200k short =400k byte 
        static int recv_data_globa_len = 0;
        static int start_recv_sample_data_flag = 0;//接收采样数据中
        private static void DataReceived_callback(object os, AsyncEventArgs state)
        {
            //优先判断是否有应答消息
           

            
            if (start_recv_sample_data_flag == 1)
            {
                //保存数据到内存
                //all_recv_num = all_recv_num + state._state.buffer_data_size;

                for (int i = 0; i < state._state.buffer_data_size; i++)
                {
                    Console.Write(state._state.Buffer[i].ToString("x2") + ",");  //法1
                }
                Console.WriteLine("\n");
                Buffer.BlockCopy(state._state.Buffer, 0,
                    recv_data_globa, recv_data_globa_len,
                    state._state.buffer_data_size);
                recv_data_globa_len = recv_data_globa_len + state._state.buffer_data_size;
            }
            else
            {
                //解析消息
                //byte[] msg = new byte[state._state.buffer_data_size];
                show_status_info(System.Text.Encoding.Default.GetString(state._state.Buffer, 0, state._state.buffer_data_size));
                return;
            }

        }
        private void button4_Click(object sender, EventArgs e)
        {
            string suffix_gain = "gain0";
            if (radioButton9.Checked)
            {
                suffix_gain = "gain0";
            }
            else if (radioButton10.Checked)
            {
                suffix_gain = "gain1";
            }
            else if (radioButton11.Checked)
            {
                suffix_gain = "gain2";
            }
            else if (radioButton12.Checked)
            {
                suffix_gain = "gain3";
            }
            string file_name = DateTime.Now.ToLocalTime().ToString("yyyy_MM_dd_HH_mm_ss")
                +"_"+suffix_gain+"_200k.txt";
            Console.WriteLine(file_name);

            //recv_data_globa[0] = 0x10;
            //recv_data_globa[1] = 0x20;
            //recv_data_globa[2] = 0x14;
            //recv_data_globa[3] = 0x20;
            //recv_data_globa[4] = 0x13;
            //recv_data_globa[5] = 0x20;
            //for (int i = 0; i < 6 / 2; i++)
            //{
            //    int tmp = recv_data_globa[i * 2] + recv_data_globa[i * 2 + 1] * 256;
            //    tmp = (tmp & 0x3fff) >> 2;
            //    tmp = tmp * 3300;
            //    tmp = tmp / 4096;
                
            //    Console.WriteLine(tmp.ToString());   
            //}

            StreamWriter sw = File.CreateText(file_name);
            for (int i = 0; i < recv_data_globa_len / 2; i++)
            {
              
                  //采集已经转换过了
                //int tmp = recv_data_globa[i * 2] + recv_data_globa[i * 2 + 1] * 256;
                //tmp = (tmp & 0x3fff) >> 2;

                int tmp = recv_data_globa[i * 2] + recv_data_globa[i * 2 + 1] * 256;
                tmp = tmp * 3300;
                tmp = tmp / 4096;
                sw.Write(tmp.ToString("D4") + "\n");                //写入一行文本 
                
                //sw.WriteLine(recv_data_globa[i * 2].ToString("x2") + recv_data_globa[i * 2 + 1].ToString("x2"));                //写入一行文本 
                //sw.Write(tmp.ToString("x4")+"\n");                //写入一行文本 
                //sw.Write(tmp.ToString() + "\n");
            }
            sw.Flush();                    //清空 
            sw.Close();                    //关闭 

            show_status_info("保存成功," + file_name);
        }
        private static void ThreadWork_start(object param)
        {
            //StartServer();

            tttp.DataReceived += new System.EventHandler<AsyncEventArgs>(DataReceived_callback);
            tttp.ClientConnected += new System.EventHandler<AsyncEventArgs>(clientconnect_callback);
            tttp.ClientDisconnected += new System.EventHandler<AsyncEventArgs>(clientdisconnect_callback);
            tttp.CompletedSend += new System.EventHandler<AsyncEventArgs>(sendcomplete_callback);
            tttp.Start();
        }
        private static void sendcomplete_callback(object os, AsyncEventArgs state)
        {
            //send complete

        }
        private static void send_read_gain()
        {
            byte[] message = System.Text.Encoding.Default.GetBytes("set read_gain");
            tttp.Send(client_fd, message);
        }
        private static void clientconnect_callback(object os, AsyncEventArgs state)
        {
            show_connect_info("connect:" + state._state.TcpClient.Client.RemoteEndPoint.ToString());
            client_fd = state._state;//保存client信息,发送会用到
            send_read_gain();
        }
        private static void clientdisconnect_callback(object os, AsyncEventArgs state)
        {
            show_connect_info("disconnect");
            do_update_type = do_update_type | 0x0002;
        }

        private static void show_status_info(string info)
        {
            
            status_info = info;
            do_update_type |= 0x0004;

        }
        private static void show_connect_info(string info)
        {
            label4_text = info;
            do_update_type = do_update_type | 0x0002;
        }

        private void button1_Click_1(object sender, System.EventArgs e)
        {
            //all_recv_num = 0;
            recv_data_globa_len = 0;
            label2.Text = recv_data_globa_len.ToString();
        }
        private void parse_msg_data(string info)
        {
            if (info.StartsWith("b>>>") && (info.EndsWith("<<<d") || info.EndsWith("<<<d\n")))
            {
                if (info.IndexOf("read_param=", 0) > 0)
                {

                }
                else if (info.IndexOf("read_gain=", 0) > 0)
                {
                    int offset_start = info.IndexOf("read_gain=", 0);
                    int offset_end = info.IndexOf(",", 0);

                    string gain = info.Substring(offset_start + 10, 1);
                    if (gain == "0")
                    {
                        radioButton9.Checked = true;
                    }
                    else if (gain == "1")
                    {
                        radioButton10.Checked = true;

                    }
                    else if (gain == "2")
                    {
                        radioButton11.Checked = true;

                    }
                    else if (gain == "3")
                    {
                        radioButton12.Checked = true;
                    }
                    //---设备版本:------
                    offset_start = info.IndexOf("version=", 0)+8;
                    label9.Text = "设备版本:  " + info.Substring(offset_start, 8);
                }
                  
            }
        }
        private static UInt32 do_update_type = 0;
        private static string label4_text = "";
        private static string status_info = "";
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            UInt32 tmp;
            //tmp = do_update_type & 0x0001;
            //if (tmp > 0)
            //{
            label2.Text = (recv_data_globa_len/2).ToString();
            //}
            tmp = do_update_type & 0x0002;
            if (tmp > 0)
            {
                if (label4_text.StartsWith("connect"))
                {

                    all_button_set(true);
                }
                else
                {
                    all_button_set(false);
                }
                label4.Text = label4_text;

            }
            tmp = do_update_type & 0x0004;
            if (tmp > 0)
            {
                label7.Text = "消息状态:" + status_info;
                parse_msg_data(status_info);
            }
            do_update_type = 0;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //tServer.Stop();
            tttp.Start();
        }
        DateTime t_last = System.DateTime.Now;
        private UInt64 start_timestamp = 0;
        private void button2_Click(object sender, System.EventArgs e)
        {
            recv_data_globa_len = 0;//清零接收缓存
            //all_recv_num = 0;
            //client_fd
            byte[] message = System.Text.Encoding.Default.GetBytes("set start");
           
            tttp.Send(client_fd, message);
            t_last = System.DateTime.Now;
            start_recv_sample_data_flag = 1;
            //Send(client_fd, "hello world");
            button3.Enabled = true;
            //------------
            timer_work.Interval = System.Convert.ToInt32(textBox1.Text)*1000;
            timer_work.Enabled = true;
        }

        
        public void all_button_set(bool flag)
        {
            if (flag)
            {
                button2.Enabled = true;
                //button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
            else
            {

                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //client_fd
            if (radioButton9.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set gain=0");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if (radioButton10.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set gain=1");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if (radioButton11.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set gain=2");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if (radioButton12.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set gain=3");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
               //client_fd


            byte[] message = System.Text.Encoding.Default.GetBytes("set stop");
            Console.WriteLine(System.Text.Encoding.Default.GetString(message));
            tttp.Send(client_fd, message);
            //------延时停止-----
            timer2_stop_delay.Interval = 1000;
            timer2_stop_delay.Enabled = true;
            timer_work.Enabled = false;

        }

    

        private void timer2_stop_delay_Tick(object sender, EventArgs e)
        {
            timer2_stop_delay.Enabled = false;
            start_recv_sample_data_flag = 0;//延时停止接收,防止一些数据丢失
            //---读取采集结果-----
            byte[] message = System.Text.Encoding.Default.GetBytes("set read_param");
            Console.WriteLine(System.Text.Encoding.Default.GetString(message));
            tttp.Send(client_fd, message);

            button3.Enabled = false;
        }

        private void timer_work_Tick(object sender, EventArgs e)
        {
            button3_Click(sender, e);
            timer_work.Enabled = false;
        }

        private void timer2_delay_save_text_Tick(object sender, EventArgs e)
        {
            timer2_delay_save_text.Stop();
            Console.WriteLine("timer2_delay_save_text " + textBox1.Text);
            try
            {

                //根据INI文件名设置要写入INI文件的节点名称
                //此处的节点名称完全可以根据实际需要进行配置

                WritePrivateProfileString(strSec, "Name", textBox1.Text.Trim(), strFilePath);

            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());

            }
        }

        private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            timer2_delay_save_text.Stop();
            timer2_delay_save_text.Interval = 3000;
            timer2_delay_save_text.Start();
            Console.WriteLine("textBox1_KeyPress reset timer " + textBox1.Text);
        }

   


    }
}
