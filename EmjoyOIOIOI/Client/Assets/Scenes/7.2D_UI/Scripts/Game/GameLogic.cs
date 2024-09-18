using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 开启一场对局
//主机  map  客机
public class GameLogic : MonoBehaviour
{
    private GameObject player1;
    private GameObject player2;

    public int playerId;
    public int gameTime; //游戏时间

    private int[] id;

    //每回合的拖拽次数
    public int count;
    public int score = 0;

    private GameObject prefab;
    private Dictionary<int, GameObject> allCLientDic = new Dictionary<int, GameObject>();
    private MyGame.C2S_OperationMsg m = new MyGame.C2S_OperationMsg();

    private void Awake()
    {
        prefab = Resources.Load<GameObject>("PlayerScores");
    }

    // Start is called before the first frame update
    void Start()
    {
        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_FameMsgID, LogicUpdate);
    }

    private void LogicUpdate(Notification obj)
    {
        MyGame.S2C_FameMsg m = MyGame.S2C_FameMsg.Parser.ParseFrom(obj.content);

        if (m.NowTime != gameTime)
        {
            gameTime = m.NowTime;
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

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.drag)
        {
            m.Drag = true;
            m.Count = --count;
            Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
            GameManager.Instance.drag = false;
        }

        if (GameManager.Instance.canSend)
        {
            m.Score = score;
            m.Userid = playerId;
            Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
            GameManager.Instance.canSend = false;
        }
    }
}