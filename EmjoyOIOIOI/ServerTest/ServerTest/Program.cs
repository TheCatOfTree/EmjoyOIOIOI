using System;
using ServerTest.framework;
using ServerTest.GameControll;

namespace ServerTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            NetManager.GetInstance().Start();
            Console.WriteLine("网络管理启动");
            
            GameManager.GetInstance().Init();
            Console.WriteLine("游戏逻辑启动");
            while (true)
            {

            }
        }
    }
}