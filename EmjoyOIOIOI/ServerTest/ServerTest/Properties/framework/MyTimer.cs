using System;
using System.Windows.Forms;
namespace countdown_timer
{
    public class MyTimer
    {
        public MyTimer()
        {
            InitializeComponent();
        }
        private int duration = 60;
        private void button1_Click(object sender, EventArgs e) {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(count_down);
            timer1.Interval = 1000;
            timer1.Start();
        }
        private void count_down(object sender, EventArgs e) {
            if (duration == 0) {
                timer1.Stop();

            } else if (duration > 0) {
                duration--;
                label1.Text = duration.ToString();
            }
        }


    }
}