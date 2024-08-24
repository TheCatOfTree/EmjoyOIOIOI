using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Directory = UnityEngine.Windows.Directory;


public class Pro2cs : EditorWindow
{
    private const string path = "Assets";
    
    private static string folderPath;
    [Tooltip("保存C#的位置")]
    private static string Savepath;
    
    private static string ProtocPath;

    private static string Batpath;
    private static string Result;
    private static int FileCount = 0;

    Pro2cs()
    {
        this.titleContent = new GUIContent("Protobuf to C#");
    }

    [MenuItem("Window/Pro2cs")]
    static void showWindow()
    {
        EditorWindow.GetWindow(typeof(Pro2cs));
        Savepath = @$"{Application.dataPath}\Scenes\7.2D_UI\Scripts\Protobuf\CS\";
        ProtocPath = $@"{Application.dataPath}\Scenes\7.2D_UI\Scripts\Protobuf\Proto\";
        Batpath = $@"{Application.dataPath}\Scenes\7.2D_UI\Scripts\Protobuf\_GenAllC#.bat";
    }

    void Init()
    {
        Result = " ";
        FileCount = 0;
        
        
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Protobuf to C#");
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 12;  
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUILayout.Label($"Save Path :{Savepath} ");
        GUILayout.Space(5);
        GUILayout.Label($"ProtocPath :{ProtocPath} ");
        GUILayout.Space(5);
        GUILayout.Label("Result: " + Result);
        GUILayout.Space(5);
        GUILayout.Label("Switch File Count : " + FileCount);
        if (GUILayout.Button("确定"))
        {
            Init();
            PtoFile();
            PtoC();
        }
    }

    static void PtoC()
    {
        try
        {
            Process pro = new Process();
            FileInfo file = new FileInfo(Batpath);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = Batpath;
            pro.StartInfo.CreateNoWindow = false;
            pro.Start();
            pro.WaitForExit();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
      

    }

    static void PtoFile()
    {
        folderPath = Application.dataPath;
        if (!Directory.Exists(path)) return;

        var directoryInfo = new DirectoryInfo(folderPath);
        var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

        foreach (var VARIABLE in fileInfos)
        {
            if (VARIABLE.Name.EndsWith(".meta")) continue;
            try
            {
                if (VARIABLE.Name.EndsWith(".proto"))
                {
                    VARIABLE.MoveTo(ProtocPath + VARIABLE.Name);

                    Debug.Log(VARIABLE.Name);


                    Result = "Finish!";
                    FileCount++;
                }
                else if (FileCount == 0)
                {
                    Result = "No Found '.proto' File !";
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}