using Controller;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldObjects;

namespace DrawingAux
{
    public class DrawingPanel : Panel
    {
        //A controller used to reference the world and information necessary. 
        SpaceController controller;

        // This is a list of all the ship, projectile, and star images. We need to make these members of the
        // class because creating the Bitmaps is a bottleneck in our program, and it slows down when there
        // is too much going on on the screen.
        private Image starImage = new Bitmap(@"..\..\..\Resources\Images\star.jpg"); 
        

        private Image shipImageThrustBlue = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-blue.png");
        private Image shipImageThrustBrown = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-brown.png");
        private Image shipImageThrustGreen = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-green.png");
        private Image shipImageThrustGrey = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-grey.png");
        private Image shipImageThrustRed = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-red.png");
        private Image shipImageThrustViolet = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-violet.png");
        private Image shipImageThrustWhite = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-white.png");
        private Image shipImageThrustYellow = new Bitmap(@"..\..\..\Resources\Images\ship-thrust-yellow.png");

        private Image shipImageCoastBlue = new Bitmap(@"..\..\..\Resources\Images\ship-coast-blue.png");
        private Image shipImageCoastBrown = new Bitmap(@"..\..\..\Resources\Images\ship-coast-brown.png");
        private Image shipImageCoastGreen = new Bitmap(@"..\..\..\Resources\Images\ship-coast-green.png");
        private Image shipImageCoastGrey = new Bitmap(@"..\..\..\Resources\Images\ship-coast-grey.png");
        private Image shipImageCoastRed = new Bitmap(@"..\..\..\Resources\Images\ship-coast-red.png");
        private Image shipImageCoastViolet = new Bitmap(@"..\..\..\Resources\Images\ship-coast-violet.png");
        private Image shipImageCoastWhite = new Bitmap(@"..\..\..\Resources\Images\ship-coast-white.png");
        private Image shipImageCoastYellow = new Bitmap(@"..\..\..\Resources\Images\ship-coast-yellow.png");

        private Image projImageBlue = new Bitmap(@"..\..\..\Resources\Images\shot-blue.png");
        private Image projImageBrown = new Bitmap(@"..\..\..\Resources\Images\shot-brown.png");
        private Image projImageGreen = new Bitmap(@"..\..\..\Resources\Images\shot-green.png");
        private Image projImageGrey = new Bitmap(@"..\..\..\Resources\Images\shot-grey.png");
        private Image projImageRed = new Bitmap(@"..\..\..\Resources\Images\shot-red.png");
        private Image projImageViolet = new Bitmap(@"..\..\..\Resources\Images\shot-violet.png");
        private Image projImageWhite = new Bitmap(@"..\..\..\Resources\Images\shot-white.png");
        private Image projImageYellow = new Bitmap(@"..\..\..\Resources\Images\shot-yellow.png");

        //This image is 225 X 225 pixels
        private Image explosionSprite = new Bitmap(@"..\..\..\Resources\Images\expl_sprites.png");

        private static int expSide = 225 / 3; 
        

        //Sections of the explosion Sprite image 
        Rectangle upLeft = new Rectangle(0, 0, expSide, expSide);
        Rectangle upMid = new Rectangle(expSide, 0, expSide, expSide);
        Rectangle upRight = new Rectangle(expSide*2, 0, expSide, expSide); 

        Rectangle midLeft = new Rectangle(0, expSide, expSide, expSide);
        Rectangle midMid = new Rectangle(expSide, expSide, expSide, expSide);
        Rectangle midRight = new Rectangle(expSide * 2, expSide, expSide, expSide); 

        Rectangle botLeft = new Rectangle(0, expSide * 2, expSide, expSide);
        Rectangle botMid = new Rectangle(expSide, expSide * 2, expSide, expSide);
        Rectangle botRight = new Rectangle(expSide * 2, expSide * 2, expSide, expSide);


        public DrawingPanel(SpaceController controller)
        {
            DoubleBuffered = true;
            this.controller = controller;
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // Perform the transformation
            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            // Draw the object 
            drawer(o, e);
            // Then undo the transformation
            e.Graphics.ResetTransform();
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (this.controller.World)
            {
                // Draw the Ships
                foreach (Ship s in this.controller.World.Ships.Values)
                {
                    DrawObjectWithTransform(e, s, this.Size.Width, s.Loc.GetX(), s.Loc.GetY(), s.Dir.ToAngle(), ShipDrawer);
                }

                // Draw the Projectiles
                foreach (Projectile proj in this.controller.World.Projectiles.Values)
                {
                    DrawObjectWithTransform(e, proj, this.Size.Width, proj.Loc.GetX(), proj.Loc.GetY(), proj.Dir.ToAngle(), ProjDrawer);
                }
                // Draw the Star
                foreach (Star star in this.controller.World.Stars.Values)
                {
                    DrawObjectWithTransform(e, star, this.Size.Width, star.Loc.GetX(), star.Loc.GetY(), 0, StarDrawer);
                }
                // Do anything that Panel (from which we inherit) needs to do
                
            }
            base.OnPaint(e);
        }
        /// <summary>
        /// Draws a Star within the drawing panel. 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void StarDrawer(object o, PaintEventArgs e)
        {
            //EDIT
            int starWidth = 60;
            Rectangle r = new Rectangle(-(starWidth / 2), -(starWidth / 2), starWidth, starWidth);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Star s = o as Star;
            e.Graphics.DrawImage(starImage, r);
        }
        /// <summary>
        /// Draws a projectile within the Drawing Panel. 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjDrawer(object o, PaintEventArgs e)
        {
            //EDIT
            // set default projectile image
            Image projImage = projImageBlue;
            int projWidth = 25;
            Rectangle r = new Rectangle(-(projWidth / 2), -(projWidth / 2), projWidth, projWidth);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Projectile p = o as Projectile;
            switch (p.Owner % 8)
            {
                case 0:
                    projImage = projImageBlue;
                    break;
                case 1:
                    projImage = projImageBrown;
                    break;
                case 2:
                    projImage = projImageGreen;
                    break;
                case 3:
                    projImage = projImageGrey;
                    break;
                case 4:
                    projImage = projImageRed;
                    break;
                case 5:
                    projImage = projImageViolet;
                    break;
                case 6:
                    projImage = projImageWhite;
                    break;
                case 7:
                    projImage = projImageYellow;
                    break;
            }
            e.Graphics.DrawImage(projImage, r);

        }
        /// <summary>
        /// Draws a Ship within the drawing panel. 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ShipDrawer(object o, PaintEventArgs e)
        {
            // set default ship image
            Image shipImage = shipImageCoastBlue;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Ship s = o as Ship;
            //If the ship is below HP it might have been destroyed. 
            if (s.HP <= 0)
            { 
                //If the ship has been spawned in (in this context meaning that the ship has been initialized 
                //by the controller), then we know that if its hp is below 0 that the ship was destroyed, which 
                //means we should start a destruction sprite animation. 
                if (s.Spawned) {
                    int explWidth = 75;
                    //The section of the drawn image that will be determined by the death counter. 
                    Rectangle chooseSection = new Rectangle();
                    //How big the explosion is
                    Rectangle expRect = new Rectangle(-(explWidth / 2), -(explWidth / 2), explWidth, explWidth);
                    //If the deathcounter is too far along, do not initialize the draw and just return. 
                    bool sectionPotential = true;
                    switch (s.DeathCounter)
                    {
                        case int n when (n <=5):
                            chooseSection = upLeft;
                            break;
                        case int n when (n >6  && n <= 10 ):
                            chooseSection = upMid;
                            break;
                        case int n when (n > 11 && n <= 15):
                            chooseSection = upRight;
                            break;
                        case int n when (n > 16 && n <= 20):
                            chooseSection = midLeft;
                            break;
                        case int n when (n > 21 && n <= 25):
                            chooseSection = midMid;
                            break;
                        case int n when (n > 26 && n <= 30):
                            chooseSection = midRight;
                            break;
                        case int n when (n > 31 && n <= 35):
                            chooseSection = botLeft;
                            break;
                        case int n when (n > 36 && n <= 40):
                            chooseSection = botMid;
                            break;
                        case int n when (n > 41 && n <= 45):
                            chooseSection = botRight;
                            break;
                        default:
                            sectionPotential = false;
                            break;
                    }
                    if (sectionPotential)
                    {
                        e.Graphics.DrawImage(explosionSprite, expRect, chooseSection, GraphicsUnit.Pixel);
                    }
                }
                return;
            }
            int shipWidth = 35;
            Rectangle r = new Rectangle(-(shipWidth / 2), -(shipWidth / 2), shipWidth, shipWidth);
            switch (s.ID % 8)
            {
                case 0:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustBlue;
                    }
                    else
                    {
                        shipImage = shipImageCoastBlue;
                    }
                    break;
                case 1:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustBrown;
                    }
                    else
                    {
                        shipImage = shipImageCoastBrown;
                    }
                    break;
                case 2:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustGreen;
                    }
                    else
                    {
                        shipImage = shipImageCoastGreen;
                    }
                    break;
                case 3:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustGrey;
                    }
                    else
                    {
                        shipImage = shipImageCoastGrey;
                    }
                    break;
                case 4:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustRed;
                    }
                    else
                    {
                        shipImage = shipImageCoastRed;
                    }
                    break;
                case 5:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustViolet;
                    }
                    else
                    {
                        shipImage = shipImageCoastViolet;
                    }
                    break;
                case 6:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustWhite;
                    }
                    else
                    {
                        shipImage = shipImageCoastWhite;
                    }
                    break;
                case 7:
                    if (s.Thrust)
                    {
                        shipImage = shipImageThrustYellow;
                    }
                    else
                    {
                        shipImage = shipImageCoastYellow;
                    }
                    break;
            }
            e.Graphics.DrawImage(shipImage, r);
        }
    }
}
