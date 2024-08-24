using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using Google.Protobuf;

public class Singleton<T> where T : class, new()
{
    private static T _instance;
    private static object obj = new object();

    public static T GetInstance()
    {
        if (_instance == null)
        {
            lock (obj)
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
            }
        }
        return _instance;

    }
}


public class Netmanager : Singleton<Netmanager>
{
    
    private MyMemoryStream myReceiveBuffer;
     private Socket socket;
     private byte[] buffer = new byte[2048];  ///游戏中2048就够了。存储我们收到的数据的缓冲区

     ///存放收取到的数据。
     public Queue<byte[]> msgQueue = new Queue<byte[]>();

     /// <summary>
     /// 和服务器建立链接
     /// </summary>
     /// <param name="ip"></param>
     /// <param name="port"></param>
     public void Connect(string ip, int port)
     {
         myReceiveBuffer = new MyMemoryStream();
         socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
         socket.BeginConnect(ip, port, connectCallback, null);
         Debug.Log("不阻塞");
     }
  private void connectCallback(IAsyncResult ar)
    {
        //开始和服务器进行链接
        try
        {
            socket.EndConnect(ar);
            Debug.Log("链接成功");
            //记住，一连接成功，就开始收数据。！！！！！！！！！！！！！
            socket.BeginReceive(this.buffer, 0, buffer.Length, SocketFlags.None,new AsyncCallback(receiveCallback) , null);
        }
        catch (Exception ex)
        {
            Debug.Log("链接失败" + ex.Message);
        }
    }

 private void receiveCallback(IAsyncResult ar)
    {
        int len = 0;
        try
        {
            len = socket.EndReceive(ar);  //len为真实的收到的字节数
        }
        catch (Exception)
        {
            Debug.Log("链接断了。");
        }

        if (len > 0)
        {
            byte[] tmp = new byte[len];
            Buffer.BlockCopy(this.buffer, 0, tmp, 0, len);

            //每次收到数据，把数据写入流末尾
            myReceiveBuffer.Position = myReceiveBuffer.Length;///流里的读写的起始位置，设定到流的末尾
            myReceiveBuffer.Write(tmp, 0, tmp.Length); //把新收到的数据追加到流的末尾。

            ///处理粘包的循环
            while (true)
            {
                //1.从流的开头读取包体长度
                myReceiveBuffer.Position = 0;
                ushort bodyLen = myReceiveBuffer.ReadUshort();
                ushort fullLen = (ushort)(bodyLen + 2);
                if (myReceiveBuffer.Length >= fullLen) //说明够一个完整的包了。
                {
                    myReceiveBuffer.Position = 2; //从是否压缩的标识的位置开始读取标识。
                    ushort crc = myReceiveBuffer.ReadUshort();
                    //读取数据部分
                    byte[] data = new byte[bodyLen - 2];
                    myReceiveBuffer.Read(data, 0, data.Length);

                    //////以上是 ，该读取的都读取出来了。。。
                    ushort newCrc = Crc16.CalculateCrc16(data);
                    if (newCrc == crc) //校验通过。
                    {
                        data = SecurityUtil.Xor(data); //解密
                        ///把接出来的数据包放入队列。主线程中再处理数据。
                        msgQueue.Enqueue(data);///放入队列等待主线程处理。
                    }

                    ///剩余数据在容器中保留，刚处理完的，删除。
                    ushort remainLen = (ushort)(myReceiveBuffer.Length - fullLen);
                    if (remainLen > 0)
                    {
                        byte[] remainArr = new byte[remainLen];
                        myReceiveBuffer.Position = fullLen;
                        myReceiveBuffer.Read(remainArr, 0, remainArr.Length);

                        ///清楚容器（myReceiveBuffer）里的所有内容
                        myReceiveBuffer.SetLength(0);
                        myReceiveBuffer.Position = 0;

                        //剩余部分再写回来
                        myReceiveBuffer.Write(remainArr, 0, remainArr.Length);

                    }
                    else
                    {
                        ///剩余部分长度为 0，，清楚容器（myReceiveBuffer）里的所有内容
                        myReceiveBuffer.SetLength(0);
                        myReceiveBuffer.Position = 0;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            ///再次收下次的数据，否则只能收取一次数据。
            this.socket.BeginReceive(this.buffer, 0, buffer.Length, SocketFlags.None, receiveCallback, null);
        }
        else  //len==0链接断了
        {
            Debug.Log("链接断了。");
        }

    }

    /// <summary>
    /// 和服务器断开链接的代码
    /// </summary>
    public void Close()
    {
        if (socket != null && socket.Connected) /// socket.Connected为true，则链接状态下。
        {
            this.socket.Shutdown(SocketShutdown.Both);  //数据的收/发停止掉。
            this.socket.Close();
        }
    }

    #region 发送消息的方法。
    /// <summary>
    /// 封装数据包。
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private byte[] makeData(byte[] data)
    {

        data = SecurityUtil.Xor(data); //加密
        ushort crc = Crc16.CalculateCrc16(data);  //校验码。
        ushort bodyLen = (ushort)(data.Length + 2);  //包体长度
        MyMemoryStream m = new MyMemoryStream();
        m.WriteUShort(bodyLen);
        m.WriteUShort(crc);
        m.Write(data, 0, data.Length);
        data = m.ToArray();
        m.Close();

        return data;

    }
    public void sendMsgToServer(MsgIDDefine id, IMessage msg)
    {
        ///把id和pb内容封装成一个数据包(byte[])  ,此处容易出错。。。
        byte[] idArr = BitConverter.GetBytes((int)id);
        byte[] pbData = msg.ToByteArray();
        byte[] content = new byte[idArr.Length + pbData.Length];
        Buffer.BlockCopy(idArr, 0, content, 0, idArr.Length);
        Buffer.BlockCopy(pbData, 0, content, idArr.Length, pbData.Length);///
        ///调用发送byte[]的方法，给发送出去。
     
        this.sendMsgToServer(content);
    }

    public void sendMsgToServer(byte[] data)
    {
        ///发送之前包装一下。
        data = makeData(data);

        if (socket != null && socket.Connected) /// socket.Connected为true，则链接状态下。
        {
            try
            {
                this.socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, null);
            }
            catch (Exception)
            {
                Debug.Log("链接断了。");
            }
        }
    }

    public void sendMsgToServer(string msg)
    {
        byte[] data = UTF8Encoding.UTF8.GetBytes(msg);
        this.sendMsgToServer(data);
    }



    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            int len = this.socket.EndSend(ar);
            Debug.Log("发送成功 实际发送成功字节数=" + len);
        }
        catch (Exception)
        {
            Debug.Log("链接断了。");
        }
    }
    #endregion
    
    public void Update()
    {
        //从存放数据的队列中取数据，然后进行处理。
        
        while (msgQueue.Count > 0)
        {
            byte[] data = msgQueue.Dequeue();
            //从消息中拆分出消息id和pb内容两部分
            int msgID = BitConverter.ToInt32(data, 0);
            byte[] pbData = new byte[data.Length - 4];
            Buffer.BlockCopy(data, 4, pbData, 0, pbData.Length);

            #region 消息的派发
            new Notification(msgID, pbData).Send();
            #endregion
        }

    }
}


