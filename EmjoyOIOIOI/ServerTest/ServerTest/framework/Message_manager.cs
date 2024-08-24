using System;
using System.Collections.Generic;

namespace ServerTest.framework
{
    public class Notification
    {
        public int id;
        public Client client;
        public byte[] content;

        public Notification(int id, byte[] content, Client cli)
        {
            this.id = id;
            this.content = content;
            this.client = cli;
        }

        public void Send()
        {
            Message_manager.GetInstance().Dispatch(this.id,this);
        }
    }

    public class Message_manager : Singleton<Message_manager>
    {
        private Dictionary<int, Action<Notification>> dic = new Dictionary<int, Action<Notification>>();

        ///<sumary>
        /// 监听的消息
        ///</sumary>
        ///<param name ="id"></param>
        ///<param name ="action"></param>
        public void Addlistener(int id, Action<Notification> action)
        {
            if (dic.ContainsKey(id))
            {
                dic[id] += action;
            }
            else
            {
                dic.Add(id, action);
            }
        }

        /// <summary>
        /// 消息的派发
        /// </summary>
        /// <param name="id"></param>
        /// <param name="noti"></param>
        public void Dispatch(int id, Notification noti)
        {
            if (dic.ContainsKey(id))
            {
                dic[id]?.Invoke(noti);
            }
            else
            {
                Console.WriteLine("消息ID={0} 没有任何模块监听", id);
            }
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        /// <param name="id"></param>
        /// <param name="action"></param>
        public void RemoveListener(int id, Action<Notification> action)
        {
            if (dic.ContainsKey(id))
            {
                dic[id] -= action;
                if (dic[id] == null)
                {
                    dic.Remove(id);
                }
            }
        }
    }

}