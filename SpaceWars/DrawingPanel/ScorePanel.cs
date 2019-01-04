using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controller;
using WorldObjects;

namespace DrawingAux
{
    public class ScorePanel : Panel
    {
        //controller to reference the important ship data to draw on the scoreboard. 
        SpaceController controller;

        public ScorePanel(SpaceController controller)
        {
            DoubleBuffered = true;
            this.controller = controller;
        }
        /// <summary>
        /// Overriden method to draw different components into the view. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        { 
            
            Font defaultFont = new Font("Arial", 14);
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            int margin = 0;
            lock (this.controller.World)
            {
                foreach (Ship s in controller.World.Ships.Values)
                {
                    e.Graphics.DrawString(s.Name + ": " + s.Score, defaultFont, blackBrush, 5, margin);
                    margin += 25; 

                    int xRectBuffer = (this.Size.Width-10) / s.InitialHp;
                    int xCurrent = 5;
                    e.Graphics.DrawRectangle(new Pen(blackBrush), new Rectangle(xCurrent - 2, margin - 2, this.Size.Width - 7, 19));
                    for (int currHP = s.HP; currHP > 0; currHP--)
                    {
                        e.Graphics.FillRectangle(brushDecider(s.HP, s.InitialHp), new Rectangle(xCurrent, margin, xRectBuffer, 16));
                        xCurrent += xRectBuffer;
                    }
                    margin += 20;
                }
            }
            e.Graphics.ResetTransform();
            base.OnPaint(e);
        }
        /// <summary>
        /// A polished component to determine different componets
        /// </summary>
        /// <param name="currentHp"></param>
        /// <param name="initialHP"></param>
        /// <returns></returns>
        private SolidBrush brushDecider(int currentHp, int initialHP) {
            int ranges = initialHP / 3;
            if (currentHp <= ranges)
            {
                return new SolidBrush(Color.Red);
            }
            else if (currentHp > ranges && currentHp <= ranges * 2)
            {
                return new SolidBrush(Color.Yellow);
            }
            else {
                return new SolidBrush(Color.Green);
            }
            
        }

    }
}
