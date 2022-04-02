using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;


public class StroopManager : MonoBehaviour
{

    enum STATES { started, end, wait, waitForStart};

    public GameObject startCanvas;
    public GameObject taskCanvas;
    public GameObject textTask;
    public GameObject textColor;

    public int maxTasks = 10;
    public int counter = 0;

    Button startButton;

    private STATES state = STATES.waitForStart;

    private List<StroopItem> taskList = new List<StroopItem>();

    // Start is called before the first frame update
    void Start()
    {
        taskCanvas.active = false;
        //startButton = GameObject.GetComponent<Button>();

        for (int i = 0; i < maxTasks; i++)
        {
            taskList.Add(StroopItem.Generate());
        }
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void ClickStart()
    {
        state = STATES.started;
        startCanvas.active = false;
        taskCanvas.active = true;
        next();
    }

    private void CheckColor(StroopItem.COLORS c) {
        next();
    }

    private void next()
    {
        if (counter + 1 != maxTasks)
        {
            update(taskList.ElementAt(counter));
            counter++;
        }
        else {
            taskCanvas.active = false;
        }
    
    }

    public void ClickPurple()
    {
        CheckColor(StroopItem.COLORS.purple);
    }

    public void ClickBlue()
    {
        CheckColor(StroopItem.COLORS.blue);
    }

    public void ClickGreen()
    {
        CheckColor(StroopItem.COLORS.green);
    }

    public void ClickOrange()
    {
        CheckColor(StroopItem.COLORS.orange);
    }

    public void ClickYellow()
    {
        CheckColor(StroopItem.COLORS.yellow);
    }

    public void ClickRed()
    {
        CheckColor(StroopItem.COLORS.red);
    }

    private void update(StroopItem i) {

        Text t1 = textTask.GetComponent(typeof(Text)) as Text;
        if (i.task == StroopItem.TASKS.word) {
            t1.text = "Word";
        } else {
            t1.text = "Color";
        }


        Text t2 = textColor.GetComponent(typeof(Text)) as Text;
        
        switch (i.colorString)
        {
            case StroopItem.COLORS.purple:
                t2.text = "Purple";
                break;
            case StroopItem.COLORS.blue:
                t2.text = "Blue";
                break;
            case StroopItem.COLORS.green:
                t2.text = "Green";
                break;
            case StroopItem.COLORS.orange:
                t2.text = "Orange";
                break;
            case StroopItem.COLORS.yellow:
                t2.text = "Yellow";
                break;
            case StroopItem.COLORS.red:
                t2.text = "Red";
                break;
            default:
                // code block
                t2.text = "ERROR";
                break;
        }

        switch (i.color)
        {
            case StroopItem.COLORS.purple:
                t2.color = new Vector4(0.7176471f, 0.2352941f, 0.5058824f, 1.0f);
                break;
            case StroopItem.COLORS.blue:
                t2.color = UnityEngine.Color.blue;
                break;
            case StroopItem.COLORS.green:
                t2.color = UnityEngine.Color.green;
                break;
            case StroopItem.COLORS.orange:
                t2.color = new Vector4(1f, 0.4941176f, 0.2f, 1.0f);
                break;
            case StroopItem.COLORS.yellow:
                t2.color = UnityEngine.Color.yellow;
                break;
            case StroopItem.COLORS.red:
                t2.color = UnityEngine.Color.red;
                break;
            default:
                // code block
                t2.text = "ERROR";
                break;
        }
    }

}
