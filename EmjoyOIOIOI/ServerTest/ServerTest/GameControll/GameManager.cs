using System.Collections.Generic;
using System.Threading;
using MyGame;
using ServerTest.framework;
using ServerTest.framework.proto;

namespace ServerTest.GameControll
{
    public class GameManager : Singleton<GameManager>
    {
        private Dictionary<int, Client> allClients = new Dictionary<int, Client>();
        private object obj = new object();
        private int clientID = 0;
        private int clientReady = 0;
        MyTimer time = new MyTimer();
        private List<MyGame.C2S_OperationMsg> operationMsgList = new List<C2S_OperationMsg>();

        public void Init()
        {
            Message_manager.GetInstance().Addlistener((int)MsgIDDefine.ClientConnectedID, ClientConnect);
            Message_manager.GetInstance().Addlistener((int)MsgIDDefine.C2S_OperationMsgID, operationMsgHandler);
            Message_manager.GetInstance().Addlistener((int)MsgIDDefine.C2S_LoginMsgId, loginHandler);
        }

        private void loginHandler(Notification obj)
        {
            MyGame.C2S_OperationMsg msg = MyGame.C2S_OperationMsg.Parser.ParseFrom(obj.content);
            if (msg.Ready)
                clientReady++;
            if (clientID == 2 && clientReady == 2)
            {
                time.StartCountdown(0, 0, 60);
                ///开启一个500ms执行一次的逻辑
                ThreadPool.QueueUserWorkItem(sendFrameLoop);
            }
        }

        private void operationMsgHandler(Notification obj)
        {
            MyGame.C2S_OperationMsg msg = MyGame.C2S_OperationMsg.Parser.ParseFrom(obj.content);
            operationMsgList.Add(msg);
        }

        private void ClientConnect(Notification obj)
        {
            clientID++;
            allClients.Add(clientID, obj.client);

            ///发消息告诉客户端，你的ID是什么
            MyGame.S2C_ConnectResponseMsg m = new MyGame.S2C_ConnectResponseMsg();
            m.Userid = clientID;
            NetManager.GetInstance().sendMsgToClient(MsgIDDefine.S2C_ConnectResponseMsgID, m, obj.client);
        }

        private void sendFrameLoop(object state)
        {
            ///每50毫秒执行。
            while (true)
            {
                broadcastOperationMsg();
                Thread.Sleep(500);
            }
        }

        private void broadcastOperationMsg()
        {
            MyGame.S2C_FameMsg m = new MyGame.S2C_FameMsg();
            lock (obj)
            {
                if (time.totalSeconds > 0)
                {
                    m.GamePlaying = true;
                }
                else
                {
                    m.GamePlaying = false;
                }

                m.NowTime = time.totalSeconds;
                foreach (var item in operationMsgList)
                {
                    m.OperationList.Add(item);
                }

                operationMsgList.Clear();
            }

            ///广播给所有客户端，操作的消息。
            foreach (var item in allClients.Values)
            {
                NetManager.GetInstance().sendMsgToClient(MsgIDDefine.S2C_FameMsgID, m, item);
            }
            //清空操作列表。
        }
    }
}