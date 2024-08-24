using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace Common
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get { return _instance; }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
}

class Level
{
    public float Time;
    private int bout;
    private int count;
}

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject canvas;
    public bool GamePlaying = false;
    private bool GameReady;
    public Button Ready;
    public Image Back;
    public string ip;

    private List<Level> leve = new List<Level>();

    //Get in Sever
    private int userid;
    public bool Drag = false;

    public bool canSend = false;

    //游戏剩余时间
    public int gameTime;

    //init in Client
    //游戏回合数
    public int bout;

    //每回合的拖拽次数
    public int count;

    //游戏得分
    public int score = 0;

    private MyGame.C2S_OperationMsg m = new MyGame.C2S_OperationMsg();

    private Dictionary<int, GameObject> allCLientDic = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate(canvas, Vector3.zero, quaternion.identity).name="Map";
        Netmanager.GetInstance().Connect(ip, 8848);

        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_ConnectResponseMsgID, connecthandler);
        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_FameMsgID, logicUpdate);
    }

    private void connecthandler(Notification obj)
    {
        MyGame.S2C_ConnectResponseMsg m = MyGame.S2C_ConnectResponseMsg.Parser.ParseFrom(obj.content);
        this.userid = m.Userid;

        m.Userid = this.userid;
    }

    private void logicUpdate(Notification obj)
    {
        MyGame.S2C_FameMsg m = MyGame.S2C_FameMsg.Parser.ParseFrom(obj.content);

        if (m.NowTime != gameTime)
        {
            gameTime = m.NowTime;
            GamePlaying = m.GamePlaying;
            TimeUpdate.Instance.timeUpdate(m);
        }

        if (m.OperationList.Count > 0) //不是空帧
        {
            foreach (var operation in m.OperationList)
            {
                if (allCLientDic.ContainsKey(operation.Userid))
                {
                    ScoreUpdate score = allCLientDic[operation.Userid].GetComponent<ScoreUpdate>();
                    score.UpdateScore(operation);
                }
                else
                {
                    GameObject prefab = Resources.Load<GameObject>("PlayerScores");
                    GameObject go = GameObject.Instantiate<GameObject>(prefab);
                    go.transform.parent = GameObject.Find("PlayerScore").transform;
                    allCLientDic.Add(operation.Userid, go);

                    ScoreUpdate ctrl = go.GetComponent<ScoreUpdate>();
                    ctrl.UpdateScore(operation);
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
        GameReady = true;
        Ready.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Netmanager.GetInstance().Update();
        m.Userid = this.userid;

        if (GamePlaying)
        {
            if (Back.IsActive())
            {
                Back.gameObject.SetActive(false);
            }

            if (Drag)
            {
                m.Drag = true;
                Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
                Drag = false;
            }

            if (canSend)
            {
                m.Score = this.score;
                Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_OperationMsgID, m);
                canSend = false;
            }
        }

        if (GameReady)
        {
            m.Ready = true;
            Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_LoginMsgId, m);
            GameReady = false;
        }
    }
}