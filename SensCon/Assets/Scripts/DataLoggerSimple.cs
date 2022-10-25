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
    StreamWriter swSphere;
    StreamWriter swFeedback;

    // Start is called before the first frame update
    public void Start()
    {

        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }

        string filepath;
        filepath = rootFolder + "ID" + participantId + "-simple-state.csv";

        while (File.Exists(filepath)) {
            participantId++;
            filepath = rootFolder + "ID" + participantId + "-simple-state.csv";
        }

        init();


    }

    private void init()
    {
        // this should happen only once during the runtime
        Debug.Log("Init Files");
        string filepath = rootFolder + "ID" + participantId + "-simple-state.csv";

        if (swState == null)
        {
            filepath = rootFolder + "ID" + participantId + "-simple-state.csv";
            swState = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swState.WriteLine("Time,Condition,PId,Status,Comment");
            swState.Flush();
        }

        if (swFeedback == null)
        {
            filepath = rootFolder + "ID" + participantId + "-feedback.csv";
            swFeedback = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swFeedback.WriteLine("Time,NbackColor,CurrentColors,Trash,IsCorrect,nBackNumber");
            swFeedback.Flush();
        }
        
        if (swSphere == null)
        {
            filepath = rootFolder + "ID" + participantId + "-sphere.csv";
            swSphere = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swSphere.WriteLine("Time,Type");
            swSphere.Flush();
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

    public void writeScore(double timestamp, int nbackColor, int currentColor, string trash, bool isCorrect , int nBackNumber)
    {
        if (swFeedback == null)
        {
            init();
        }
        swFeedback.WriteLine(timestamp + "," + nbackColor + "," + currentColor + "," + trash + "," + isCorrect + "," + nBackNumber);
        swFeedback.Flush();
    }

        public void writeSphereClick(double timestamp, string type)
    {
        if (swSphere == null)
        {
            init();
        }
        swSphere.WriteLine(timestamp + "," + type);
        swSphere.Flush();
    }



    void OnApplicationQuit()
    {
        double timestamp = UnixTime.GetTime();
        writeState(timestamp, "quit", "");
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

}

