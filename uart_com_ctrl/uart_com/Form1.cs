using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace uart_com
{
    public partial class Form1 : Form
    {
      
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            disable_ctrl_button();
            //--------------扫描com-------------------
            string[] ArryPort = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            for (int i = 0; i < ArryPort.Length; i++)
            {
                comboBox1.Items.Add(ArryPort[i]);
            }
            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Items.Add("None");
            }
            comboBox1.SelectedIndex = 0;          
        //--------------------波特率-------------------
            comboBox2.Items.Add("300");
            comboBox2.Items.Add("1200");
            comboBox2.Items.Add("4800");
            comboBox2.Items.Add("9600");
            comboBox2.Items.Add("19200");
            comboBox2.Items.Add("38400");
            comboBox2.Items.Add("43000");
            comboBox2.Items.Add("56000");
            comboBox2.Items.Add("57600");
            comboBox2.Items.Add("115200");
            comboBox2.SelectedIndex = 3;  
        
            

        }
        public int open_com()
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text.Trim());//115200;
                serialPort1.PortName = comboBox1.Text.Trim();//"COM1";
                serialPort1.DataBits = 8;
                serialPort1.Open();//打开串口
            }
     
            return 0;
        }
        public int close_com()
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();//打开串口
            }
          
            return 0;
        }
        private void disable_ctrl_button()
        {
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
       
        }
        private void enable_ctrl_button()
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "打开")
            {
                button1.Text = "关闭";
                open_com();
                enable_ctrl_button();
                timer1.Enabled = true;
                time_count = 0;
            }
            else
            {
                button1.Text = "打开";
                close_com();
                disable_ctrl_button();
                timer1.Enabled = false;
                time_count = 0;
            }
        }



        int buffer_count = 0;//接收到数据的长度
        byte[] buffer_b = new byte[2048];//接收数据缓冲区
        int time_count = 0;//接收到最后一个byte后的多少ms处理数据
        void deal_ReceiveDate(int bytedata)
        {
            buffer_b[buffer_count++] = (byte)bytedata;
            time_count = 0;
            
            
           //显示
            //listBox1.Invoke(
            //    new EventHandler(
            //        delegate
            //        {
            //            listBox1.Items.Add(System.Text.Encoding.ASCII.GetString(buffer_b, buffer_count-1, 1)); 
            //        }
            //        )
            //    );
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
           while (serialPort1.BytesToRead > 0)
           {
               int SDateTemp = this.serialPort1.ReadByte();
               //接收处理数据
               deal_ReceiveDate(SDateTemp);
           }
          
           
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (buffer_count > 0)
            {
                time_count++;
                if (time_count > 10)//超时30ms没收到新的数据
                {

                    string recv_str = System.Text.Encoding.ASCII.GetString(buffer_b, 0, buffer_count);
                    Console.Write(recv_str);//控制台输出打印
                    textBox1.Text = textBox1.Text+recv_str;
                  //  label4.Text = recv_str;
                    parse_data(recv_str);
                    buffer_count = 0;
                    time_count = 0;
                }

            }
        }
        public void parse_data(string data)
        {

            string[] strarr = data.Split('\n');
            for (int i = 0; i < strarr.Length; i++ )
            {
                string cmd_str = strarr[i];
                cmd_str = cmd_str.Replace('\r',' ');
                cmd_str = cmd_str.Trim();
                string[] cmd_arr = cmd_str.Split(':');
                if (cmd_arr[0] == "cmd" && cmd_arr[1] == "up")
                {
                    Console.Write("cmd_arr[2]="+cmd_arr[2]);//控制台输出打印
                    if (cmd_arr[2] == "voltage0")
                    {
                        try
                        {
                            int vol = Int32.Parse(cmd_arr[3]);
                            vol = (vol * 1000 / 255) * 5;
                            label_vol.Text = vol.ToString();
                        }
                        catch
                        { }
                        
                    }
                    else if (cmd_arr[2] == "voltage1")
                    {
                        try
                        {
                            int vol = Int32.Parse(cmd_arr[3]);
                            vol = (vol * 1000 / 255) * 5;
                            label_vol1.Text = vol.ToString();
                        }
                        catch
                        { }

                    }
                }
            }
                
        }

        
        private void button2_Click_1(object sender, EventArgs e)
        {
            byte[] send_byte = System.Text.Encoding.ASCII.GetBytes("cmd51:leftrun") ;
            serialPort1.Write(send_byte, 0, send_byte.Length);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] send_byte = System.Text.Encoding.ASCII.GetBytes("cmd51:ledon");
            serialPort1.Write(send_byte, 0, send_byte.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] send_byte = System.Text.Encoding.ASCII.GetBytes("cmd51:ledoff");
            serialPort1.Write(send_byte, 0, send_byte.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] send_byte = System.Text.Encoding.ASCII.GetBytes("cmd51:rightrun");
            serialPort1.Write(send_byte, 0, send_byte.Length);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }





    }
}
