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
namespace tcp_server
{
    public partial class Form1 : Form
    {
        private static int all_recv_num = 0;
        public Form1()
        {
            InitializeComponent();
            var str = Console.ReadLine();
            Thread workThread = new Thread(new ParameterizedThreadStart(ThreadWork_start));
            workThread.Start(str);
            label2.Text = all_recv_num.ToString();
            label6.Text = "192.168.0.112:6800";
            all_button_set(false);
            this.Text = "200k pc tools--版本号1.01.01";
        }
        /* Initializes the Listener */

        static AsyncTCPServer tttp = new AsyncTCPServer(IPAddress.Parse("192.168.0.112"), 6800);
        static TCPClientState client_fd;
        static byte[] recv_data_globa = new byte[1000 * 400 * 120+100];//120s,200k short =400k byte 
        static int recv_data_globa_len = 0;
        static int ack_count = 0;//发送数据以后等待接收
        private static void DataReceived_callback(object os, AsyncEventArgs state)
        {
            //优先判断是否有应答消息
            if (ack_count > 0)
            {
                ack_count = 0;
                //byte[] msg = new byte[state._state.buffer_data_size];
                label4_text = System.Text.Encoding.Default.GetString(state._state.Buffer, 0, state._state.buffer_data_size);
                do_update_type |= 0x0004;
                return;
            }
            //保存数据到内存
            all_recv_num = all_recv_num + state._state.buffer_data_size;
            do_update_type = do_update_type | 0x01; 
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
        private void button4_Click(object sender, EventArgs e)
        {
            string file_name = DateTime.Now.ToLocalTime().ToString("yyyy_MM_dd_hh_mm_ss")+"_200k.txt";
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
                int tmp = recv_data_globa[i * 2] + recv_data_globa[i * 2 + 1] * 256;
                tmp = (tmp & 0x3fff) >> 2;
                //sw.WriteLine(recv_data_globa[i * 2].ToString("x2") + recv_data_globa[i * 2 + 1].ToString("x2"));                //写入一行文本 
                sw.WriteLine(tmp.ToString());                //写入一行文本 
                
            }
            sw.Flush();                    //清空 
            sw.Close();                    //关闭 
            label4_text = "保存成功,"+file_name;
            do_update_type |= 0x0004;
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
        private static void clientconnect_callback(object os, AsyncEventArgs state)
        {
            label4_text = "connect:"+state._state.TcpClient.Client.RemoteEndPoint.ToString();
            do_update_type = do_update_type |0x0002;
            client_fd = state._state;//保存client信息,发送会用到
            
        }
        private static void clientdisconnect_callback(object os, AsyncEventArgs state)
        {
            label4_text = "disconnect";
            do_update_type = do_update_type | 0x0002;
        }


      
        private void button1_Click_1(object sender, System.EventArgs e)
        {
            all_recv_num = 0;
            label2.Text = all_recv_num.ToString();
            recv_data_globa_len = 0;
        }
        private static UInt32 do_update_type = 0;
        private static string label4_text = "";
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            UInt32 tmp;
            tmp = do_update_type & 0x0001;
            if (tmp > 0)
            {
                label2.Text = all_recv_num.ToString();
            }
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
                label7.Text = "消息状态:" + label4_text;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //tServer.Stop();
            tttp.Start();
        }
        

        private void button2_Click(object sender, System.EventArgs e)
        {
            recv_data_globa_len = 0;//清零接收缓存
            //client_fd
            byte[] message = System.Text.Encoding.Default.GetBytes("set start");
           
            tttp.Send(client_fd, message);

            //Send(client_fd, "hello worl");
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            ack_count = 1;
            //client_fd
            if(radioButton1.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=10000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if(radioButton2.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=20000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if(radioButton3.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=30000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if(radioButton4.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=60000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if(radioButton5.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=120000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if(radioButton6.Checked)
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=240000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if (radioButton7.Checked)// 5s
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=5000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }
            else if (radioButton8.Checked)//1s
            {
                byte[] message = System.Text.Encoding.Default.GetBytes("set long=1000");
                Console.WriteLine(System.Text.Encoding.Default.GetString(message));
                tttp.Send(client_fd, message);
            }

        }
        public void all_button_set(bool flag)
        {
            if (flag)
            {
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {

                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }


    }
}
