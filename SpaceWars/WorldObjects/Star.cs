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
    public class Star
    {
        /// <summary>
        /// an int representing the star's unique ID.
        /// </summary>
        [JsonProperty(PropertyName = "star")]
        private int id;
        /// <summary>
        /// a Vector2D representing the star's location.
        /// </summary>
        [JsonProperty]
        private Vector2D loc;
        /// <summary>
        /// a double representing the star's mass. Note that the sample client does not use this information, 
        /// but you may choose to display stars differently based on their mass.
        /// </summary>
        [JsonProperty]
        private double mass;

        /// <summary>
        /// Represents a star's acceleration
        /// </summary>
        private double accel; 

        /// <summary>
        /// The radius of a star's collision detection 
        /// </summary> 
        private static int starRad;

        /// <summary>
        /// Star ID generator
        /// </summary>
        private static int starID = -1;

        /// <summary>
        /// A star's velocity
        /// </summary>
        private Vector2D velocity = new Vector2D(0, 0);
        /// <summary>
        /// Determines if a star is alive or not
        /// </summary>
        private bool alive;
        /// <summary>
        /// A status counter for potential star spawning
        /// </summary> 
        private double starFrame = 0;
        /// <summary>
        /// The direction a Star travels in
        /// </summary>
        private Vector2D dir = new Vector2D(0,0);



        public int ID { get => id; set => id = value; }
        public Vector2D Loc { get => loc; set => loc = value; }
        public double Mass { get => mass; set => mass = value; }
        public static int StarRad { get => starRad; set => starRad = value; }
        public double Accel { get => accel; set => accel = value; }
        public Vector2D Velocity { get => velocity; set => velocity = value; }
        public bool Alive { get => alive; set => alive = value; }
        public double StarFrame { get => starFrame; set => starFrame = value; }
        public Vector2D Dir { get => dir; set => dir = value; }

        /// <summary>
        ///  Default Constructor that just allocates memory for a Star object. Necessary for JSON serialization. 
        /// </summary>
        public Star()
        {
        }
        /// <summary>
        /// Instantiates a Star
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        /// <param name="mass"></param>
        public Star(int id, Vector2D loc, double mass, double accel)
        {
            this.id = id;
            this.loc = loc;
            this.mass = mass;
            this.accel = accel;
        }

        public static int GetStarID()
        {
            if (starID == Int32.MaxValue)
            {
                starID = 0;
                return starID;
            }
            else
            {
                starID++;
                return starID;
            }
        }
    }
}

