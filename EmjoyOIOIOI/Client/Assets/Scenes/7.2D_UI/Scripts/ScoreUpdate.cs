using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using MyGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUpdate : MonoBehaviour
{
    public Text text;

    public Text count;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update score
    public void UpdatePlayer(C2S_OperationMsg item)
    {
        text.text = $"得分：{item.Score}";
        count.text = $"剩余次数：{item.Count}";
    }
}