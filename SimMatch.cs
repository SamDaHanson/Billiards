using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SimMatch : MonoBehaviour {

    public ChooseCompetitors cc;

    private GameObject stick1;
    private GameObject stick2;

    private Transform player1;
    private Transform player2;

    public bool readyToGo = false;
    private bool startButton = false;

    // Use this for initialization
    public void Start () 
    {
        readyToGo = true;
		GameObject stick1 = new GameObject();
        stick1 = Instantiate(cc.Player1.Stick, gameObject.transform.GetChild(3).transform);
        GameObject stick2 = new GameObject();
        stick2 = Instantiate(cc.Player2.Stick, gameObject.transform.GetChild(4).transform);
        
        stick1.transform.localPosition = new Vector3(-15, 130, 0);
        stick2.transform.localPosition = new Vector3(-15, -130, 0);

        player1 = gameObject.transform.GetChild(3);
        player2 = gameObject.transform.GetChild(4);

        player1.GetChild(0).GetComponent<TextMeshPro>().SetText(cc.Player1.Name);
        player2.GetChild(0).GetComponent<TextMeshPro>().SetText(cc.Player2.Name);
        player1.GetChild(2).gameObject.SetActive(true);
        player2.GetChild(2).gameObject.SetActive(true);
        startButton = true;
    }

    public void Update()
    {
        Transform input1 = player1.GetChild(1).GetChild(0).GetChild(1).GetChild(2);
        int player1Score = -1;
        bool isNum = false;
        if (input1.GetComponent<Text>().text != "")
            isNum = Int32.TryParse(input1.GetComponent<Text>().text, out player1Score);

        Transform input2 = player2.GetChild(1).GetChild(0).GetChild(1).GetChild(2);
        int player2Score = -1;
        bool isNum2 = false;
        if (input2.GetComponent<Text>().text != "")
            isNum2 = Int32.TryParse(input2.GetComponent<Text>().text, out player2Score);
        Debug.Log(player1Score+" "+player2Score);
        Debug.Log(isNum+" "+isNum2);
        if (!isNum)
            player1Score = -1;
        if (!isNum2)
            player2Score = -1;
        if (player1Score != -1 && player2Score != -1 && startButton)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    private void Setup()
    {
        readyToGo = true;
        GameObject stick1 = new GameObject();
        stick1 = Instantiate(cc.Player1.Stick, gameObject.transform.GetChild(3).transform);
        GameObject stick2 = new GameObject();
        stick2 = Instantiate(cc.Player2.Stick, gameObject.transform.GetChild(4).transform);

        stick1.transform.localPosition = new Vector3(-15, 130, 0);
        stick2.transform.localPosition = new Vector3(-15, -130, 0);

        player1 = gameObject.transform.GetChild(3);
        player2 = gameObject.transform.GetChild(4);

        player1.GetChild(0).GetComponent<TextMeshPro>().SetText(cc.Player1.Name);
        player2.GetChild(0).GetComponent<TextMeshPro>().SetText(cc.Player2.Name);
        player1.GetChild(2).gameObject.SetActive(true);
        player2.GetChild(2).gameObject.SetActive(true);
        startButton = true;
    }

    public void PlayGame()
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        startButton = false;
        Transform input1 =  player1.GetChild(1).GetChild(0).GetChild(1).GetChild(2);
        int player1Score = -1;
        Int32.TryParse(input1.GetComponent<Text>().text, out player1Score);
        
        Transform input2 = player2.GetChild(1).GetChild(0).GetChild(1).GetChild(2);
        int player2Score = -1;
        Int32.TryParse(input2.GetComponent<Text>().text, out player2Score);
        if (player1Score != -1 && player2Score != -1)
        {
            string winnerName = null;
            string loserName = null;
            if (player1Score > player2Score)
            {
                //P1 wins
                winnerName = cc.Player1.Name;
                loserName = cc.Player2.Name;
                gameObject.transform.GetChild(5).gameObject.SetActive(true);
            } else if (player2Score > player1Score)
            {
                //P2 wins
                winnerName = cc.Player2.Name;
                loserName = cc.Player1.Name;
                gameObject.transform.GetChild(6).gameObject.SetActive(true);
            } else
            {
                //Tie
                winnerName = null;
                loserName = null;
                gameObject.transform.GetChild(7).gameObject.SetActive(true);
            }
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            gameObject.transform.GetChild(8).gameObject.SetActive(true);

            //string path = Application.dataPath;
            string path = Application.dataPath;
            path = path.Substring(path.Length);
            //Debug.Log(path);
            string currentText = System.IO.File.ReadAllText(path + "Players.txt");
            string[] players = currentText.Split('\n');

            if (winnerName != null & loserName != null)
            {
                string[] allTogether = new string[players.Length-1];
                for (int x = 0; x < players.Length-1; x++)
                {
                    String[] parts = players[x].Split('\t');
                    Debug.Log(parts[0]+"\t"+ parts[1] + "\t" + parts[2]+"\t"+ parts[3]+ "\t" + parts[4]);//+ "\t" + parts[5]);
                    string name = parts[1];
                    string[] winLoss = parts[5].Split(',');
                    int wins = Int32.Parse(winLoss[0]);
                    int losses = Int32.Parse(winLoss[1]);

                    if (name == winnerName)
                    {
                        //Add win
                        wins++;
                        parts[5] = wins+","+losses;
                        Debug.Log("Winner: " + name + " Wins: " + wins);
                    }
                    else if (name == loserName)
                    {
                        //Add loss
                        losses++;
                        parts[5] = wins+","+losses;
                        Debug.Log("Loser: "+name+" Losses: "+losses);
                    }
                    allTogether[x] = parts[0]+"\t"+parts[1]+"\t"+parts[2]+"\t"+parts[3]+"\t"+parts[4]+"\t"+parts[5];
                }
                System.IO.File.WriteAllLines(@"" + path + "Players.txt", allTogether);
            } else
            {
                //Tie

            }
        }
    }

    public void ResetMenu()
    {
        if (readyToGo)
        {
            Debug.Log("Resetting the Sim");
            Debug.Log(player1.GetChild(1).GetChild(0).GetChild(1).GetChild(2).GetComponent<Text>().text);
            player1.GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>().text = "";
            player2.GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>().text = "";
            player1.GetChild(0).GetComponent<TextMeshPro>().SetText("Player2");
            player2.GetChild(0).GetComponent<TextMeshPro>().SetText("Player1");
            GameObject.Destroy(player1.GetChild(2).gameObject);
            GameObject.Destroy(player2.GetChild(2).gameObject);
            gameObject.transform.GetChild(5).gameObject.SetActive(false);
            gameObject.transform.GetChild(6).gameObject.SetActive(false);
            gameObject.transform.GetChild(7).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            gameObject.transform.GetChild(8).gameObject.SetActive(false);
            readyToGo = false;
            Setup();
        }
    }
}
