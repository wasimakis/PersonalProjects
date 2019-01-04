/// SpaceWars.NetworkerController.SocketState   
/// CS 3500 PS7
/// Original Authors:   William Asimakis    Joshua Call
/// Version 1.0 (11/05/2018)
///     -Created the class, members, and variables for Socket State 
/// Current: Version 2.0 (11.28/2018) 
///     -Added Server boolean needed for RecieveName callbacks
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace NetworkController
{
    //Function to be called when a connection is made.  The Socket state needs to be passed to extract important useful information from the Socket. 
    public delegate void NetworkAction(SocketState m);


    /// <summary>
    /// Socket State encapsulates all the information (Socket, Buffer, StringBuidler) needed for Network callbacks
    /// </summary>
    public class SocketState
    {

        //An appropriate buffer size for recieving data from a Server
        private const int DEFAULT_BUFFER = 8000; 

        //The actual connection state of the server/client relationship 
        private Socket theSocket; 

        //The location of the raw bytes recieved from the server. 
        private byte[] messageBuffer = new byte[SocketState.DEFAULT_BUFFER];

        //A "growable" buffer that contains contextual byte-to-String data
        private StringBuilder messageState;

        //A function delegate that will do something with the completedMessage if it is ready. 
        private NetworkAction networkFunction;

        //Socket ID
        private int id;

        //Utilization for Server connection management 
         private bool forServer; 

        public NetworkAction NetworkFunction { get => networkFunction; set => networkFunction = value; }

        public StringBuilder MessageState { get => messageState; set => messageState = value; }

        public int ID { get => id; set => id = value; }

        public SocketState(Socket s, int ID, NetworkAction callMe)
        {
            this.id = ID;
            this.theSocket = s;
            messageState = new StringBuilder();
            networkFunction = callMe;
        }

        public Socket TheSocket
        {
            get
            {
                return theSocket;
            }
            set
            {
                theSocket = value;
            }
        }

        public byte[] MessageBuffer
        {
            get
            {
                return messageBuffer;
            }
            private set
            {
                messageBuffer = value;
            }
        }

        public bool ForServer { get => forServer; set => forServer = value; }
    }
}
