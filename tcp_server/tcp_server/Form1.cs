using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace tcp_server
{
    public partial class Form1 : Form
    {
        public static ManualResetEvent TcpListenerEndEvent = new ManualResetEvent(false);
        public static ManualResetEvent TcpClientClosedEvent = new ManualResetEvent(false);

        public Form1()
        {
            InitializeComponent();
            var str = Console.ReadLine();
            Thread workThread = new Thread(new ParameterizedThreadStart(ThreadWork_start));
            workThread.Start(str);
            label2.Text = all_recv_num.ToString();
        }
        /* Initializes the Listener */
        static TcpListener tServer = new TcpListener(IPAddress.Parse("192.168.0.112"), 6800);

        private static void ThreadWork_start(object param)
        {
            StartServer();


        }
     
        private static void StartServer()
        {
            try
            {
                ///* Initializes the Listener */
                //TcpListener tServer = new TcpListener(IPAddress.Parse("192.168.0.112"), 6800);

                /* Start Listeneting at the specified port */
                tServer.Start();

                TcpListenerEndEvent.Reset();
                
                while (!TcpListenerEndEvent.WaitOne(0))
                {
                    BeginAccecptATcpClient(tServer);
                }
                //Console.WriteLine(string.Concat(tServer.LocalEndpoint, " will been termined!"));

                //tServer.Stop();

                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }


        }
        private static void BeginAccecptATcpClient(TcpListener tServer)
        {
            // Set the event to nonsignaled state.
            TcpClientClosedEvent.Reset();

            tServer.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), tServer);

            TcpClientClosedEvent.WaitOne();
        }

        private static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpClient client = null;
            try
            {
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                client = listener.EndAcceptTcpClient(ar);

                if (client != null)
                {
                    if (client.Client != null)
                    {
                        Console.WriteLine("Connection accepted from " + client.Client.RemoteEndPoint);
                        label4_text = "Connection accepted from " + client.Client.RemoteEndPoint;
                        do_update_type = do_update_type | 0x02;
                    }
                    Thread th = new Thread(new ParameterizedThreadStart(ThreadWork));
                    th.Start(client);
                }
            }
            catch (SocketException se) { Console.WriteLine(se.StackTrace); }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            finally { }
        }
       

        private const uint SIZE_OF_READ_BUFFER = 10240;
        private const long SIZE_OF_MAX_READ = 200;
        private static int all_recv_num = 0;
        private static void ThreadWork(object param)
        {
            TcpClient s = param as TcpClient;
            NetworkStream stream = s.GetStream();
            try
            {
                byte[] b = new byte[SIZE_OF_READ_BUFFER];
                int numberOfBytesRead = 0;
                //MemoryStream dummyByteStream = new MemoryStream();
                //long bytesToRead = SIZE_OF_MAX_READ;
                do
                {
                    byte[] buffer = new byte[1];
                    int bytesPeeked = s.Client.Receive(buffer, SocketFlags.Peek);
                    if (bytesPeeked == 0)
                    {
                        //s.Client.Close();
                        break;
                    }
                    else if (stream.DataAvailable)
                    {

                        int len = (int)SIZE_OF_READ_BUFFER;
                        numberOfBytesRead = stream.Read(b, 0, len);
                        Console.WriteLine("numberOfBytesRead=" + numberOfBytesRead);
                        all_recv_num = all_recv_num + numberOfBytesRead;
                        do_update_type = do_update_type | 0x01;
                        //Console.WriteLine(b.ToString());
                        for (int i = 0; i < numberOfBytesRead; i++ )
                        {
                            Console.Write(b[i].ToString("x2") + ",");  //法1
                        }
                        Console.WriteLine("\n");
                        //dummyByteStream.Write(b, 0, numberOfBytesRead);
                    }

                    //if (dummyByteStream.Length == 4)
                    //{
                    //    // header complete, get message length and read rest of message
                    //    byte[] header = dummyByteStream.ToArray();

                    //    bytesToRead = header[0];
                    //    bytesToRead += header[1] * 0x100;
                    //    bytesToRead += header[2] * 0x10000;
                    //    bytesToRead += header[3] * 0x1000000;
                    //    bytesToRead += 5; // length + ending null
                    //    // Signal the calling thread to continue.
                    //}

                    //if (dummyByteStream.Length > 0)
                    //{
                    //    byte[] received = dummyByteStream.ToArray();
                    //    var output = Encoding.GetEncoding(1252).GetString(received);
                    //    if ("exit".Equals(output, StringComparison.InvariantCultureIgnoreCase))
                    //        TcpListenerEndEvent.Set();
                    //    Console.WriteLine(string.Concat("Rec client: ", output));
                    //}

                } while (true);
                //} while (stream.DataAvailable && bytesToRead > dummyByteStream.Length);

                // Process the connection here. (Add the client to a
                // server table, read data, etc.)
                //Console.WriteLine("Client connected completed");
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.StackTrace);
            }
            finally
            {
                label4_text = "disConnection ";
                do_update_type = do_update_type | 0x02;

                TcpClientClosedEvent.Set();
                stream.Close();
                s.Close();
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            all_recv_num = 0;
            label2.Text = all_recv_num.ToString();
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
                label4.Text = label4_text;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tServer.Stop();
        }
        private  void stop_tcp_server()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 6800);

            NetworkStream ns = tcpClient.GetStream();
            if (ns.CanWrite)
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Exit");
                ns.Write(sendBytes, 0, sendBytes.Length);
                //lbMsg.Items.Add("发送退出命令成功！");
            }
            else
            {
                //lbMsg.Items.Add("发送退出命令失败！");
                return;
            }
            ns.Close();
            tcpClient.Close();
        }
    }
}
