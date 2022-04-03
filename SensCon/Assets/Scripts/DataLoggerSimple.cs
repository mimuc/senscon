using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;

public class DataLoggerSimple : MonoBehaviour
{
    public int participantId = 0;
    public string condition;

    public string rootFolder = "./LogData/";


    StreamWriter swState;

    // Start is called before the first frame update
    public void Start()
    {

        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }

        string filepath;
        filepath = rootFolder + "ID" + participantId + "-simple-state.csv";

        if (File.Exists(filepath))
        {
            Debug.LogError("Participant log files already exists ID " + participantId);
#if UNITY_EDITOR
            //UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        }

        init();

    }

    private void init()
    {
        // this should happen only once during the runtime
        Debug.Log("Init Files");
        string filepath;
        if (swState == null)
        {
            filepath = rootFolder + "ID" + participantId + "-simple-state.csv";
            swState = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swState.WriteLine("Time,Condition,PId,Status,Comment");
            swState.Flush();
        }
    }


    // Update is called once per frame
    void Update()
    {

        double timestamp = UnixTime.GetTime();

        if (Input.GetKeyDown("s"))
        {
            Debug.Log("Start Logging");
            writeState(timestamp, "start", "");
        }

        if (Input.GetKeyDown("e"))
        {
            Debug.Log("End Logging");
            writeState(timestamp, "end", "");
        }
    }

    public void writeState(double timestamp, string status, string comment)
    {
        if (swState == null)
        {
            init();
        }
        swState.WriteLine(timestamp + "," + condition + "," + participantId + "," + status + "," + comment);
        swState.Flush();
    }



    void OnApplicationQuit()
    {
        double timestamp = UnixTime.GetTime();
        writeState(timestamp, "quit", "");
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

}

