using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Common;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


public class Level
{
    public float time;
    private int bout;
    public int count;
}

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject PlayGame;
    public GameObject canvas;
    public GameObject prefabMap;

    public bool gamePlaying = false;
    private bool gameReady;
    public Button ready;
    public Image back;

    public Level one;

    //Get in Sever
    public int userid;
    public bool drag = false;

    public bool canSend = false;

    //游戏剩余时间
    public int gameTime;

    //init in Client
    //游戏回合数
    public int bout;


    //游戏得分
    public int score;

    private MyGame.C2S_OperationMsg m = new MyGame.C2S_OperationMsg();

    private Dictionary<int, GameObject> allCLientDic = new Dictionary<int, GameObject>();


    protected override void Awake()
    {
        base.Awake();
        one = new Level();
        one.count = 20;
        one.time = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate(canvas, Vector3.zero, quaternion.identity).name="Map";
        Netmanager.GetInstance().Connect("127.0.0.1", 8848);
        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_ConnectResponseMsgID, ConnectHandler);
        Message_manager.GetInstance().Addlistener((int)MsgIDDefine.S2C_GamePlaying, GamePlaying);
    }

    private void GamePlaying(Notification obj)
    {
        gamePlaying = true;
    }

    private void ConnectHandler(Notification obj)
    {
        MyGame.S2C_ConnectResponseMsg m = MyGame.S2C_ConnectResponseMsg.Parser.ParseFrom(obj.content);

        this.userid = m.Userid;
    }

    public void ChangeGameReady()
    {
        gameReady = true;
        ready.gameObject.SetActive(false);
    }

    public void CreateGame()
    {
        PlayGame.SetActive(false);
        GameObject go = Instantiate(prefabMap, canvas.transform);
        go.GetComponent<GameLogic>().playerId = userid;
        ready = go.transform.Find("Image/Button (Legacy)").GetComponent<Button>();
        back = go.transform.Find("Image").GetComponent<Image>();
        ready.onClick.AddListener(ChangeGameReady);
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
        }


        if (gameReady)
        {
            m.Ready = true;
            m.Count = one.count;
            m.Userid = userid;
            Netmanager.GetInstance().sendMsgToServer(MsgIDDefine.C2S_LoginMsgId, m);
            gameReady = false;
        }
    }
}