using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayer : MonoBehaviour {

    public int stickChild;

    public string Name { get; set; }

    public float FrontR { get; set; }
    public float FrontG { get; set; }
    public float FrontB { get; set; }

    public float MidR { get; set; }
    public float MidG { get; set; }
    public float MidB { get; set; }

    public float BackR { get; set; }
    public float BackG { get; set; }
    public float BackB { get; set; }

    public Menus Menus;

    public void Start()
    {
        FrontR = 0.0f;
        FrontG = 0.0f;
        FrontB = 1.0f;

        MidR = 0.0f;
        MidG = 0.0f;
        MidB = 0.0f;

        BackR = 1.0f;
        BackG = 0.0f;
        BackB = 0.0f;
    }

    public void Update()
    {
        UpdateObjectColor();
    }

    public void UpdateObjectColor()
    {
        Transform stick = gameObject.transform.GetChild(stickChild);
        float[] front = new float[] {FrontR, FrontG, FrontB };
        float[] mid = new float[] {MidR, MidG, MidB };
        float[] back = new float[] {BackR, BackG, BackB };
        var list = new List<float[]> { front, mid, back };

        for (int x = 0; x < 3; x++)
        {
            SpriteRenderer symbol = stick.GetChild(x+2).GetComponent<SpriteRenderer>();
            symbol.color = new Color(list[x][0], list[x][1], list[x][2]);
        }
    }

    public void SavePlayer()
    {
        string path = Application.dataPath;
        path = path.Substring(path.Length);
        //Debug.Log(path);
        string currentText = System.IO.File.ReadAllText(path + "Players.txt");
        string[] lines = currentText.Split('\n');
        string[] allLines = new string[lines.Length];

        int index;
        for (index = 0; index < lines.Length-1; index++)
        {
            allLines[index] = lines[index];
        }
        string inputLine = lines.Length.ToString()+")\t"+Name+"\t"+FrontR+","+FrontG+","+FrontB
                                +"\t"+BackR+","+BackG+","+BackB+"\t"+MidR+","+MidG+","+MidB+"\t"+0+","+0; //0 is for wins/loss
        allLines[index] = inputLine;
        System.IO.File.WriteAllLines(@""+path+"Players.txt", allLines);

        Menus.ChangePage(4);
    }
}
