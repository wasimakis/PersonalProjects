using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Setting;
using NetworkController;
using System.Text.RegularExpressions;
using WorldObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace Controller
{
    /// <summary>
    /// This class is responsible for parsing the information
    /// from the network controller, updating the model, and informing the
    /// view that the world has changed.
    /// </summary>
    public class SpaceController
    {

        private bool turnRight = false;
        private bool turnLeft = false;
        private bool fireProj = false;
        private bool thrustForward = false;
        /// <summary>
        /// playerName initialized by ServerConnect and utilized in the FirstContact function
        /// </summary>
        private string playerName;
        /// <summary>
        /// The model of the world.
        /// </summary>
        private World world;

        /// <summary>
        /// Delegate used to do something if there is a networking error. 
        /// </summary>
        /// <param name="e"></param>
        public delegate void NetworkingError();

        public event NetworkingError connectionFailure;

        /// <summary>
        ///Boolean variables used to delegate strings being sent by the server during RecieveStartup. 
        /// </summary>
        private bool IDInit = false;
        private bool WorldInit = false;

        /// <summary>
        /// Determines if we can change the delegate of NetworkFunction within the SocketState
        /// </summary>

        /// <summary>
        /// Delegate used for the event of the button click to connect the client to the server.
        /// </summary>
        /// <param name="server"></param>
        public delegate void AttemptConnection();
        /// <summary>
        /// Delegate used for the event of a server message being sent, and deserialized.
        /// </summary>
        /// <param name="world"></param>
        public delegate void ServerUpdateHandler();
        /// <summary>
        /// Delegate used for the even to change the world size when recieved. 
        /// </summary>
        /// <param name="size"></param>
        public delegate void ChangeWorldSize();
        /// <summary>
        /// Event called when the button is clicked to connect the client to the server.
        /// </summary>
        public event AttemptConnection AttemptConnect;

        public event ServerUpdateHandler ServerUpdater;

        public event ChangeWorldSize WorldUpdater;

        private Socket socket;

        public World World { get => world; set => world = value; }

        public SpaceController()
        {
            World = new World();
            Networking.connectionFailed += this.connectionFailed;

        }

        /// <summary>
        /// This is the method that should be called whenever a connection to the server has failed.
        /// </summary>
        private void connectionFailed()
        {
            connectionFailure();
        }

        public void ServerConnect(string hostName, string playerName)
        {
            this.playerName = playerName;
            //System.Diagnostics.Debug.Write("About to initiate connection to server " + hostName);
            socket = Networking.ConnectToServer(FirstContact, hostName);
            //System.Diagnostics.Debug.Write("Finished attempting a server connection.");
            AttemptConnect();
        }

        /// <summary>
        /// Sends the player's name and a new line to server. (Based upon custom networking protocols)
        /// </summary>
        /// <param name="s"></param>
        public void FirstContact(SocketState s)
        {
            s.NetworkFunction = ReceiveStartup;
            Networking.Send(s.TheSocket, playerName);
            //System.Diagnostics.Debug.WriteLine("...End invoking networking function");
        }

        private void ReceiveStartup(SocketState m)
        {
            ProcessMessage(m, true);
            if (WorldInit)
            {
                m.NetworkFunction = ReceiveWorld;
            }
            Networking.GetData(m);
        }

        private void ReceiveWorld(SocketState m)
        {
            ProcessMessage(m, false);
            this.SendData(m.TheSocket);
            Networking.GetData(m);
        }

        private void SendData(Socket m)
        {
            string message = "(";
            if (turnRight)
            {
                message += "R";
            }
            if (turnLeft)
            {
                message += "L";
            }
            if (fireProj)
            {
                message += "F";
            }
            if (thrustForward)
            {
                message += "T";
            }
            if (thrustForward || fireProj || turnLeft || turnRight)
            {
                message += ")";
            }
            if (message.Length >= 3)
                Networking.Send(m, message);
        }

        /// <summary>
        /// This is the worker method that processes a message when received.
        /// For proper MVC, this should go in its own controller
        /// </summary>
        /// <param name="ss">The SocketState on which the message was received</param>
        private void ProcessMessage(SocketState ss, bool isRecieveStartup)
        {
            string totalData = ss.MessageState.ToString();
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

                //Recieve startup is a special first case called on process message. 
                //When this is true, the client will attempt to determine the world size and extract the player ID. 
                if (isRecieveStartup)
                {

                    //If the ID has not been extracted.
                    if (!IDInit)
                    {
                        if (Int32.TryParse(p, out int playerID))
                        {
                            IDInit = true;
                        }

                    }
                    //If the world size has not been extracted. 
                    else if (!WorldInit)
                    {
                        if (Int32.TryParse(p, out int worldSize))
                        {
                            this.World.WorldSize = worldSize;
                            WorldUpdater();

                            WorldInit = true;
                        }

                    }

                }
                //If it is not this special first case, the client will attempt to deserialize each completed 
                //message. 
                else
                {
                    lock (this.World)
                    {
                        this.InitDeserializer(p);
                    }


                }


                // Then remove it from the SocketState's growable buffer

                ss.MessageState.Remove(0, p.Length);
            }
            //We only want to update the server every frame which occurs at the end of all the processed messages. 
            ServerUpdater();
        }

        /// <summary>
        /// Flagger control function to determine what commands to send the server if any. 
        /// </summary>
        /// <param name="keyCode">the key that has been pressed</param>
        /// <param name="keyDown">to determine if the key has been pressed/lifted. </param>
        public void FlagChanger(Keys keyCode, bool keyDown)
        {
            switch (keyCode)
            {
                case Keys.Up:
                    //set Thrust Flag true.
                    thrustForward = keyDown;
                    break;
                case Keys.Right:
                    //Set Right Flag true 
                    turnRight = keyDown;
                    break;
                case Keys.Left:
                    //Set Left flag true.  
                    turnLeft = keyDown;
                    break;
                case Keys.Space:
                    //Set Fire flag true.  
                    fireProj = keyDown;
                    break;
                default:
                    //Do nothing
                    break;
            }
        }
        /// <summary>
        /// Deserializes a completed message into a world object and adjusts the world control accordingly.  
        /// </summary>
        /// <param name="p">The completed message</param>
        private void InitDeserializer(string p)
        {

            JObject obj = JObject.Parse(p);
            JToken token = obj["ship"];

            //We know that the completed message is a ship, which contains all the JSON fieldNames of a Ship  
            if (token != null)
            {
                Ship rebuilt = JsonConvert.DeserializeObject<Ship>(p);


                if (this.world.Ships.ContainsKey(rebuilt.ID))
                {
                    //Setup required to determine the initial HP of a ship. 
                    rebuilt.InitialHp = this.world.Ships[rebuilt.ID].InitialHp;

                    //Extract the previous deathCounter
                    int curDeathCounter = this.world.Ships[rebuilt.ID].DeathCounter;

                    //If the ship is still dead we want to increment the amount of its death counter: 
                    if (rebuilt.HP == 0)
                    {
                        curDeathCounter++;

                    }
                    //If the ship is not dead, revert the death counter. 
                    else
                    {
                        curDeathCounter = 0;
                    }

                    this.world.Ships[rebuilt.ID] = rebuilt;
                    this.world.Ships[rebuilt.ID].DeathCounter = curDeathCounter;
                    this.world.Ships[rebuilt.ID].Spawned = true;
                }
                else
                {
                    if (rebuilt.HP == 0)
                    {
                        rebuilt.InitialHp = 5;
                        if (rebuilt.Spawned)
                        {
                            rebuilt.DeathCounter++;
                        }
                        rebuilt.Spawned = true;
                    }
                    else
                    {
                        rebuilt.DeathCounter = 0;
                        rebuilt.InitialHp = rebuilt.HP;
                        rebuilt.Spawned = true;
                    }

                    this.world.Ships.Add(rebuilt.ID, rebuilt);
                }


                return;
            }

            //We know that the completed message is a projectile, which contains all the JSON fieldNames of a Projectile
            token = obj["proj"];
            if (token != null)
            {
                Projectile rebuilt = JsonConvert.DeserializeObject<Projectile>(p);

                if (this.world.Projectiles.ContainsKey(rebuilt.ID))
                {
                    if (rebuilt.Alive)
                    {
                        this.world.Projectiles[rebuilt.ID] = rebuilt;
                    }
                    else
                    {
                        this.World.Projectiles.Remove(rebuilt.ID);
                    }
                }
                else
                {
                    if (rebuilt.Alive)
                    {
                        this.world.Projectiles.Add(rebuilt.ID, rebuilt);
                    }
                }



                return;
            }
            //We know that the completed message is a star, which contains all the JSON fieldNames of a Star
            token = obj["star"];
            if (token != null)
            {
                Star rebuilt = JsonConvert.DeserializeObject<Star>(p);

                if (this.world.Stars.ContainsKey(rebuilt.ID))
                {
                    this.world.Stars[rebuilt.ID] = rebuilt;
                }
                else
                {
                    this.world.Stars.Add(rebuilt.ID, rebuilt);
                }
                return;
            }

        }
    }
}
