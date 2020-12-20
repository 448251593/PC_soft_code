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
            

            button_send.Enabled = false;
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
            comboBox2.SelectedIndex = 0;  
         //------------------数据位--------------------
            comboBox3.Items.Add("8");
            comboBox3.Items.Add("7");
            comboBox3.Items.Add("6");
            comboBox3.Items.Add("5");

            comboBox3.SelectedIndex = 0;
         //------------------停止位---------------------
            comboBox4.Items.Add("1");
            comboBox4.Items.Add("1.5");
            comboBox4.Items.Add("2");            
            comboBox4.SelectedIndex = 0; 
         //-----------------奇偶校验--------------------
            comboBox5.Items.Add("none");
            comboBox5.Items.Add("add");
            comboBox5.Items.Add("even");
            comboBox5.Items.Add("mark");
            comboBox5.Items.Add("space");            
            comboBox5.SelectedIndex = 0; 
            

        }
        public int open_com()
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text.Trim());//115200;
                serialPort1.PortName = comboBox1.Text.Trim();//"COM1";
                serialPort1.DataBits = Convert.ToInt32(comboBox3.Text.Trim());
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

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "打开")
            {
                button1.Text = "关闭";
                open_com();
                button_send.Enabled = true;
            }
            else
            {
                button1.Text = "打开";
                close_com();
                button_send.Enabled = false;
            }
        }
        //public delegate void tB_ReceiveDate(string str);
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int SDateTemp = this.serialPort1.ReadByte();
           
            //读取串口中一个字节的数据  
            this.textBox2.Invoke(
                //在拥有此控件的基础窗口句柄的线程上执行委托Invoke(Delegate)  
                //即在textBox_ReceiveDate控件的父窗口form中执行委托.  
                 new MethodInvoker(
                    /*表示一个委托，该委托可执行托管代码中声明为 void 且不接受任何参数的任何方法。 在对控件的 Invoke    方法进行调用时或需要一个简单委托又不想自己定义时可以使用该委托。*/
                     delegate
                     {
                         /*匿名方法,<a href="http://lib.csdn.net/base/csharp" class='replace_word' title="C#知识库" target='_blank' style='color:#df3434; font-weight:bold;'>C#</a>2.0的新功能，这是一种允许程序员将一段完整代码区块当成参数传递的程序代码编写技术，通过此种方法可    以直接使用委托来设计事件响应程序以下就是你要在主线程上实现的功能但是有一点要注意，这里不适宜处理过多的方法，因为C#消息机制是消息流水线响应机制，如果这里在主线程上处理语句的时间过长会导致主UI线程阻塞，停止响应或响应不顺畅,这时你的主form界面会延迟或卡死      */

                        /// this.textBox2.AppendText("0x"+String.Format("{0:x00}", SDateTemp)+" ");//输出到主窗口文本控件 
                         if(check_show_hex.Checked)
                         {
                             this.textBox2.AppendText(SDateTemp.ToString("x2") + " ");//输出到主窗口文本控件  
                         }
                         else
                         {
                             byte[] tmp_b = new byte[1];
                             tmp_b[0] =(byte) SDateTemp;
                             this.textBox2.AppendText(System.Text.Encoding.ASCII.GetString(tmp_b));//输出到主窗口文本控件  
                         }

                         //    this.textBox2.Items.Add += " ";
                     }
                 )
             );  
        }

        private void button2_Click(object sender, EventArgs e)
        {           
            if (checkbox_hex_flag.Checked)
            {
               
                try
                {
                    String send_str = textBox1.Text.Trim();
                    String[] byte_arr = send_str.Split(' ');

                    byte[] send_byte = new byte[byte_arr.Length];
                    for (int i = 0; i < byte_arr.Length; i++)
                    {
                        send_byte[i] = byte.Parse(byte_arr[i],System.Globalization.NumberStyles.HexNumber);
                     
                    }
                    serialPort1.Write(send_byte, 0, byte_arr.Length);
                }
                catch
                {
                    MessageBox.Show("发送数据格式以空格隔开!");
                }
            }
            else
            {
               
                String send_str = textBox1.Text.Trim();

                byte[] send_byte = System.Text.Encoding.Default.GetBytes(send_str);

                serialPort1.Write(send_byte, 0, send_byte.Length);
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void checkbox_hex_flag_MouseHover(object sender, EventArgs e)
        {
            ToolTip tooltip = new ToolTip();
            tooltip.Show("文本框输入数字,以空格分割", button_send, 3000);
        }

    }
}
