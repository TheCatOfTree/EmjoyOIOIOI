using System;
using System.Windows.Forms;

namespace ServerTest.framework
{
    
    public partial class MyTimer : Form
    {
        public int totalSeconds;
        private System.Threading.Timer timer;

        public MyTimer() { }

        public void StartCountdown(int hours, int minutes, int seconds)
        {
            totalSeconds = hours * 3600 + minutes * 60 + seconds;
            Console.WriteLine($"剩余时间：{totalSeconds} 秒");

            timer = new System.Threading.Timer(TimerCallback, null, 1000, 1000);
        }

        private void TimerCallback(object state)
        {
            if (totalSeconds > 0)
            {
                totalSeconds--;
                Console.WriteLine($"剩余时间：{totalSeconds} 秒");
            }
            else
            {
                timer.Dispose();
                Console.WriteLine("倒计时结束！");
            }
        }
        
    }
}