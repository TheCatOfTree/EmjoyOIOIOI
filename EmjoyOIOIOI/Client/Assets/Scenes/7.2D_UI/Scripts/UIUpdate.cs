using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Common;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoSingleton<UIUpdate>
{
    public Text time;

    // Update is called once per frame
    public void TimeUpdate(MyGame.S2C_FameMsg game)
    {
        time.text = "剩余时间：" + game.NowTime.ToString();
    }
}