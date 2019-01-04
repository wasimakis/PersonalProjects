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
    public class Projectile
    {
        /// <summary>
        /// An Id associated with the projectile object. 
        /// </summary>
        [JsonProperty(PropertyName = "proj")]
         private int id;
        /// <summary>
        /// The location the projectile object exists at, represented as a Vector2D obkect. 
        /// </summary>
        [JsonProperty]
        private Vector2D loc;
        /// <summary>
        /// The direction the projectile object is rotated at, represented as a Vector2D object. 
        /// </summary>
        [JsonProperty]
        private Vector2D dir;
        /// <summary>
        /// Determines if this projectile is alive. 
        /// </summary>
        [JsonProperty]
        private bool alive;
        /// <summary>
        /// Determines the ID owner of this projectile
        /// </summary>
        [JsonProperty]
        private int owner; 

        /// <summary>
        /// The current speed of the projectile (effected by collisions)
        /// </summary>
        private int speed;

        /// <summary>
        /// A unique projectile ID that will wrap around after it reaches the Max integer.
        /// </summary>
        private static int projID = -1;

        /// <summary>
        /// A projectile speed. 
        /// </summary>
        private static int projSpeed;

        /// <summary>
        /// A tag to denote a projectile came into contact with a ship
        /// </summary> 
        private bool madeContact;

        public Vector2D Loc { get => loc; set => loc = value; }
        public Vector2D Dir { get => dir; set => dir = value; }
        public bool Alive { get => alive; set => alive = value; }
        public int Owner { get => owner; set => owner = value; }
        public int ID { get => id; set => id = value; }
        public static int ProjSpeed { get => projSpeed; set => projSpeed = value; }
        public bool MadeContact { get => madeContact; set => madeContact = value; }
        public int Speed { get => speed; set => speed = value; }

        /// <summary>
        /// Default Constructor that just allocates memory for a Projectile object. Necessary for JSON serialization. 
        /// </summary>
        public Projectile() { }

        /// <summary>
        /// Constructs a Projectile
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="loc"></param>
        /// <param name="dir"></param>
        /// <param name="alive"></param>
        /// <param name="owner"></param>
        public Projectile(int ID, Vector2D loc, Vector2D dir, bool alive, int owner, int speed) {
            this.id = ID;
            this.loc = loc;
            this.dir = dir;
            this.alive = alive;
            this.owner = owner;
            this.Speed = speed;
        }

        public static int GetProjID()
        {
            if (projID == Int32.MaxValue)
            {
                projID = 0;
                return projID;
            }
            else
            {
                projID++;
                return projID;
            }
        }
    }
}

