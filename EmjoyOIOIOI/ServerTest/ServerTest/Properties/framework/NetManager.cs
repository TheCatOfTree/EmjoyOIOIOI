using System.Data;
using System.Net;
using System.Net.Sockets;
using ConsoleApp1.framework.proto;
using ConsoleApp1.lib;
using Google.Protobuf;

namespace ConsoleApp1.framework;

using System;

public class NetManager : Singleton<NetManager>
{
    private Socket listener;

    public void Start()
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(new IPEndPoint(IPAddress.Any, 8848));
        listener.Listen(20);
        listener.BeginAccept(acceptCallBack, null);
    }

    private void acceptCallBack(IAsyncResult ar)
    {
        Socket clientSocket = listener.EndAccept(ar);

        Console.WriteLine($"客户端连接 IP{clientSocket.RemoteEndPoint}");

        Client cli = new Client(clientSocket);

        new Notification((int)MsgIDDefine.ClientConnectedID, null, cli).Send();

        clientSocket.BeginReceive(cli.buffer, 0, cli.buffer.Length, SocketFlags.None, ReceiveCallBack, cli);

        listener.BeginAccept(acceptCallBack, null);
    }

    private void ReceiveCallBack(IAsyncResult ar)
    {
        Client cli = ar.AsyncState as Client;
        int len = 0;
        try
        {
            len = cli.clientSocket.EndReceive(ar);
        }
        catch (Exception a)
        {
            Console.WriteLine(a.Message);
        }

        if (len > 0)
        {
            byte[] tmp = new byte[len];
            Buffer.BlockCopy(cli.buffer, 0, tmp, 0, len);

            cli.myReceiveBuffer.Position = cli.myReceiveBuffer.Length;
            cli.myReceiveBuffer.Write(tmp, 0, tmp.Length);

            while (true)
            {
                cli.myReceiveBuffer.Position = 0;
                ushort bodyLen = cli.myReceiveBuffer.ReadUshort();
                ushort fullLen = (ushort)(bodyLen + 2);
                if (cli.myReceiveBuffer.Length >= fullLen)
                {
                    cli.myReceiveBuffer.Position = 2;
                    bool isCompress = cli.myReceiveBuffer.ReadBool();
                    ushort crc = cli.myReceiveBuffer.ReadUshort();

                    byte[] data = new byte[bodyLen - 3];
                    cli.myReceiveBuffer.Read(data, 0, data.Length);

                    ushort newCrc = Crc16.CalculateCrc16(data);
                    if (newCrc == crc)
                    {
                        data = SecurityUtil.Xor(data);
                        if (isCompress)
                        {
                            data = ZlibHelper.DeCompressBytes(data);
                        }

                        int msgID = BitConverter.ToInt32(data, 0);
                        byte[] pbData = new byte[data.Length - 4];
                        Buffer.BlockCopy(data, 4, pbData, 0, pbData.Length);

                        #region 消息的派发

                        new Notification(msgID, pbData, cli).Send();

                        #endregion
                    }

                    ushort remainLen = (ushort)(cli.myReceiveBuffer.Length - fullLen);
                    if (remainLen > 0)
                    {
                        byte[] remainArr = new byte[remainLen];
                        cli.myReceiveBuffer.Position = fullLen;
                        cli.myReceiveBuffer.Read(remainArr, 0, remainArr.Length);

                        cli.myReceiveBuffer.SetLength(0);
                        cli.myReceiveBuffer.Position = 0;

                        cli.myReceiveBuffer.Write(remainArr, 0, remainArr.Length);

                    }
                    else
                    {
                        cli.myReceiveBuffer.SetLength(0);
                        cli.myReceiveBuffer.Position = 0;
                        break;
                    }

                }
                else
                {
                    break;
                }
            }

            cli.clientSocket.BeginReceive(cli.buffer, 0, cli.buffer.Length, SocketFlags.None, ReceiveCallBack, cli);
        }else {
           Console.WriteLine("结束连接");
        }
    }
    
    public void sendMsgToClient(MsgIDDefine id, IMessage msg, Client cli)
    {
        MyMemoryStream m = new MyMemoryStream();
        m.WriteInt((int)id);
        byte[] bpData = msg.ToByteArray();
        m.Write(bpData, 0, bpData.Length);
        byte[] data = m.ToArray();
        m.Close();
        data = makeData(data);
        this.sendMsgToClient(data, cli);
    }

    /// <summary>
    /// 封装数据包。
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private byte[] makeData(byte[] data)
    {
        bool isCompress = false;
        if (data.Length > 200)
        {
            isCompress = true;
            data = ZlibHelper.CompressBytes(data);
        }
        data = SecurityUtil.Xor(data); //加密
        ushort crc = Crc16.CalculateCrc16(data);  //校验码。
        ushort bodyLen = (ushort)(data.Length + 3);  //包体长度

        MyMemoryStream m = new MyMemoryStream();
        m.WriteUShort(bodyLen);
        m.WriteBool(isCompress);
        m.WriteUShort(crc);
        m.Write(data, 0, data.Length);
        data = m.ToArray();
        m.Close();
        return data;
    }
    public void sendMsgToClient(byte[] data, Client cli)
    {
        try
        {
            if (cli != null && cli.clientSocket != null && cli.clientSocket.Connected) /// socket.Connected为true，则链接状态下。
            {
                cli.clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, sendCallback, cli);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(cli + "掉线了！\n" + ex.Message);
        }

    }
    private void sendCallback(IAsyncResult ar)
    {
        Client cli = ar.AsyncState as Client;
        try
        {
            int len = cli.clientSocket.EndSend(ar);
            Console.WriteLine("发送成功 字节数=" + len);
        }
        catch (Exception)
        {
            Console.WriteLine("130和客户端之间断开链接了。 客户端的ip=" + cli.clientSocket.RemoteEndPoint);
            ///通知模块有客户端掉线了。
            new Notification((int)MsgIDDefine.ClientClosedID, null, cli).Send();
        }
    }
}


public class Client
{
    public Socket clientSocket;
    public byte[] buffer = new byte[2048];

    public MyMemoryStream myReceiveBuffer = new MyMemoryStream();

    public Client(Socket sc)
    {
        this.clientSocket = sc;
    }
}