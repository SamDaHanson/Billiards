using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LeaderBoard : MonoBehaviour {

    public GameObject panelPrefab;
    private GameObject playerPanel;

    public void Start()
    {
        CreateBoard();
    }

    public void ResetMenu()
    {
        Transform content = gameObject.transform.GetChild(3).GetChild(0).GetChild(0);
        for (int x = 0; x < content.transform.childCount; x++)
        {
            GameObject chosen = content.GetChild(x).gameObject;
            Destroy(chosen);
        }
        CreateBoard();
    }

    public void CreateBoard()
    {
        //string path = "Data";
        string path = Application.dataPath;
        path = path.Substring(path.Length);
        //Debug.Log(path);
        string currentText = System.IO.File.ReadAllText(path + "Players.txt");
        string[] players = currentText.Split('\n');
        Transform content = gameObject.transform.GetChild(3).GetChild(0).GetChild(0);

        for (int x = 0; x < players.Length - 2; x++)
        {
            string[] parts = players[x].Split('\t');
            //string details = parts[2] + "\t" + parts[3] + "\t" + parts[4];
            string[] winLoss = parts[5].Split(',');
            //Debug.Log("Name: " + name + " winLoss: " + winLoss[0] + "-" + winLoss[1]);
            string[] parts2 = players[x+1].Split('\t');
            string[] winLoss2 = parts2[5].Split(',');
            int wins = Int32.Parse(winLoss[0]);
            int wins2 = Int32.Parse(winLoss2[0]);
            //Debug.Log("Wins: "+wins+" Wins2:"+wins2);

            if (wins2 > wins)
            {
                //Debug.Log("P"+x+") "+ players[x]);
                //Debug.Log("P2)"+ players[x+1]);
                string placeHolder = players[x];
                players[x] = players[x+1];
                players[x+1] = placeHolder;
                x = x - 2;
                if (x < -1)
                    x = -1;
            }
        }

        for (int x = 0; x < players.Length - 1; x++)
        {
            string[] parts = players[x].Split('\t');
            //string details = parts[2] + "\t" + parts[3] + "\t" + parts[4];
            string[] winLoss = parts[5].Split(',');
            //Debug.Log("Name: "+name+" winLoss: "+ winLoss[0]+"-"+winLoss[1]);
            playerPanel = Instantiate(panelPrefab, content) as GameObject;
            GameObject chosen = content.GetChild(x).gameObject;
            TextMeshProUGUI setName = chosen.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI setScore = chosen.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            setName.SetText(parts[1]);
            setScore.SetText(winLoss[0]);
        }
    }
}
