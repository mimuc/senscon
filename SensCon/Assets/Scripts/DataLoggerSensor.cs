using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;

public class DataLoggerSensor : MonoBehaviour
{
    public DataLoggerSimple loggerSimple;

    StreamWriter swEda, swEeg, swPpg, swM_Eda, swM_Ppg;

    StringBuilder stringbuilderEda = new StringBuilder();
    StringBuilder stringbuilderEeg = new StringBuilder();
    StringBuilder stringbuilderPpg = new StringBuilder();
    StringBuilder stringbuilderM_Eda = new StringBuilder();
    StringBuilder stringbuilderM_Ppg = new StringBuilder();

    private int countedEda = 0, countedEeg = 0, countedPpg = 0, countedM_Eda = 0, countedM_Ppg = 0;


    // Start is called before the first frame update
    public void Start()
    {

        if (!Directory.Exists(loggerSimple.rootFolder))
        {
            Directory.CreateDirectory(loggerSimple.rootFolder);
        }

        string filepath;
        filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-EDA.csv";

        if (File.Exists(filepath))
        {
            Debug.LogError("Participant log files already exists ID " + loggerSimple.participantId);
#if UNITY_EDITOR
            //UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        }

        init();

    }

    public void init()
    {
        // this should happen only once during the runtime
        Debug.Log("Init Files");
        string filepath;

        if (swEda == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-EDA.csv";
            swEda = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swEda.WriteLine("Time,TimeLsl,Value");
            swEda.Flush();
        }

        if (swM_Eda == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-MEDA.csv";
            swM_Eda = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swM_Eda.WriteLine("Time,TimeLsl,EDA,Heartrate");
            swM_Eda.Flush();
        }

        if (swPpg == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-PPG.csv";
            swPpg = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swPpg.WriteLine("Time,TimeLsl,Status,Oxygen,Confidence,Heartrate");
            swPpg.Flush();
        }

        if (swM_Ppg == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-MPPG.csv";
            swM_Ppg = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swM_Ppg.WriteLine("Time,TimeLsl,Heartrate");
            swM_Ppg.Flush();
        }

        if (swEeg == null)
        {
            filepath = loggerSimple.rootFolder + "ID" + loggerSimple.participantId + "-EEG.csv";
            //swEeg = File.CreateText(filepath);
            swEeg = (!File.Exists(filepath)) ? File.CreateText(filepath) : File.AppendText(filepath);
            swEeg.WriteLine("Time,TimeLsl,Value0,Value1,Value2,Value3,Value4,Value5,Value6,Value7");
            swEeg.Flush();
        }
    }
    internal void write(string name, SignalSample1D s)
    {
        if (swEda == null || swEeg == null || swPpg == null || swM_Eda == null || swM_Ppg == null)
        {
            init();
        }
        if (name.ToLower() == "eda")
        {
            stringbuilderEda.AppendFormat("{0},{1},{2}{3}", s.time, s.timeLsl, s.values[0], Environment.NewLine);

            countedEda++;
            if (countedEda % 1000 == 0)
            {
                swEda.WriteLine(stringbuilderEda);
                stringbuilderEda.Clear();
                swEda.Flush();
            }
        }
        else if (name.ToLower() == "m_eda")
        {

            if (s.values.Length == 2)
            {
                stringbuilderM_Eda.AppendFormat("{0},{1},{2},{3}{4}", s.time, s.timeLsl, s.values[0], s.values[1], Environment.NewLine);
            }
            else
            {
                throw new NotImplementedException("Your electrode count is not 2 please ajust the script");
            }

            countedM_Eda++;
            if (countedM_Eda % 1000 == 0)
            {
                swM_Eda.WriteLine(stringbuilderM_Eda);
                stringbuilderM_Eda.Clear();
                swM_Eda.Flush();
            }
        }
        else if (name.ToLower() == "eeg")
        {
            if (s.values.Length == 8)
            {
                stringbuilderEeg.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}{10}", s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], s.values[4], s.values[5], s.values[6], s.values[7], Environment.NewLine);
            }
            else
            {
                throw new NotImplementedException("Your electrode count is not 8 please ajust the script");
            }

            countedEeg++;
            if (countedEeg % 1000 == 0)
            {
                swEeg.WriteLine(stringbuilderEeg);
                stringbuilderEeg.Clear();
                swEeg.Flush();
            }
        }
        else if (name.ToLower() == "ppg")
        {
            if (s.values.Length == 1)
            {
                // stringbuilderPpg.AppendFormat("{0},{1},{2},{3},{4},{5}{6}", s.time, s.timeLsl, s.values[0], s.values[1], s.values[2], s.values[3], Environment.NewLine);
                stringbuilderPpg.AppendFormat("{0},{1},{2}{3}", s.time, s.timeLsl, s.values[0], Environment.NewLine);
            }
            else
            {
                // throw new NotImplementedException("Your electrode count is not 4 please ajust the script");
                throw new NotImplementedException("Your electrode count is not 1 please ajust the script");
            }
            countedPpg++;
            if (countedPpg % 1000 == 0)
            {
                swPpg.WriteLine(stringbuilderPpg);
                stringbuilderPpg.Clear();
                swPpg.Flush();
            }
        }
        else if (name.ToLower() == "m_ppg")
        {
            if (s.values.Length == 2)
            {
                stringbuilderM_Ppg.AppendFormat("{0},{1},{2}{3}", s.time, s.timeLsl, s.values[0], Environment.NewLine);
            }
            else
            {
                throw new NotImplementedException("Your electrode count is not 1 please ajust the script");
            }
            countedPpg++;
            if (countedM_Ppg % 1000 == 0)
            {
                swM_Ppg.WriteLine(stringbuilderPpg);
                stringbuilderM_Ppg.Clear();
                swM_Ppg.Flush();
            }
        }
        else
        {
            Debug.LogWarning("Logger Data Dropped");
        }
    }

    void OnDestroy()
    {
        if (swEda != null)
        {
            swEda.Flush();
        }
        if (swM_Eda != null)
        {
            swM_Eda.Flush();
        }
        if (swEeg != null)
        {
            swEeg.Flush();
        }
        if (swPpg != null)
        {
            swPpg.Flush();
        }
        if (swM_Ppg != null)
        {
            swM_Ppg.Flush();
        }
    }
}
