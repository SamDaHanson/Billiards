using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChooseCompetitors : MonoBehaviour {

    public GameObject competitorCase;
    public GameObject competitorCase2;
    GameObject newCompetitor;

    public GameObject stickPrefab;
    GameObject newStick;

    public bool trigger;
    public bool imReady = false;

    private Transform playerList1;
    private Transform playerList2;
    private Transform content1;
    private Transform content2;

    public Player Player1;
    public Player Player2;

    public GameObject player1;
    public GameObject player2;

    private bool ready1 = false;
    private bool ready2 = false;

    public bool isReady = false;

    public class Player
    {
        public string Name { get; set; }
        public GameObject Stick { get; set; }

        public Player(string name, GameObject stick)
        {
            Stick = stick;
            Name = name;
        }

        public Player()
        {
            Stick = null;
            Name = "Empty";
        }
    }

    public void Start() 
    {
        isReady = true;

        playerList1 = gameObject.transform.GetChild(2);
        playerList2 = gameObject.transform.GetChild(4);
        content1 = playerList1.GetChild(0).GetChild(0);
        content2 = playerList2.GetChild(0).GetChild(0);
        trigger = false;

        Debug.Log("Start");

        Player1 = new Player();
        Player2 = new Player();
        //imReady = true;
        CreatePlayerLists();
    }

    public void Update()
    {
            if (trigger)
            {
                CreatePlayerLists();
                trigger = false;
            }
    }

    public void DestroyPlayers()
    {
        Debug.Log("hulk smash");

        content1.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 0.0f);
        for (int x = 0; x < content1.childCount; x++)
        {
            //Debug.Log(content1.GetChild(x).gameObject);
            GameObject.Destroy(content1.GetChild(x).gameObject);
        }

        for (int x = 0; x < content2.childCount; x++)
        {
            GameObject.Destroy(content2.GetChild(x).gameObject);
        }
    }

    private void CreatePlayerLists()
    {
        content1.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 0.0f);
        content2.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 0.0f);
        string path = Application.dataPath;
        path = path.Substring(path.Length);
        //Debug.Log(path);
        string currentText = System.IO.File.ReadAllText(path + "Players.txt");
        string[] lines = currentText.Split('\n');

        for (int x = 0; x < lines.Length - 1; x++)
        {
            string[] parts = lines[x].Split('\t');
            string name = parts[1];
            string details = parts[2]+"\t"+parts[3]+"\t"+parts[4];

            newCompetitor = Instantiate(competitorCase, content1) as GameObject;
            newCompetitor = Instantiate(competitorCase2, content2) as GameObject;

            GameObject chosen = content1.GetChild(x).gameObject;
            chosen.GetComponent<Button>().onClick.AddListener(delegate { SelectPlayer(chosen, content1, 1);});
            content1.GetChild(x).GetChild(0).GetComponent<Text>().text = name;
            content1.GetChild(x).GetChild(1).GetComponent<Text>().text = details;
            content1.GetComponent<RectTransform>().sizeDelta = new Vector2(180,content1.GetComponent<RectTransform>().sizeDelta.y+30.0f);
            
            GameObject chosen2 = content2.GetChild(x).gameObject;
            chosen2.GetComponent<Button>().onClick.AddListener(delegate { SelectPlayer(chosen2, content2, 2);});
            content2.GetChild(x).GetChild(0).GetComponent<Text>().text = name;
            content2.GetChild(x).GetChild(1).GetComponent<Text>().text = details;
            content2.GetComponent<RectTransform>().sizeDelta = new Vector2(180,content2.GetComponent<RectTransform>().sizeDelta.y+30.0f);
        }
        content2.transform.rotation = new Quaternion(0,180,0,0);
    }

    private GameObject getStick(string name, string details, GameObject player)
    {
        newStick = Instantiate(stickPrefab, player.transform);
        string[] parts = details.Split('\t');
        string[] front = parts[0].Split(',');
        string[] back = parts[1].Split(',');
        string[] background = parts[2].Split(',');

        var list = new List<string[]> { front, background, back };

        for (int x = 0; x < 3; x++)
        {
            SpriteRenderer symbol = newStick.transform.GetChild(x+2).GetComponent<SpriteRenderer>();
            Color color =  new Color(float.Parse(list[x][0]), float.Parse(list[x][1]), float.Parse(list[x][2]));
            symbol.color = color;
        }
        newStick.AddComponent<Text>().text = name;
        return newStick;
    }

    void SelectPlayer(GameObject chosen, Transform parent, int playerNumber)
    {
        for (int x = 0; x < parent.childCount; x++)
        {
            parent.GetChild(x).GetComponent<Image>().color = new Color(1,1,1);
        }

        GameObject player = player1;
        Player Player = Player1;
        if (playerNumber == 1)
        {
            player = player1;
            Player = Player1;
        } else if (playerNumber == 2)
        {
            player = player2;
            Player = Player2;
        } else
        {
            Debug.Log("Error, invalid player number in SelectPlayer()");
        }

        for (int x = 0; x < player.transform.childCount; x++)
        {
            Destroy(player.transform.GetChild(x).gameObject);
        }

        chosen.gameObject.GetComponent<Image>().color = new Color(1,1,0);
        string name = chosen.gameObject.transform.GetChild(0).GetComponent<Text>().text;
        string details = chosen.gameObject.transform.GetChild(1).GetComponent<Text>().text;
        GameObject stick = getStick(name,details,player);
        Player.Name = name;
        Player.Stick = stick;
        Destroy(Player.Stick.GetComponent<Text>());
        Player.Stick.SetActive(false);
        Player.Stick.transform.localScale = new Vector3(20,20,1);
        Player.Stick.transform.localPosition = new Vector3(-15,5,0);
        Player.Stick.transform.rotation = new Quaternion(0,0,30,0);
        if (playerNumber == 1)
            Player.Stick.transform.Rotate(new Vector3(0,0,180));
        Player.Stick.transform.Rotate(new Vector3(0,0,20));
        ConfirmPlayer(playerNumber);
    }

    public void ConfirmPlayer(int playerNumber)
    {
        if (playerNumber == 1)
        {
            gameObject.transform.GetChild(2).gameObject.SetActive(false);
            gameObject.transform.GetChild(3).gameObject.SetActive(false);
            Player1.Stick.SetActive(true);
            Transform texty = gameObject.transform.GetChild(12).GetChild(0).GetChild(0);
            texty.GetComponent<TextMeshPro>().SetText(Player1.Name);
            ready1 = true;
        } else if (playerNumber == 2)
        {
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            gameObject.transform.GetChild(5).gameObject.SetActive(false);
            Player2.Stick.SetActive(true);
            Transform texty = gameObject.transform.GetChild(13).GetChild(0).GetChild(0);
            texty.GetComponent<TextMeshPro>().SetText(Player2.Name);
            ready2 = true;
        } else
        {
            Debug.Log("Error, incorrect error number for ConfirmPlayer()");
        }

        if (ready1 && ready2)
        {
            gameObject.transform.GetChild(6).gameObject.SetActive(true);
            gameObject.transform.GetChild(8).gameObject.SetActive(false);
        }
    }

    public void ResetMenu()
    {
        if (ready1)
            Player1.Stick.SetActive(false);
        if (ready2)
            Player2.Stick.SetActive(false);
        Transform texty = gameObject.transform.GetChild(12).GetChild(0).GetChild(0);
        texty.GetComponent<TextMeshPro>().SetText("");
        texty = gameObject.transform.GetChild(13).GetChild(0).GetChild(0);
        texty.GetComponent<TextMeshPro>().SetText("");
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
        gameObject.transform.GetChild(3).gameObject.SetActive(true);
        gameObject.transform.GetChild(4).gameObject.SetActive(true);
        gameObject.transform.GetChild(5).gameObject.SetActive(true);
        gameObject.transform.GetChild(6).gameObject.SetActive(false);
        gameObject.transform.GetChild(8).gameObject.SetActive(true);
    }
}
