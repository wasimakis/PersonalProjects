using Positioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorldObjects;


namespace Setting
{
    /// <summary>
    /// The world contains specific world objects
    /// </summary>
    public class World
    {
        /// <summary>
        /// List of Ships
        /// </summary> 
        private Dictionary<int, Ship> ships;

        /// <summary>
        /// List of Projectiles
        /// </summary>
        private Dictionary<int, Projectile> projectiles;

        /// <summary>
        /// List of stars
        /// </summary>
        private Dictionary<int, Star> stars;

        /// <summary>
        /// The width and heigth of the world.
        /// </summary>
        private int worldSize;

        /// <summary>
        /// The maximum required amount of frames to fire a projectile. 
        /// </summary>
        private int frameMax;

        /// <summary>
        /// The maximum amount of frame delay required before respawning. 
        /// </summary>
        private int respawnDelay;

        /// <summary>
        /// The maximum amount of frames a star needs to wait before prepping in. 
        /// </summary>
        private int starDelay;

        /// <summary>
        /// Generator used for ship positioning and star direction.
        /// </summary>
        private readonly Random rand;

        public int StartingStarAmount { get => starInitAmount; set => starInitAmount = value; }

        /// <summary>
        /// A base frequency used for star frame respawn potential
        /// </summary>
        private int baseFreq;

        /// <summary>
        /// Initial amount of stars at server startup
        /// </summary>
        private int starInitAmount;

        /// <summary>
        /// Increment to put chance to spawn a star in favor. 
        /// </summary>
        private double chanceToSpawn = 0.0;

        /// <summary>
        /// Used to increment a counter to determine when next to add or delete new stars. 
        /// </summary>
        private int starCounter = 0;

        /// <summary>
        /// Used to increment a counter to determine when next to add or delete new stars. 
        /// </summary>
        private int starCounterEnd = 0;


        /// <summary>
        /// Enables/Disables Enhanced Mode
        /// </summary> 
        private bool enhanced; 

        public World()
        {
            Ships = new Dictionary<int, Ship>();
            Projectiles = new Dictionary<int, Projectile>();
            Stars = new Dictionary<int, Star>();
            rand = new Random(1996);
            //starDelay = 900;
            worldSize = 0;
        }

        /// <summary>
        /// Application function to update all ships
        /// </summary>
        private void UpdateShips()
        {
            foreach (Ship s in ships.Values)
            {
                this.UpdateShip(s);
            }

        }
        /// <summary>
        /// Application function to update all projectiles
        /// </summary>
        private void UpdateProjectiles()
        {
            List<int> projCleanup = new List<int>();
            foreach (Projectile p in projectiles.Values)
            {
                int projClean = this.UpdateProj(p);
                if (projClean != -1)
                {
                    projCleanup.Add(projClean);
                };
            }
            foreach (int projID in projCleanup)
            {
                this.projectiles.Remove(projID);
            }

        }

        /// <summary>
        /// Application function to update all stars
        /// </summary>
        private void UpdateStars()
        {
            foreach (Star s in stars.Values)
            {
                UpdateStar(s);
            } 
            //If we are unenhanced, this will essentially block the stars from doing the enhanced updating
            if (!this.enhanced) {
                return;
            } 
            //The following contains the logic that recreates new stars that will follow across screen. 
            if (starCounter >= starCounterEnd)
            {
                if (rand.NextDouble() + chanceToSpawn > 0.96)
                {
                    this.Stars.Add(Star.GetStarID(), new Star(Star.GetStarID(), new Vector2D(WorldSize, WorldSize), 0.002, 0.02));
                    chanceToSpawn = -0.05 * this.Stars.Count;
                }
                else
                {
                    chanceToSpawn += 0.1;
                }
                starCounter = 0;
            }
            else
            {
                starCounter++;
            }
        }
        /// <summary>
        /// Updates all the world objects on seperate threads to divide and conquer the updating. 
        /// </summary>
        public void UpdateWorld()
        {
            UpdateShips();
            UpdateProjectiles();
            UpdateStars();
        }
        /// <summary>
        /// Updates a star
        /// </summary>
        /// <param name="s"></param>
        private void UpdateStar(Star s)
        { 
            //If we are in an unenhanced world
            if (!this.enhanced ) { 
                //If the current star is not in the correct value default placement
                if (s.Loc.GetX() != 0 && s.Loc.GetY() != 0) {
                    s.Loc = new Vector2D(0, 0);
                }
                return;
            }
            //Star is dead, so see if we can spawn in 
            if (!s.Alive)
            {
                //Star is dead and past the star frame timer, therefore we can reset the 
                //star frame and make the star alive. 
                if (s.StarFrame >= StarDelay)
                {
                    s.Alive = true;
                    s.Velocity = new Vector2D(0, 0);
                    s.StarFrame = 0;
                }
                //Increment the StarFrame counter in a range from 1-50 frames. 
                else
                {
                    s.StarFrame += baseFreq * rand.NextDouble();
                }
                return;
            }
            //At this point if the star is alive and it has just been alive, we can determine its position 
            if (s.StarFrame == 0)
            {
                GenerateStarSpawn(out Vector2D initPos, out Vector2D initDir);
                s.Loc = initPos;
                s.Dir = initDir;
                s.StarFrame++;
                return;
            }
            Vector2D accel = new Vector2D(s.Dir);
            accel *= s.Accel;
            s.Velocity += accel;
            s.Loc += s.Velocity;
            if (!InBounds(s.Loc, -worldSize / 2))
            {
                s.Alive = false;
                s.Velocity = new Vector2D(0, 0);
                s.StarFrame = 0;
            }
        }

        private void GenerateStarSpawn(out Vector2D initPos, out Vector2D initDir)
        {
            initPos = new Vector2D();
            initDir = new Vector2D(50, 50);
            initDir.Normalize();
            initDir.Rotate(45);
            double randomSet = rand.Next() % this.worldSize / 2;
            if (rand.Next() % 2 == 0)
            {
                randomSet = -randomSet;
            }
            switch (rand.Next() % 3)
            {
                //Left Side
                case 0:
                    initPos = new Vector2D(((-this.worldSize / 2) - (Star.StarRad * 2)), randomSet);
                    initDir.Rotate(-90);
                    break;
                //Right Side
                case 1:
                    initPos = new Vector2D((this.worldSize / 2 + (Star.StarRad * 2)), randomSet);
                    initDir.Rotate(90);
                    break;
                //Bottom Side
                case 2:
                    initPos = new Vector2D(randomSet, (this.worldSize / 2 + (Star.StarRad * 2)));
                    initDir.Rotate(180);
                    break;
                //Top Side
                case 3:
                    initPos = new Vector2D(randomSet, (-this.worldSize / 2 - (Star.StarRad * 2)));
                    initDir.Rotate(0);
                    break;
            }
        }
        /// <summary>
        /// Update projectiles
        /// </summary>
        /// <param name="p"></param>
        private int UpdateProj(Projectile p)
        {
            if (!p.Alive)
            {
                return p.ID;
            }
            //If it already made contact, set it to dead. 
            if (p.MadeContact)
            {
                p.Alive = false;
                return -1;
            }
            //If it is out of bounds, set it to dead 
            if (!(InBounds(p.Loc, -10))) {
                p.Alive = false;
                return -1;
            }
            foreach (Star star in this.Stars.Values)
            {
                //Comes into contact with a sun it is destroyed. 
                if (ColliderDetection(p.Loc, star.Loc, 10, Star.StarRad))
                {
                    p.Alive = false;
                    return -1;
                }
            }
            //The direction of the projectile
            Vector2D projVel = new Vector2D(p.Dir);
            projVel = new Vector2D(p.Dir);
            projVel *= p.Speed;
            p.Loc += projVel;
            return -1;
        }

        ///// <summary>
        ///// A function to bounce a projectile off from a circle
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="s"></param>
        ///// <returns></returns>
        //private Vector2D StarBounce(Projectile p, Star star) {
        //    Vector2D relativeAngle = new Vector2D(p.Loc-star.Loc);
        //    relativeAngle.Normalize(); 
        //    //Find the angle of the star relative to Up
        //    double starAngle = relativeAngle.ToAngle();  
        //    //Find the angle of the projectile relative to Up
        //    double projAngle = new Vector2D(p.Dir).ToAngle();
        //    //Through a lot of math.... subtract both angles to find the rotation required after the point of rotation is normalized to the original angle 
        //    double addRotation = projAngle - starAngle;
        //    if (addRotation <= 0)
        //    {
        //        relativeAngle.Rotate(addRotation);
        //    }
        //    else {
        //        relativeAngle.Rotate(-addRotation);
        //    }
        //    return relativeAngle;

        //}

        /// <summary>
        /// Resets a ship's direction, value, HP, thrust, and velocity
        /// </summary>
        public void SpawnShip(Ship s)
        {
            s.Loc = SpawnPicker(s);
            s.Dir = new Vector2D(0, -1);
            s.Velocity = new Vector2D(0, 0);
            s.Thrust = false;
            s.HP = 5;
        }
        //Helper to recursively randomize but insure ship placement is valid and not within Star radius.
        private Vector2D SpawnPicker(Ship s)
        {
            //Create a new x coordinate within the world size. 
            int xRes = rand.Next() % (this.WorldSize / 2);
            //Randomize to turn the cooridnate negative 
            if (rand.Next() % 2 == 0)
            {
                xRes = -xRes;
            }

            //Create a new y coordinate within the world size. 
            int yRes = rand.Next() % (this.WorldSize / 2);
            //Randomize to turn the cooridnate negative 
            if (rand.Next() % 2 == 0)
            {
                yRes = -yRes;
            }
            Vector2D resetLoc = new Vector2D(xRes, yRes);
            foreach (Star star in Stars.Values)
            {
                if (ColliderDetection(resetLoc, star.Loc, Ship.Radius, Star.StarRad))
                {
                    resetLoc = SpawnPicker(s);
                }
            }
            return resetLoc;
        }
        /// <summary>
        /// Updating the Ship with movement, commands, and collision detection
        /// </summary>
        /// <param name="s"></param>
        private void UpdateShip(Ship s)
        {

            //If the ship is dead,
            if (s.HP == 0)
            {
                //If the ship still is counting through its respawn delay, increment the death counter and return (the ship is essentially not in play while its hp is zero)
                if (s.DeathCounter < RespawnDelay)
                {
                    s.DeathCounter++;
                    return;
                }
                //If the ship is done counting through its respawn delay, respawn the ship. 
                else
                {
                    s.DeathCounter = 1;
                    this.SpawnShip(s);
                }
            }

            //EXTRA----Border Functionality 
            if (!InBounds(s.Loc, Ship.Radius * 2))
            { 
                
                double outX = s.Loc.GetX();
                double outY = s.Loc.GetY();
                //Ship has gone outside left of world size
                outX = (s.Loc.GetX() < (-this.worldSize / 2) + Ship.Radius) ? -(this.worldSize / 2) + Ship.Radius : outX;
                //Ship has gone outside right of world size
                outX = (s.Loc.GetX() > (this.worldSize / 2) - Ship.Radius) ? (this.worldSize / 2) - Ship.Radius : outX;
                //Ship has gone outside down of world size  
                outY = (s.Loc.GetY() > (this.WorldSize / 2) - Ship.Radius) ? (this.worldSize / 2) - Ship.Radius : outY;
                //Ship has gone outside up of world size
                outY = (s.Loc.GetY() < (-this.worldSize / 2) + Ship.Radius) ? -(this.worldSize / 2) + Ship.Radius : outY;
                s.Loc = new Vector2D(outX, outY);
            }
            Vector2D totalAccel = new Vector2D(0, 0);
            //Calculate mass-based acceleration (i.e. gravity)
            foreach (Star star in this.Stars.Values)
            {
                Vector2D starGrav = star.Loc - s.Loc;
                starGrav.Normalize();
                starGrav *= star.Mass;
                totalAccel += starGrav;
            }

            //Thrust
            if (s.Command.Contains("T"))
            {
                Vector2D thrustAccel = new Vector2D(s.Dir);
                thrustAccel *= Ship.EngineStrength;
                totalAccel += thrustAccel;
                s.Thrust = true;
            }
            else
            {
                s.Thrust = false;
            }
            s.Velocity += totalAccel;
            s.Loc += s.Velocity;
           
            //turn right
            if (s.Command.Contains("R"))
            {
                s.Dir.Rotate(Ship.TurnRate);
            }
            //Turn left
            if (s.Command.Contains("L"))
            {
                s.Dir.Rotate(-Ship.TurnRate);
            }
            //Fire a projectile
            if (s.Command.Contains("F") && s.FrameDelay >= this.FrameMax)
            {
                Vector2D projDir = new Vector2D(s.Dir);
                Projectile proj = new Projectile(Projectile.GetProjID(), s.Loc, projDir, true, s.ID, Projectile.ProjSpeed);
                this.projectiles.Add(proj.ID, proj);
                s.FrameDelay = 0;
            }
            s.FrameDelay++;
            s.Command = "";
            //Collision detection with projectiles and stars.   
            foreach (Projectile potentialProj in this.projectiles.Values)
            {
                //Projectile collision is a point collision with a ship's radius, therefore the projectile radius is 0. 
                if (ColliderDetection(s.Loc, potentialProj.Loc, Ship.Radius, 0))
                {
                    //Decrement the ship's hp if the projectile is not its owner.  
                    if (s.ID != potentialProj.Owner)
                    {
                        s.HP--;
                        //Add a point for dealing damage
                        this.Ships[potentialProj.Owner].Score++;
                        potentialProj.MadeContact = true;
                    }
                    //If the ship is dead, return.
                    if (s.HP == 0)
                    {
                        //Gain more points for the final blow
                        this.Ships[potentialProj.Owner].Score += 9;
                        break;
                    }
                }
            }
            foreach (Star star in this.Stars.Values)
            {
                if (ColliderDetection(s.Loc, star.Loc, Ship.Radius, Star.StarRad))
                {
                    s.HP = 0;
                    return;

                }
            }
        }
        /// <summary>
        /// Circular collision detection for two different Vector2D positions and corresponding radi. 
        /// Follows the Pythagorean theorem: (x1-x2)^2+(y1-y2)^2 = (r1+r2)^2
        /// </summary>
        /// <param name="gameObj1">a game object's positon</param>
        /// <param name="gameObj2">another game object's position</param>
        /// <param name="gameObj1Radius">a game object's radius</param> 
        /// <param name="gameObj2Radius">another game object's radius</param>
        /// <returns></returns>
        private static bool ColliderDetection(Vector2D gameObj1, Vector2D gameObj2, int gameObj1Radius, int gameObj2Radius)
        {
            return Math.Pow((gameObj2.GetY() - gameObj1.GetY()), 2) + Math.Pow(gameObj2.GetX() - gameObj1.GetX(), 2) <= Math.Pow((gameObj1Radius + gameObj2Radius), 2);

        }

        public int WorldSize { get => worldSize; set => worldSize = value; }
        public Dictionary<int, Star> Stars { get => stars; set => stars = value; }
        public Dictionary<int, Projectile> Projectiles { get => projectiles; set => projectiles = value; }
        public Dictionary<int, Ship> Ships { get => ships; set => ships = value; }
        public int FrameMax { get => frameMax; set => frameMax = value; }
        public int RespawnDelay { get => respawnDelay; set => respawnDelay = value; }
        public int BaseFreq { get => baseFreq; set => baseFreq = value; }
        public int StarDelay { get => starDelay; set => starDelay = value; }
        public int StarCounterEnd { get => starCounterEnd; set => starCounterEnd = value; }
        public bool Enhanced { get => enhanced; set => enhanced = value; }


        /// <summary>
        /// Determines if a vector 2d position is out of the world space with a desired padding. 
        /// </summary>
        /// <param name="pos">The vector2d treated as a point</param> 
        /// <param name="padding">The padding/param>
        /// <returns></returns>
        private bool InBounds(Vector2D pos, int padding)
        {
            return pos.GetX() < (worldSize / 2) - padding && pos.GetX() > -(worldSize / 2) + padding && pos.GetY() < (worldSize / 2) - padding && pos.GetY() > -(worldSize / 2) + padding;
        }

    }
}
