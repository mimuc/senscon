using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;

public class DataLogger : MonoBehaviour
{
    public DataLoggerSimple loggerSimple;

    StreamWriter swVisitor;
    StreamWriter swSphere;
    StreamWriter swFeedback;
    StreamWriter swState;
    StreamWriter swFlow;
    StreamWriter swVisitorCount;
    StreamWriter swAdaption;




    // Start is called before the first frame update
    public void Start()
    {
        
        if (!Directory.Exists(loggerSimple.rootFolder))
        {
            Directory.CreateDirectory(loggerSimple.rootFolder);
        }

        string filepath;
        filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-visitor.csv";

        if (File.Exists(filepath)) {
            Debug.LogError("Participant log files already exists ID " + loggerSimple.participantId);
#if UNITY_EDITOR
            //UnityEditor.EditorApplication.isPlaying = false;
#else
               Application.Quit();
#endif

        }

        init();

    }

    private void init() {
        // this should happen only once during the runtime
        Debug.Log("Init Files");
        string filepath;


        if (swVisitor == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-visitor.csv";
            swVisitor = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swVisitor.WriteLine("Time,Name,HasTicket,Type");
            swVisitor.Flush();
        }

        if (swSphere == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-sphere.csv";
            swSphere = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            //swSphere.WriteLine("Time,Type,Feedback");
            swSphere.Flush();
        }

        if (swFeedback == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-feedback.csv";
            swFeedback = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            //swFeedback.WriteLine("Time,NbackColor,CurrentColors,Trash,IsCorrect");
            swFeedback.Flush();
        }

        if (swState == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-state.csv";
            swState = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            //swState.WriteLine("Time,State,BlockNumber,AdaptationStatus");
            swState.Flush();
        }

        if (swFlow == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-flow.csv";
            swFlow = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            //swFlow.WriteLine("Time,Name,HasTicket,Shirtcolor,Hair");
            swFlow.Flush();
        }

      
        if (swVisitorCount == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-visitorCount.csv";
            swVisitorCount = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swVisitorCount.WriteLine("Time,Count,CountActual");
            swVisitorCount.Flush();
        }


        if (swAdaption == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-adaptation.csv";
            swAdaption = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            //swAdaption.WriteLine("Time,Direction,CurrentCount,SlopeBaseline,SlopeEDA");
            swAdaption.Flush();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void writeVisitorClick(double timestamp, string name, bool hasTicket, string type)
    {
        if (swVisitor == null) {
            init();
        }
        swVisitor.WriteLine(timestamp + "," + name + "," + hasTicket + "," + type);
        swVisitor.Flush();
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

    public void writeScore(double timestamp, int nbackColor, int currentColor, string trash, bool isCorrect)
    {
        if (swFeedback == null)
        {
            init();
        }
        swFeedback.WriteLine(timestamp + "," + nbackColor + "," + currentColor + "," + trash + "," + isCorrect);
        swFeedback.Flush();
    }


    public void writeState(double timestamp, string state, int blockNumber, int adaptationStatus)
    {
        if (swState == null)
        {
            init();
        }
        swState.WriteLine(timestamp + "," + state + "," + blockNumber + "," + adaptationStatus);
        swState.Flush();
    }

    public void writeState(double timestamp, string state, int blockNumber, double adaptationStatus)
    {
        if (swState == null)
        {
            init();
        }
        swState.WriteLine(timestamp + "," + state + "," + blockNumber + "," + adaptationStatus);
        swState.Flush();
    }


    public void writeFlow(double timestamp, string name, bool hasticket, string shirtcolor, string hair)
    {
        if (swFlow == null)
        {
            init();
        }
        swFlow.WriteLine(timestamp + "," + name + "," + hasticket + "," + shirtcolor + "," + hair);
        swFlow.Flush();
    }

    public void writeVisitorCount(double timestamp, int count, int countActual)
    {
        if (swVisitorCount == null)
        {
            init();
        }
        swVisitorCount.WriteLine(timestamp + "," + count + "," + countActual);
        swVisitorCount.Flush();
    }


}
