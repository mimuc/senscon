using System;
using UnityEngine;

public class StroopItem
{

    public enum TASKS { color, word };
    public enum COLORS { purple, blue, green, orange, yellow, red };

    public TASKS task { get; set; }
    public COLORS color { get; set; }
    public COLORS colorString { get; set; }

    public StroopItem(TASKS task, COLORS color, COLORS colorString)
    {
        this.task = task;
        this.color = color;
        this.colorString = colorString;
    }

    public static StroopItem Generate() {

        COLORS randomColor = (COLORS)UnityEngine.Random.Range(0, Enum.GetValues(typeof(COLORS)).Length);
        COLORS randomColorString = (COLORS)UnityEngine.Random.Range(0, Enum.GetValues(typeof(COLORS)).Length);
        TASKS randomTask = (TASKS) UnityEngine.Random.Range(0, Enum.GetValues(typeof(TASKS)).Length);

        return new StroopItem(randomTask, randomColor, randomColorString);

    }
}
