using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


class Level
{
    public float time;
    private int bout;
    private int count;
}

public class GameManager : MonoSingleton<GameManager>
{
    public bool gamePlaying = false;
    private bool gameReady;
    public Button ready;
    public Image back;

    private List<Level> leve = new List<Level>();

    //Get in Sever
    private int userid;
    public bool drag = false;

    public bool canSend = false;

    //游戏剩余时间
    public int gameTime;

    //init in Client
    //游戏回合数
    public int bout;

    //每回合的拖拽次数
    public int count;

    //游戏得分
    public int score;

    private MyGame.C2S_OperationMsg m = new MyGame.C2S_OperationMsg();

    private Dictionary<int, GameObject> allCLientDic = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate(canvas, Vector3.zero, quaternion.identity).name="Map";
        Netmanager.GetInstance().Connect("127.0.0.1", 8848);

        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_ConnectResponseMsgID, ConnectHandler);
        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_FameMsgID, LogicUpdate);
    }

    private void ConnectHandler(Notification obj)
    {
        MyGame.S2C_ConnectResponseMsg m = MyGame.S2C_ConnectResponseMsg.Parser.ParseFrom(obj.content);

        this.userid = m.Userid;
    }

    private void LogicUpdate(Notification obj)
    {
        MyGame.S2C_FameMsg m = MyGame.S2C_FameMsg.Parser.ParseFrom(obj.content);

        if (m.NowTime != gameTime)
        {
            gameTime = m.NowTime;
            gamePlaying = m.GamePlaying;
            UIUpdate.Instance.TimeUpdate(m);
        }

        if (m.OperationList.Count > 0) //不是空帧
        {
            foreach (var operation in m.OperationList)
            {
                if (allCLientDic.TryGetValue(operation.Userid, out var value))
                {
                    ScoreUpdate score = value.GetComponent<ScoreUpdate>();
                    score.UpdatePlayer(operation);
                }
                else
                {
                    GameObject prefab = Resources.Load<GameObject>("PlayerScores");
                    GameObject go = GameObject.Instantiate(prefab, GameObject.Find("PlayerScore").transform, true);
                    allCLientDic.Add(operation.Userid, go);

                    ScoreUpdate score = go.GetComponent<ScoreUpdate>();
                    score.UpdatePlayer(operation);
                }
            }
        }
        else //空帧 情况下；
        {
            foreach (var go in allCLientDic.Values)
            {
                // ScoreUpdate ctrl = go.GetComponent<ScoreUpdate>();
                // ctrl.UpdateScore(null);
            }
        }

        gameTime = m.NowTime;
    }

    public void ChangeGameReady()
    {
        gameReady = true;
        ready.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Netmanager.GetInstance().Update();
        m.Userid = this.userid;

        if (gamePlaying)
        {
            if (back.IsActive())
            {
                back.gameObject.SetActive(false);
            }

            if (drag)
            {
                m.Drag = true;
                m.Count = --count;
                Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
                drag = false;
            }

            if (canSend)
            {
                m.Score = this.score;
                Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
                canSend = false;
            }
        }

        if (gameReady)
        {
            m.Ready = true;
            m.Count = count;
            Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_LoginMsgId, m);
            gameReady = false;
        }
    }
}