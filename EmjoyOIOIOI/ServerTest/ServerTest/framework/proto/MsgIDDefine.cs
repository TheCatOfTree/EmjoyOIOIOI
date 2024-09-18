namespace ServerTest.framework.proto
{
    public enum MsgIDDefine
    {/// <summary>
        /// 上行协议的编号
        /// </summary>
        C2S_OperationMsgID= 10001,
        C2S_LoginMsgId=10002,
        /// <summary>
        /// 下行协议
        /// </summary>
        S2C_ConnectResponseMsgID = 20001,
        S2C_GamePlaying=20002,
        S2C_FameMsgID= 20003,
    
        /// <summary>
        /// 内部传输协议
        /// </summary>
        ClientConnectedID = 30001,
        ClientClosedID = 30002,
    
    }
}