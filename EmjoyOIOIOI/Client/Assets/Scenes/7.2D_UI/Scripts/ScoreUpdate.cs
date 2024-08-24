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
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update score
    public void UpdateScore(C2S_OperationMsg item)
    {
        text.text = item.Score.ToString();
    }
}
