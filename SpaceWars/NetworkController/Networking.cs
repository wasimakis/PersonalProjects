/// SpaceWars.NetworkerController.Networking   
/// CS 3500 PS7
/// Original Authors:   William Asimakis    Joshua Call
/// Version 1.0 (11/06/2018)
///     -Created the class, members, and variables 
///Current: Version 2.0 (11/28/2018) 
///     -Added server functionalities
/// TODO: Implement Sending and SendCallback functions
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkController
{

    /// <summary>
    /// Stores the TcpListener and network callback action for different networking server based functions
    /// </summary>
    public class ConnectionState
    {
        private TcpListener lstn;
        private NetworkAction action;

        public TcpListener Lstn { get => lstn; set => lstn = value; }
        public NetworkAction Action { get => action; set => action = value; }
    }
    /// <summary>
    /// This class provides a number of generic helper methods to help with networking.
    /// </summary>
    public static class Networking
    {
        /// <summary>
        /// Delegate used to do something if there is a networking error. 
        /// </summary>
        /// <param name="e"></param>
        public delegate void NetworkingError();
        /// <summary>
        ///Delegate used to do something if there is a networking error specifically used for Server based execution 
        /// </summary> 

        public static event NetworkingError connectionFailed;

        public static event NetworkAction serverFail;

        public const int DEFAULT_PORT = 11000;
        /// <summary>
        /// Initiates a connection to the server and begins Connection process.
        /// </summary>
        /// <param name="callMe">A function that the Socket State will call when a message is gathered</param>
        /// <param name="hostname">the IP</param>
        /// <returns>A socket that has been initialized with the hostname</returns>
        public static Socket ConnectToServer(NetworkAction callMe, string hostName)
        {
            Console.WriteLine("Attempting connection with " + hostName);

            Networking.MakeSocket(hostName, out Socket socket, out IPAddress ipAddress);

            SocketState currState = new SocketState(socket, 0, callMe);

            // System.Diagnostics.Debug.WriteLine("Beginning connection...");

            currState.TheSocket.BeginConnect(ipAddress, Networking.DEFAULT_PORT, Networking.ConnectedCallback, currState);

            return socket;
        }

        private static void ConnectedCallback(IAsyncResult stateAsArObject)
        {
            //System.Diagnostics.Debug.WriteLine("....Contact from Server");

            SocketState currState = stateAsArObject.AsyncState as SocketState;
            //Complete the connection. Test for a connection. 
            try
            {

                currState.TheSocket.EndConnect(stateAsArObject);

                //System.Diagnostics.Debug.WriteLine("Server connection ended.");
                //Invoke the Network Function of the current socket state (this function delegate should change after first contact with server).  
                //System.Diagnostics.Debug.WriteLine("Invoking networking function..."); 
                currState.NetworkFunction(currState);
            }
            catch (Exception e)
            {
                // System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                connectionFailed();
                return;
            }
            // System.Diagnostics.Debug.WriteLine("Begin recieving data from server....");
            //Start an event loop to recieve data from the server 
            currState.TheSocket.BeginReceive(currState.MessageBuffer, 0, currState.MessageBuffer.Length, SocketFlags.None, RecieveCallback, currState);
        }
        /// <summary> 
        /// This method will be invoked through BeginRecieve at the time a message is recieved from the server. 
        /// </summary>
        /// <param name="stateAsArObject"></param>
        private static void RecieveCallback(IAsyncResult stateAsArObject)
        {
            SocketState currState = stateAsArObject.AsyncState as SocketState;
            int incomingBytes = 0;
            if (!currState.TheSocket.Connected)
            {
                if (!currState.ForServer)
                    throw new SocketException();
                serverFail(currState);
                return;
            }
            try
            {
                incomingBytes = currState.TheSocket.EndReceive(stateAsArObject);
            }
            catch (Exception)
            {
                currState.TheSocket.Shutdown(SocketShutdown.Both);
                currState.TheSocket.Close();
                serverFail(currState);

            }
            //If the socket is still open
            if (incomingBytes > 0)
            {

                string incomingMessage = Encoding.UTF8.GetString(currState.MessageBuffer, 0, incomingBytes);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece  
                currState.MessageState.Append(incomingMessage);

                //Invoke the Network Function of the current socket state.  
                currState.NetworkFunction(currState);
            }
        }
        /// <summary>
        /// Requests more data from a Socket 
        /// </summary>
        /// <param name="currState">The socket state that contains the socket to ask for more data</param>
        public static void GetData(SocketState currState)
        {
            // Start listening for more parts of a message, or more new messages
            currState.TheSocket.BeginReceive(currState.MessageBuffer, 0, currState.MessageBuffer.Length, SocketFlags.None, RecieveCallback, currState);
        }

        /// <summary>
        /// Sends data over a Socket (Note: data does not have to have "\n" appended at the end, this function will do that for you))
        /// </summary>
        /// <param name="socket">The socket to send data over</param>
        /// <param name="data">The data to send</param>
        public static void Send(Socket socket, String data)
        {
            try
            {
                byte[] sendingBytes = Encoding.UTF8.GetBytes(data + "\n");
                socket.BeginSend(sendingBytes, 0, sendingBytes.Length, SocketFlags.None, SendCallback, socket);
            }
            catch (SocketException)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Socket disconnected. Shutting Down");   
            }
           

        }
        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                socket.EndSend(ar);
            }
            catch (Exception) {
                //TODO: Handle
            }
        }


        /// <summary>
        /// Creates a Socket object for the given host String 
        /// Method content and signature credited to Daniel Kopta
        /// </summary>
        /// <param name="hostName">The host name or IP address</param>
        /// <param name="socket">The created Socket</param>
        /// <param name="ipAddress">The created IPAddress</param>
        public static void MakeSocket(string hostName, out Socket socket, out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None;
            socket = null;
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        // System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        throw new ArgumentException("Invalid address");
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    // System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // Disable Nagle's algorithm - can speed things up for tiny messages, 
                // such as for a game
                socket.NoDelay = true;

            }
            catch (Exception e)
            {
                // System.Diagnostics.Debug.WriteLine("Unable to create socket. Error occured: " + e);
                throw e;
            }
        }

        /// <summary>
        /// Starts a tcplistener for new connections and passes the listneer along with the handleNewClient to a BeginAcceptSocket state parameter. Upon a connection 
        /// request coming in, the OS should invoke the AcceptNewClient as the callback method. 
        /// </summary>
        /// <param name="HandleNewClient"></param>
        public static void ServerAwaitingClientLoop(NetworkAction HandleNewClient)
        {
            //Instantiate a new tcp listener. 
            TcpListener lstn = new TcpListener(IPAddress.Any, Networking.DEFAULT_PORT);

            lstn.Start();

            ConnectionState cs = new ConnectionState();
            cs.Lstn = lstn;
            cs.Action = HandleNewClient;
            lstn.BeginAcceptSocket(AcceptNewClient, cs);

        }
        /// <summary>
        /// This is the callback that BeginAcceptSocket uses.  It will be invoked by the OS when a connection request comes in. 
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            ConnectionState cs = ar.AsyncState as ConnectionState;
            Socket socket = cs.Lstn.EndAcceptSocket(ar);
            SocketState ss = new SocketState(socket, 0, cs.Action);
            ss.ForServer = true;
            ss.NetworkFunction(ss);
            cs.Lstn.BeginAcceptSocket(AcceptNewClient, cs);


        }
    }
}

