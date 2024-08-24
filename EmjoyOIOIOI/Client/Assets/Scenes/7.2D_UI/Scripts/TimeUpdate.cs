using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;

public class TimeUpdate : MonoSingleton<TimeUpdate>
{
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetComponent<Text>();
    }

    // Update is called once per frame
    public void timeUpdate(MyGame.S2C_FameMsg game)
    {
        text.text = "剩余时间：" + game.NowTime.ToString();
    }
}
