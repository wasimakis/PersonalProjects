using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NetworkController;
using Positioning;
using Setting;
using WorldObjects;
using Newtonsoft.Json;
using System.Xml;
using System.IO;

namespace Server
{
    class SpaceServer
    {
        /// <summary>
        /// A dictionary to store valued Socket State keyed to their corresponding IDs. 
        /// </summary>
        private Dictionary<int, SocketState> gameClients;

        /// <summary>
        /// Active world information constantly updated and sent to gameClients
        /// </summary>
        private volatile World world;

        /// <summary>
        /// Read from an xml setting file. 
        /// </summary>
        private int MSPerFrame;

        private int startingHp;
        /// <summary>
        /// Client ID counter
        /// </summary>
        /// <param name="args"></param> 
        private static int currClientID = -1;
        /// <summary>
        /// Ship ID counter
        /// </summary>
        /// <param name="args"></param> 
        private static int currShipID = -1;

        /// <summary>
        /// OS path to the XML file to determine server settings
        /// </summary>
        private static string pathToSettings = @"..\..\..\Resources\server_settings.xml";

        /// <summary>
        /// Stringbuidler that represents a completed world string
        /// </summary> 
        StringBuilder worldMessage;

        public int MSPerFrame1 { get => MSPerFrame; set => MSPerFrame = value; }
        public int StartingHp { get => startingHp; set => startingHp = value; }


        //Program main entry point for the server
        static void Main(string[] args)
        {
            SpaceServer theServer = new SpaceServer();
            Console.WriteLine("Press 1 to start the server.");
            string input = Console.ReadLine();
            while (!input.Equals("1"))
            {
                Console.WriteLine("Press 1 to start the server.");
                input = Console.ReadLine();
            }
            theServer.StartServer();
        }

        private bool ReadSettings(string filepath, out string errorMessage)
        {
            try
            {
                double accel = 0.0;
                double mass = 0.0;
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    bool isServerSetting = false;
                    bool isStarSetting = false;
                    bool isProjSetting = false;
                    bool isShipSetting = false;
                    bool isWorldSetting = false;
                    while (reader.Read())
                    {
                        //Only detect start elements
                        if (reader.IsStartElement())
                        {
                            //Object to determine what to put into what
                            object toSet = new object();
                            //if the starting element is a ServerSetting, we know we are starting to acquire ServerSettings from 
                            //the XML
                            if (reader.Name.Equals("ServerSettings"))
                            {
                                isServerSetting = true;
                                continue;
                            }
                            //If we are within the server settings section of the XML
                            if (isServerSetting)
                            {
                                //Update the current section to equate condtions
                                if (reader.Name.Equals("StarSettings"))
                                {
                                    isStarSetting = true;
                                    isProjSetting = false;
                                    isShipSetting = false;
                                    isWorldSetting = false;
                                }
                                if (reader.Name.Equals("ProjSettings"))
                                {
                                    isStarSetting = false;
                                    isProjSetting = true;
                                    isShipSetting = false;
                                    isWorldSetting = false;
                                }
                                if (reader.Name.Equals("ShipSettings"))
                                {
                                    isStarSetting = false;
                                    isProjSetting = false;
                                    isShipSetting = true;
                                    isWorldSetting = false;
                                }
                                if (reader.Name.Equals("WorldSettings"))
                                {
                                    isStarSetting = false;
                                    isProjSetting = false;
                                    isShipSetting = false;
                                    isWorldSetting = true;
                                }
                                if (isWorldSetting)
                                {
                                    //Insure its a starting element
                                    if (reader.IsStartElement())
                                    {
                                        //Since we know its within worldSettings, find the right element tag name
                                        switch (reader.Name)
                                        {
                                            case "Enhanced":
                                                world.Enhanced = reader.ReadElementContentAsBoolean();
                                                break;
                                            case "StarDelay":
                                                world.StarDelay = reader.ReadElementContentAsInt();
                                                break;
                                            case "StarCounterEnd":
                                                world.StarCounterEnd = reader.ReadElementContentAsInt();
                                                break;
                                            case "StartingStarAmount":
                                                world.StartingStarAmount = reader.ReadElementContentAsInt();
                                                break;
                                            case "ProjFiringDelay":
                                                world.FrameMax = reader.ReadElementContentAsInt();
                                                break;
                                            case "RespawnDelay":
                                                world.RespawnDelay = reader.ReadElementContentAsInt();
                                                break;
                                            case "UniverseSize":
                                                world.WorldSize = reader.ReadElementContentAsInt();
                                                break;
                                            case "BaseFreq":
                                                world.BaseFreq = reader.ReadElementContentAsInt();
                                                break;

                                        }
                                    }
                                }
                                if (isStarSetting)
                                {
                                    if (reader.IsStartElement())
                                    {
                                        switch (reader.Name)
                                        {
                                            case "Radius":
                                                Star.StarRad = reader.ReadElementContentAsInt();
                                                break;
                                            case "Accel":
                                                accel = reader.ReadElementContentAsDouble();
                                                break;
                                            case "Mass":
                                                mass = reader.ReadElementContentAsDouble();
                                                break;
                                        }
                                    }
                                }
                                if (isShipSetting)
                                {

                                    if (reader.IsStartElement())
                                    {
                                        switch (reader.Name)
                                        {
                                            case "EngineStrength":
                                                Ship.EngineStrength = reader.ReadElementContentAsDouble();
                                                break;
                                            case "TurnRate":
                                                Ship.TurnRate = reader.ReadElementContentAsInt();
                                                break;
                                            case "StartingHP":
                                                StartingHp = reader.ReadElementContentAsInt();
                                                break;

                                        }
                                    }
                                }
                                if (isProjSetting)
                                {
                                    if (reader.IsStartElement())
                                    {
                                        switch (reader.Name)
                                        {
                                            case "Speed":
                                                Projectile.ProjSpeed = reader.ReadElementContentAsInt();
                                                break;
                                        }
                                    }
                                }
                                if (reader.Name.Equals("MSFrame"))
                                {
                                    MSPerFrame = reader.ReadElementContentAsInt();
                                }
                            }

                        }
                    }
                }
                for (int n = 1; n < world.StartingStarAmount; n++)
                {
                    world.Stars.Add(Star.GetStarID(), new Star(Star.GetStarID(), new Vector2D(world.WorldSize, world.WorldSize), mass, accel));
                }
            }
            catch (IOException e)
            {
                errorMessage = e.Message;
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Incremental System to auto-increment client IDs
        /// </summary>
        /// <returns></returns>
        private static int GetClientID()
        {
            if (currClientID == Int32.MaxValue)
            {
                currClientID = 0;
                return currClientID;
            }
            currClientID++;
            return currClientID;
        }

        /// <summary>
        /// Incremental System to auto-increment player ship IDs
        /// </summary>
        /// <returns></returns>
        private static int GetShipID()
        {
            if (currShipID == Int32.MaxValue)
            {
                currShipID = 0;
                return currShipID;
            }
            currShipID++;
            return currShipID;
        }

        public SpaceServer()
        {
            gameClients = new Dictionary<int, SocketState>();
            world = new World();
            Networking.serverFail += ClientDisconnect;
            worldMessage = new StringBuilder();
            //Error in reading the IO. 
            while (!ReadSettings(pathToSettings, out string errorMessage))
            {
                //Reset the stars
                world.Stars = new Dictionary<int, Star>();
                Console.WriteLine("Error occured when reading file: {0}", errorMessage);
                Console.WriteLine("Edit XML settings before retry! Press 1 to try again");
                string input = Console.ReadLine();
                while (!input.Equals("1"))
                {
                    Console.WriteLine("Press 1 to start the server.");
                    input = Console.ReadLine();
                }
            }
            //TODO-----------------TEST VARIABLES THAT ARE NOT ADEQUATE-----------------
            //world.FrameMax = 6;
            //world.WorldSize = 900;
            //world.RespawnDelay = 300;
            //world.BaseFreq = 10;
            //Star.StarRad = 30;
            //MSPerFrame1 = 16;
            //Ship.EngineStrength = 0.08;
            //Ship.TurnRate = 3;
            //Projectile.ProjSpeed = 14;
            //StartingHp = 25;
            //world.StartingStarAmount = 4;
            //double accel = 0.02;
            //double mass = 0.002;
            //for (int n = 1; n < world.StartingStarAmount; n++)
            //{
            //    world.Stars.Add(Star.GetStarID(), new Star(Star.GetStarID(), new Vector2D(world.WorldSize, world.WorldSize), mass, accel));
            //}
            //world.StarDelay = 125;
            //world.StarCounterEnd = 500;

        }

        /// <summary>
        /// Invoked when a client disconnects
        /// </summary>
        /// <param name="m"></param>
        private void ClientDisconnect(SocketState m)
        {
            Console.WriteLine("DISCONNECT\tID:{0}", m.ID);
        }

        /// <summary>
        /// Start accepting Tcp sockets connections from client
        /// </summary>
        public void StartServer()
        {

            System.Console.WriteLine("Starting server...");
            //Start updating all valid and initialzied clients. This will happen once per frame. 
            Thread Updater = new Thread(() => Update());

            Updater.Start();
            Networking.ServerAwaitingClientLoop(HandleNewClient);

            System.Console.WriteLine("Server completed startup!");
        }

        /// <summary>
        /// Sends the current world state every MSPerFrame
        /// </summary>
        private void Update()
        {
            Stopwatch watch = new Stopwatch();
            while (true)
            {
                watch.Start();
                //Used to remove Sockets if the connection is lost. 
                List<int> socketToRemove = new List<int>();
                lock (gameClients)
                {
                    //Update the world
                    world.UpdateWorld();
                    //Create world package
                    worldMessage.Append(ProcessWorldMessage());
                    //Send the world package to all the socket states
                    foreach (SocketState s in gameClients.Values)
                    {
                        try
                        {
                            if (worldMessage != null)
                            {
                                Networking.Send(s.TheSocket, worldMessage.ToString());
                            }
                        }
                        //If there is some networking send error, remove the client from the server connection and remove the ship from the world. 
                        catch (Exception)
                        { 
                            if(!s.TheSocket.Connected)
                            socketToRemove.Add(s.ID);
                        }
                    }
                    worldMessage.Clear();
                    //Remove all unconnected or nulled sockets
                    foreach (int id in socketToRemove)
                    {
                        Console.WriteLine("CLIENT UNRESPONSIVE..\tID:{0}", id);
                        gameClients.Remove(id);
                        world.Ships.Remove(id);
                    }
                    
                }
                while (watch.ElapsedMilliseconds < MSPerFrame1) { }
                watch.Reset();
            }
        }
        /// <summary>
        /// Sender of all world objects
        /// </summary>
        /// <param name="s"></param>
        private string ProcessWorldMessage()
        {
            StringBuilder toSend = new StringBuilder();
            foreach (Ship ship in world.Ships.Values)
            {
                String serialize = JsonConvert.SerializeObject(ship);
                toSend.Append(serialize + "\n");
            }
            foreach (Projectile proj in world.Projectiles.Values)
            {
                //FLAG: Bottleneck caused for higher AI latency
                String serialize = JsonConvert.SerializeObject(proj);
                toSend.Append(serialize + "\n");
            }
            foreach (Star star in world.Stars.Values)
            {
                String serialize = JsonConvert.SerializeObject(star);
                toSend.Append(serialize + "\n");
            }
            if (toSend.Length <= 1)
            {
                return null;
            }
            return toSend.ToString().Substring(0, toSend.Length - 1);
        }

        /// <summary>
        /// The delegate callback passed to the networking class to handle a new client connecting.  
        /// This will change the callback for the socket state to a new method that receives the player's name, then ask for data
        /// </summary>
        /// <param name="s"></param>
        private void HandleNewClient(SocketState s)
        {
            System.Console.WriteLine("New Client! Initiating Handshake....");
            s.NetworkFunction = RecieveName;
            Networking.GetData(s);
        }
        //Implements the server's part of the initial handshake
        private void RecieveName(SocketState m)
        {
            //Process commands for a pre-handshake Client. 
            ProcessData(m, false);

            //Set the SocketState's ID 
            m.ID = GetClientID();

            //change the callback to a method that handles command requests from the client
            m.NetworkFunction = HandleData;

            //Send the startup info to the client (ID and world size) 
            Networking.Send(m.TheSocket, m.ID.ToString());
            Networking.Send(m.TheSocket, world.WorldSize.ToString());

            //Then add the client's socket to a list of all clients  
            lock (gameClients)
            {
                gameClients.Add(m.ID, m);
            }
            Console.WriteLine("Finished Handshake! Looking for more commands...");
            //Ask client for data 
            Networking.GetData(m);

        }

        /// <summary>
        /// Processes client direction commands
        /// </summary>
        /// <param name="m"></param>
        private void HandleData(SocketState m)
        {
            //System.Console.WriteLine("Recieved command!");
            //Process commands for a post-handshake Client 
            ProcessData(m, true);

            //Ask for more data 
            Networking.GetData(m);

        }
        /// <summary>
        /// Process command data from clients 
        /// </summary>
        /// <param name="sender">The SocketState that represents the client</param>
        private void ProcessData(SocketState sender, bool initialized)
        {
            string totalData = sender.MessageState.ToString();

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {

                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)

                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // Console.WriteLine("received message: \"" + p + "\"");

                //For pre-handshake messages.
                if (!initialized)
                {
                    //Make a new Ship with the given name and a new unique ID  
                    Ship playerShip = new Ship()
                    {
                        Name = p.Substring(0, p.Length - 1),
                        ID = GetShipID(),
                        Score = 0,
                        InitialHp = StartingHp,
                        DeathCounter = 1
                    }; 

                    //Resets important positioning and hp stats before addition to world. 
                    world.SpawnShip(playerShip); 

                    lock (gameClients)
                    {
                        //Add ship to the world
                        world.Ships.Add(playerShip.ID, playerShip);
                    }
                }
                else
                {
                    //Command coming in will change variables of the ship, therefore it needs to be under lock from the Update function. 
                    world.Ships[sender.ID].Command = p;

                }
                // Remove it from the SocketState's growable buffer
                sender.MessageState.Remove(0, p.Length);
                if (!initialized)
                {
                    return;
                }
            }
        }
    }
}
