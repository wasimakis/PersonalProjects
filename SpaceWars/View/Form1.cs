using Controller;
using System;
using Setting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DrawingAux;
using System.Threading;

namespace View
{
    public partial class Form1 : Form
    {
        SpaceController controller;
        DrawingAux.DrawingPanel drawingPanel;

        ScorePanel scorePanel;

        public Form1()
        {
            InitializeComponent();
            controller = new SpaceController();
            controller.AttemptConnect += recallConnect;
            controller.ServerUpdater += UpdateWorld;
            controller.WorldUpdater += SizeWorldChanger;
            controller.connectionFailure += connectionFailed;

            
            drawingPanel = new DrawingAux.DrawingPanel(controller)
            {

                Location = new Point(12, 46),
                Size = new Size(250, 250),
                BackColor = System.Drawing.Color.Black 
               
              
            };
           
            scorePanel = new DrawingAux.ScorePanel(controller)
            {   
                Location = new Point(connectButton.Location.X + connectButton.Size.Width, 46), 
                Size = new Size(250, 250), 
                BackColor = System.Drawing.Color.White

            };
            this.scoreLabel.Location =  new Point(this.scorePanel.Location.X + (this.scorePanel.Width / 2) - (this.scoreLabel.Width/2), this.scoreLabel.Location.Y);
            this.Controls.Add(drawingPanel);
            this.Controls.Add(scorePanel);
            this.Enabled = true;
            


        }

        private void connectionFailed()
        {
            MethodInvoker invoker = new MethodInvoker(() => {
                MessageBox.Show(this, "Connection Failed!\nDoes this server actually run spacewars?");
                serverBox.Enabled = true;
                connectButton.Enabled = true;
                nameBox.Enabled = true;
                this.label1.Visible = false;
                this.label2.Visible = false;
                }
            );
            
            
            
            
            
            this.Invoke(invoker);


        }

        private void SizeWorldChanger()
        {
            MethodInvoker invoker = new MethodInvoker(() => this.WorldSizeChanger());
            this.Invoke(invoker);
            
        }

        private void WorldSizeChanger()
        {
           
            drawingPanel.Size = new Size(controller.World.WorldSize, controller.World.WorldSize);
            if (!(controller.World.WorldSize < connectButton.Location.X + connectButton.Size.Width))
            {
                scorePanel.Location = new Point(controller.World.WorldSize + 32, 46);
                scorePanel.Size = new Size(scorePanel.Size.Width, controller.World.WorldSize);
                this.scoreLabel.Location = new Point(this.scorePanel.Location.X + (this.scorePanel.Width / 2) - (this.scoreLabel.Width / 2), this.scoreLabel.Location.Y);
                this.Size = new Size(this.scorePanel.Size.Width + this.drawingPanel.Size.Width + 70, this.scorePanel.Size.Height + 100);
            }
            this.label1.Visible = true;
            this.label2.Visible = true; 

        }

        // Redraw the game. This method is invoked every time there is a deserialized message
        private void UpdateWorld()
        { 
            
            // Invalidate this form and all its children
            // This will cause the form to redraw as soon as it can 
            try
            {
                MethodInvoker drawingInvoker = new MethodInvoker(() => this.drawingPanel.Invalidate());
                this.Invoke(drawingInvoker);
                MethodInvoker scoreInvoker = new MethodInvoker(() => this.scorePanel.Invalidate());
                this.Invoke(scoreInvoker);
            }
            catch (Exception e)
            {
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (nameBox.Text.Length == 0) {
                MessageBox.Show("Player name cannot be empty!");
            } else {
                try
                {
                    this.controller.ServerConnect(serverBox.Text, nameBox.Text);
                }
                catch (Exception ex) {
                    MessageBox.Show(this, ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void recallConnect()
        {
            serverBox.Enabled = false;
            connectButton.Enabled = false;
            nameBox.Enabled = false;
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            this.controller.FlagChanger(e.KeyCode, true);
            e.Handled = false;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            this.controller.FlagChanger(e.KeyCode, false);
            e.Handled = false;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'h') {
                MessageBox.Show(this, "Up: Thrust! \n Left: Turn left \n Right: Turn right \n Space: Fire!", "Controls", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (e.KeyChar == 'a') {
                MessageBox.Show(this, "Space Wars Client!\nCreated by William Asimakis and Joshua Call\n Art by Alex Smith", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

   

        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Space Wars Client!\nCreated by William Asimakis and Joshua Call\n Ship Art by Alex Smith\n Explosion Art by William Asimakis", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Up: Thrust! \n Left: Turn left \n Right: Turn right \n Space: Fire!", "Controls", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
