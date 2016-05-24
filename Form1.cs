/*
CS408 Term Project - Step 1
Creators :  Öykü Kapcak
            Berke Ataç
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RPS_Step1_Client
{
    public partial class Form1 : Form
    {
        bool terminating = false; 
        Socket client;
        Thread thrReceive;

        string serverIP;
        int serverPort;
        string clientName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
            button4.Enabled = false;
            button3.Enabled = false;
        }

        //on button click connects to the server with given ip and port no
        private void button1_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //takes inputs from textboxes and updown value box
            serverIP = textBox1.Text;
            serverPort = (int)numericUpDown1.Value;
            clientName = textBox2.Text;

            //connects user to server using sockets
            client.Connect(serverIP, serverPort);

            //starts a thread with receive function
            thrReceive = new Thread(new ThreadStart(Receive));
            thrReceive.IsBackground = true;
            thrReceive.Start();

            //Sends the name of the client to server
            SendName(clientName);

            //disables the connect button
            button1.Enabled = false;
            button3.Enabled = true;
        }

        void SendMessage(string message)
        {
            //creates an array of bytes from the string input and proceeds to send it
            byte[] buffer = Encoding.Default.GetBytes(message);
            client.Send(buffer);
            richTextBox1.Text+=("Your message has been sent.\n");
        }

        //this function sends the name of the client once it is connected.
        void SendName(string message)
        {
            //creates an array of bytes from the string input and proceeds to send it
            byte[] buffer = Encoding.Default.GetBytes(message);
            client.Send(buffer);
        }

        //function is used in a thread and constantly checks for incoming buffers
        private void Receive()
        {
            bool connected = true;
            richTextBox1.Text+=("Connected to the server.\n");
            
            while (connected)
            {
                //while the connection has not ended
                try
                {
                    //checks for incoming buffers
                    byte[] buffer = new byte[256];

                    int rec = client.Receive(buffer);

                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }

                    //decodes them to strings and gives output with the incoming string
                    string newmessage = Encoding.Default.GetString(buffer);
                    newmessage = newmessage.Substring(0, newmessage.IndexOf("\0"));
                    richTextBox1.Text+=("Server: " + newmessage + "\r\n");
                    if(newmessage == "Game has started!\n")
                    {
                        button4.Enabled = true;
                    }
                    if (newmessage.Length > 10 && newmessage.Substring(0,10) == "Game Over.")
                    {
                        button4.Enabled = false;
                    }

                }
                catch
                {
                    if (!terminating)
                        richTextBox1.Text+=("Connection has been terminated...\n");
                    connected = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string message = textBox3.Text; //takes the string from textbox
    
            try
            {
                //on button click calls the message sending function with the input from text box
                SendMessage(message);
            }
            catch
            {
                richTextBox1.Text += ("Your message could not be sent.\n");
                richTextBox1.Text += ("connection lost...");
                terminating = true;
                client.Close();
            }

        }
        

        //on button click disconnects the user closing the socket
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text += ("You have disconnected.\n");
            terminating = true;
            client.Close();

            button3.Enabled = false;
            button1.Enabled = true;

            thrReceive.Abort();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendMessage("SURRENDER");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            terminating = true;
            client.Close();
        }
    }
}