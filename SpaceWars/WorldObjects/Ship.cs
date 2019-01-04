using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Positioning;

namespace WorldObjects
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Ship
    {
        /// <summary>
        /// an int representing the ship's unique ID.
        /// </summary>
        [JsonProperty(PropertyName = "ship")]
        private int id;
        /// <summary>
        /// a Vector2D representing the ship's location.
        /// </summary>
        [JsonProperty]
        private Vector2D loc;
        /// <summary>
        /// a Vector2D representing the ship's orientation
        /// </summary>
        [JsonProperty]
        private Vector2D dir;
        /// <summary>
        /// a bool representing whether or not the ship was firing engines on that frame. 
        /// This can be used by the client to draw a different representation of the ship, e.g., showing engine exhaust.
        /// </summary>
        [JsonProperty]
        private bool thrust;
        /// <summary>
        /// a string representing the player's name
        /// </summary>
        [JsonProperty]
        private string name;
        /// <summary>
        /// and int representing the hit points of the ship. This value ranges from 0 - 5. If it is 0, 
        /// then this ship is temporarily destroyed, and waiting to respawn. If the player controlling 
        /// this ship disconnects, the server will discontinue sending this ship.
        /// </summary>
        [JsonProperty]
        private int hp;
        /// <summary>
        /// an int representing the ship's score.
        /// </summary>
        [JsonProperty]
        private int score;

        /// <summary>
        /// Current command sequence to determine how to update ship appropriately. (always nothing when first created)
        /// </summary>
        private string command = "";

        private int initialHp;

        //Counter used to determine how long a ship has been dead for.
        private int deathCounter;

        /// <summary>
        /// How much acceleration the ship's engines apply.
        /// </summary>
        private static double engineStrength;

        /// <summary>
        /// How many degrees the ship's turn per rotation. 
        /// </summary> 
        private static int turnRate;

        /// <summary>
        /// How much radius does the ship occupy
        /// </summary> 
        private static readonly int radius = 30;

        /// <summary>
        /// The velocity of a ship (always zero when first created)
        /// </summary>
        private Vector2D velocity = new Vector2D(0,0);

        /// <summary>
        /// Frame counter for firing delay
        /// </summary>
        private int frameDelay = 1; 

        //Boolean to determine if the ship has been spawned in. 
        bool spawned = false;

        /// <summary>
        /// An out of bounds buffer used to calculating the border extent. 
        /// </summary>
        private int obBuffer;

        public int ID { get => id; set => id = value; }
        public Vector2D Loc { get => loc; set => loc = value; }
        public Vector2D Dir { get => dir; set => dir = value; }
        public bool Thrust { get => thrust; set => thrust = value; }
        public string Name { get => name; set => name = value; }
        public int HP { get => hp; set => hp = value; }
        public int Score { get => score; set => score = value; }
        public int InitialHp { get => initialHp; set => initialHp = value; }
        public int DeathCounter { get => deathCounter; set => deathCounter = value; }
        public bool Spawned { get => spawned; set => spawned = value; }
        public static double EngineStrength { get => engineStrength; set => engineStrength = value; }
        public static int TurnRate { get => turnRate; set => turnRate = value; }
        public static int Radius { get => radius; }
        public string Command { get => command; set => command = value; }
        public Vector2D Velocity { get => velocity; set => velocity = value; }
        public int FrameDelay { get => frameDelay; set => frameDelay = value; }
        public int ObBuffer { get => obBuffer; set => obBuffer = value; }

        /// <summary>
        ///  Default Constructor that just allocates memory for a Ship object. Necessary for JSON serialization. 
        /// </summary>
        public Ship() { }

        /// <summary>
        /// Creates a Ship instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        /// <param name="dir"></param>
        /// <param name="thrust"></param>
        /// <param name="name"></param>
        /// <param name="hp"></param>
        /// <param name="score"></param>
        public Ship(int id, Vector2D loc, Vector2D dir, bool thrust, string name, int hp, int score)
        {
            this.id = id;
            this.loc = loc;
            this.dir = dir;
            this.thrust = thrust;
            this.name = name;
            this.hp = hp;
            this.score = score;
            this.command = "";
        } 
    }
}
