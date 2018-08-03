using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using TMPro;

public class PlayGame : MonoBehaviour {

    public AIB aib;
    public GameObject mm;

    public Ball[] balls;
    private int numBalls = 9;
    private Transform ballsDir;

    public GameObject stickPrefab;
    private GameObject player1Stick;
    private GameObject player2Stick;

    private int currentPlayer = 1;  //Player 1 always starts
    private bool readyToShoot = false;
    public bool shooting = false;
    
    public bool playerShooting = false;  //For AI or player control

    public class Ball
    {
        public int Numb { get; set; }
        public GameObject Get { get; set; }

        public Ball(int numb, GameObject ball)
        {
            Numb = numb;
            Get = ball;
        }
    }

    public void Start()
    {
        balls = new Ball[numBalls+1];
        ballsDir = gameObject.transform.GetChild(0).GetChild(1);
        for (int x = 0; x < numBalls+1; x++)
        {
            balls[x] = new Ball(x, ballsDir.GetChild(x).gameObject);
        }
        //player1Stick = new GameObject();
        //player2Stick = new GameObject();
        RackBalls();
        SetupSticks();
        player1Stick.SetActive(true);       //Player 1 always starts
        mm.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Player1: "+player1Stick.name + "'s turn");
        //aib.RunBilliardAI();
    }

    public void Update()
    {
        if (playerShooting)
        {
            if (!shooting)
            {
                Vector2 pointer = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 cuePos = balls[0].Get.transform.localPosition;

                pointer.y = pointer.y - 0.75f;
                float angle = AngleBetween(pointer, cuePos);
                float distance = Vector2.Distance(pointer, cuePos);
                if (distance > 3.0f)
                    distance = 3.0f;
                Vector2 dVector = new Vector2(-distance, 0);
                player1Stick.transform.localPosition = dVector;
                player2Stick.transform.localPosition = dVector;
                gameObject.transform.GetChild(0).GetChild(2).localPosition = cuePos;
                gameObject.transform.GetChild(0).GetChild(2).eulerAngles = new Vector3(0, 0, angle);
                if (Input.GetMouseButton(0))
                {
                    if (readyToShoot)
                    {
                        StartCoroutine(Shoot(angle));
                        shooting = true;
                        readyToShoot = false;
                    }
                }
                else
                {
                    if (!readyToShoot)
                        readyToShoot = true;
                }
            }
        } else
        {
            if (aib.outputLayer.Length > 0)
            {
                if (!shooting)
                {
                    float pDist = aib.outputLayer[0] * 1.5f;   //If max aka 3, then pointer vector2 is (x,y)
                    float pAx = aib.outputLayer[1];
                    float pAy = aib.outputLayer[2];

                    Vector2 cuePos = balls[0].Get.transform.localPosition;
                    Vector2 pointer = new Vector2(cuePos.x+pAx, cuePos.y+pAy);

                    Vector2 dVector = new Vector2(-pDist, 0);
                    float angle = AngleBetween(pointer, cuePos);

                    player1Stick.transform.localPosition = dVector;
                    player2Stick.transform.localPosition = dVector;
                    gameObject.transform.GetChild(0).GetChild(2).localPosition = cuePos;
                    gameObject.transform.GetChild(0).GetChild(2).eulerAngles = new Vector3(0, 0, angle);
                }
            }
        }
    }

    public IEnumerator Shoot(float angle, bool manual = true, float distance = 0.0f)
    {
        shooting = true;
        if (manual)
        {
            distance = -player1Stick.transform.localPosition.x;
        } else
        {
            if (distance > 1.5f)
                distance = 1.5f;
        }

        float startingDistance = distance;
        while (distance > 0.05f)
        {
            //Debug.Log("Shooting Distance: "+distance);
            Vector3 moveVector = new Vector3(startingDistance*0.1f,0);
            player1Stick.transform.localPosition = player1Stick.transform.localPosition + moveVector;
            player2Stick.transform.localPosition = player2Stick.transform.localPosition + moveVector;
            distance = -player1Stick.transform.localPosition.x;
            yield return new WaitForSeconds(0.01f);
        }

        float x = Mathf.Cos(angle*2.0f*Mathf.PI/360.0f);
        float y = Mathf.Sin(angle*2.0f*Mathf.PI/360.0f);
        float scalar = startingDistance*5.0f;
        Vector2 force;
        if (x < 0.0f)
        {   
            if (y > 0.0f)
            {
                force = new Vector2(scalar * x, -scalar * y);
            } else
            {
                force = new Vector2(scalar * x, scalar * y);
            }
        }
        else
        {
            force = new Vector2(scalar * x, scalar * y);
        }
        balls[0].Get.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        player1Stick.SetActive(false);
        player2Stick.SetActive(false);
    }

    public IEnumerator AIShoot(float xA, float yA, float distance = 0.0f)
    {
        shooting = true;
        if (distance > 1.5f)
            distance = 1.5f;

        float startingDistance = distance;
        while (distance > 0.05f)
        {
            //Debug.Log("Shooting Distance: "+distance);
            Vector3 moveVector = new Vector3(startingDistance * 0.1f, 0);   //Gets the linear velocity from distance
            player1Stick.transform.localPosition = player1Stick.transform.localPosition + moveVector;
            player2Stick.transform.localPosition = player2Stick.transform.localPosition + moveVector;
            distance = -player1Stick.transform.localPosition.x;
            yield return new WaitForSeconds(0.01f);
        }

        float scalar = startingDistance * 5.0f;
        Vector2 force = new Vector2(scalar*xA,scalar*yA); 

        balls[0].Get.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        player1Stick.SetActive(false);
        player2Stick.SetActive(false);
    }

    public float AngleBetween(Vector2 start, Vector2 end)
    {
        float angle = 0.0f;
        if (start.x >= end.x)
        {
            if (start.y >= end.y)
            {
                angle = 360.0f * Mathf.Atan((start.y - end.y) / (start.x - end.x)) / (2 * Mathf.PI);
            }
            else if (start.y < end.y)
            {
                angle = 450.0f + (360.0f * Mathf.Atan((start.y - end.y) / (start.x - end.x)) / (2 * Mathf.PI) - 90.0f);
            }
        }
        else if (start.x < end.x)
        {
            angle = 180.0f + 360.0f * Mathf.Atan((start.y - end.y) / (start.x - end.x)) / (2 * Mathf.PI);
        }
        return angle;
    }

    public void ChangeTurn()
    {
        if (currentPlayer == 1)
        {
            player1Stick.SetActive(false);
            player2Stick.SetActive(true);
            mm.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Player2: " + player2Stick.name + "'s turn");
            currentPlayer = 2;
        } else if (currentPlayer == 2)
        {
            player1Stick.SetActive(true);
            player2Stick.SetActive(false);
            mm.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Player1: " + player1Stick.name + "'s turn");
            currentPlayer = 1;
        } else
        {
            Debug.Log("Error, invalid current player");
        }
    }

    private void SetupSticks()
    {
        string path = Application.dataPath;
        path = path.Substring(path.Length);
        string currentText = System.IO.File.ReadAllText(path + "Players.txt");
        string[] lines = currentText.Split('\n');

        int index;
        for (index = 0; index < lines.Length - 1; index++)
        {
            string[] parts = lines[index].Split('\t');
            //Debug.Log(parts[6]);
            if (int.Parse(parts[6]) == 1)
            {
                //Debug.Log("Player1: "+parts[1]);
                player1Stick = Instantiate(stickPrefab,gameObject.transform.GetChild(0).GetChild(2));
                player1Stick.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                player1Stick.transform.localPosition = Vector3.zero;

                for (int x = 2; x < 5; x++)
                {
                    string[] rgb = parts[x].Split(',');
                    float red = float.Parse(rgb[0]);
                    float green = float.Parse(rgb[1]);
                    float blue = float.Parse(rgb[2]);
                    switch (x)
                    {
                        case 2:
                            player1Stick.transform.GetChild(2).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                        case 3:
                            player1Stick.transform.GetChild(4).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                        case 4:
                            player1Stick.transform.GetChild(3).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                    }
                }
                player1Stick.name = parts[1];
                player1Stick.SetActive(false);
            } else if (int.Parse(parts[6]) == 2)
            {
                //Debug.Log("Player2: "+parts[1]);
                player2Stick = Instantiate(stickPrefab,gameObject.transform.GetChild(0).GetChild(2));
                player2Stick.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                player2Stick.transform.localPosition = Vector3.zero;

                for (int x = 2; x < 5; x++)
                {
                    string[] rgb = parts[x].Split(',');
                    float red = float.Parse(rgb[0]);
                    float green = float.Parse(rgb[1]);
                    float blue = float.Parse(rgb[2]);
                    switch (x)
                    {
                        case 2:
                            player2Stick.transform.GetChild(2).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                        case 3:
                            player2Stick.transform.GetChild(4).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                        case 4:
                            player2Stick.transform.GetChild(3).transform.GetComponent<SpriteRenderer>().color = new Color(red, green, blue);
                            break;
                    }
                }
                player2Stick.name = parts[1];
                player2Stick.SetActive(false);
            }
        }
    }

    public void RackBalls()
    {
        float forward = -1.0f;

        balls[0].Get.transform.localPosition = new Vector3(-2.5f*forward,0f);
        balls[0].Get.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        balls[1].Get.transform.localPosition = new Vector3(1.94f*forward,0);
        balls[1].Get.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        int starter = (int)Mathf.Floor(Random.Range(2, 7.999f));
        for (int x = 0; x < 6; x++)
        {
            if (starter > 7)
            {
                starter = 2;
            }
            switch (x)
            {
                case 0:
                    balls[starter].Get.transform.localPosition = new Vector3(2.22f*forward, 0.16f);
                    break;
                case 1:
                    balls[starter].Get.transform.localPosition = new Vector3(2.78f*forward, -0.16f);
                    break;
                case 2:
                    balls[starter].Get.transform.localPosition = new Vector3(2.5f*forward, 0.32f);
                    break;
                case 3:
                    balls[starter].Get.transform.localPosition = new Vector3(2.5f*forward, -0.32f);
                    break;
                case 4:
                    balls[starter].Get.transform.localPosition = new Vector3(2.78f*forward, 0.16f);
                    break;
                case 5:
                    balls[starter].Get.transform.localPosition = new Vector3(2.22f*forward, -0.16f);
                    break;
            }
            balls[starter].Get.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            starter++;
        }
        
        balls[8].Get.transform.localPosition = new Vector3(3.06f*forward,0);
        balls[8].Get.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        balls[9].Get.transform.localPosition = new Vector3(2.5f*forward,0);
        balls[9].Get.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
    }
}
