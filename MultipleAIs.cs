using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MultipleAIs : MonoBehaviour {

    public Game[] games;

    public class Game
    {
        public GameObject Get { get; set; }
        public GameObject Table { get; set; }
        public GameObject Balls { get; set; }
        public GameObject Players { get; set; }
        public GameObject Brain { get; set; }

        public Game(GameObject gm, GameObject tb, GameObject bl, GameObject pl, GameObject br)
        {
            Get = gm;
            Table = tb;
            Balls = bl;
            Players = pl;
            Brain = br;
        }
    }

    public void Start()
    {
        CreateGames();
    }

    public void Update()
    {
        Rays();
    }

    public void CreateGames()
    {
        games = new Game[gameObject.transform.childCount - 1];

        for (int x = 0; x < games.Length; x++)
        {
            GameObject game = gameObject.transform.GetChild(x+1).gameObject;
            GameObject table = game.transform.GetChild(0).GetChild(0).gameObject;
            GameObject balls = game.transform.GetChild(0).GetChild(1).gameObject;
            GameObject players = game.transform.GetChild(0).GetChild(2).gameObject;
            GameObject nns = game.transform.GetChild(0).GetChild(3).gameObject;
            Game temp = new Game(game, table, balls, players, nns);
            games[x] = temp;
        }
    }

    private void Rays()
    {
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Ray!");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject nn = hit.transform.gameObject;
                Debug.Log("Gotcha! "+nn);
                if ("GUINeuralNet" == nn.name)
                {
                    AIB script = nn.GetComponent<AIB>();
                    if (script.zoomedIn)
                    {
                        nn.GetComponent<Canvas>().sortingOrder = 6;
                        nn.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
                        script.UpdateNN();
                    } else
                    {
                        nn.GetComponent<Canvas>().sortingOrder = 15;
                        nn.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sortingOrder = 14;
                        script.UpdateNN(16);
                    }
                    script.ZoomIn();
                }
                else if ("Table Floor" == nn.name)
                {
                    Table test = nn.GetComponent<Table>();
                    /*
                    if (script.zoomedInTable)
                    {
                        script.zoomedInTable = false;
                        nn.GetComponent<Canvas>().sortingOrder = 6;
                        nn.GetComponent<SpriteRenderer>().sortingOrder = 5;
                        nn.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    }
                    else
                    {
                        script.zoomedInTable = true;
                        nn.GetComponent<Canvas>().sortingOrder = 15;
                        nn.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sortingOrder = 14;
                        nn.transform.localScale = new Vector3(5.0f, 5.0f, 0.0f);

                    }
                    script.ZoomIn();
                    */
                }
            }
        }
    }

    public void StartAIs()
    {
        StartCoroutine(AIPlays());
    }

    private float[] CrossOver(float[] nnWeights0, float[] nnWeight1)
    {
        System.Random r = new System.Random();
        float[] crossedWeights = new float[nnWeights0.Length];
        int chosenFrom0 = 0;
        int chosenFrom1 = 0;
        for (int x = 0; x < nnWeights0.Length; x++)
        {
            if (r.Next(2) == 0)
            {
                crossedWeights[x] = nnWeights0[x];
                chosenFrom0++;
            } else
            {
                crossedWeights[x] = nnWeight1[x];
                chosenFrom1++;
            }
        }
        Debug.Log("CrossOver) ChosenFrom0:"+chosenFrom0+", ChosenFrom1:"+chosenFrom1);
        return crossedWeights;
    }


    IEnumerator AIPlays()
    {
        //Stop all balls
        for (int x = 0; x < games.Length; x++)
        {
            for (int y = 0; y < games[x].Balls.transform.childCount; y++)
            {
                games[x].Balls.transform.GetChild(y).GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                games[x].Balls.transform.GetChild(y).gameObject.SetActive(true);
            }
        }

        //Setup
        for (int x = 0; x < games.Length; x++)
        {
            games[x].Get.GetComponent<PlayGame>().playerShooting = false;   //AI controls now
            games[x].Get.GetComponent<PlayGame>().RackBalls();

            AIB nn = games[x].Brain.GetComponent<AIB>();
            nn.SpawnNeuralNet();
            nn.RunNetwork();    //Automatically updates inputs
        }

        int round = 0;
        int maxRounds = 10;

        //All AIs ON
        bool running = true;
        while(running)
        {
            int[] ballObjectives = new int[games.Length];
            if (round > maxRounds)
            {
                round = 0;
                for (int x = 0; x < games.Length; x++)
                {
                    games[x].Get.GetComponent<PlayGame>().RackBalls();
                    ballObjectives[x] = 1;
                }
            }

            round++;
            Debug.Log("Shot " + round);
            for (int x = 0; x < games.Length; x++)
            {
                int ballObjective = 1;
                bool stillGood = true;

                Interactions action = games[x].Balls.GetComponent<Interactions>();
                for (int y = 1; y < action.ballsActive.Length; y++)
                {
                    if (!action.ballsActive[y] && stillGood)
                    {
                        ballObjective++;
                    } else
                    {
                        stillGood = false;
                    }
                }

                if (ballObjective > 9)
                {
                    games[x].Get.GetComponent<PlayGame>().RackBalls();
                    ballObjective = 1;
                }
                ballObjectives[x] = ballObjective;
                games[x].Get.GetComponent<PlayGame>().ChangeTurn();
                games[x].Get.GetComponent<PlayGame>().shooting = false;
                AIB nn = games[x].Brain.GetComponent<AIB>();
                nn.currentGoal = ballObjective;
                nn.RunNetwork();    //Automatically updates inputs
                nn.ready = true;
            }

            yield return new WaitForSeconds(1.0f);

            //Run all Forward
            Debug.Log("Play some pool");
            for (int x = 0; x < games.Length; x++)
            {
                AIB nn = games[x].Brain.GetComponent<AIB>();
                float dist = 1.5f*nn.outputLayer[0];
                
                float xA = nn.outputLayer[1];
                float yA = nn.outputLayer[2];
                float angle;
                if (xA < 0.0f)
                {
                    angle = 180.0f+180.0f * Mathf.Atan(yA / xA) / (Mathf.PI);
                }
                else
                {
                    angle = 180.0f * Mathf.Atan(yA / xA) / (Mathf.PI);
                }
                //Debug.Log("Game"+x+") Xa: "+xA+", Ya: "+yA+", Angle: "+angle);
                //Debug.Log("Shooting");
                StartCoroutine(games[x].Get.GetComponent<PlayGame>().AIShoot(xA, yA, dist));    //Shoot!
            }

            yield return new WaitForSeconds(10.0f);             //Prepare your butts

            Debug.Log("AI Classification");                     //Record closest ball to aimed hole shot
            float[] closestDistances = new float[games.Length];
            for (int x = 0; x < games.Length; x++)              
            {
                Interactions.Data ball1 = games[x].Balls.GetComponent<Interactions>().ballData[ballObjectives[x]-1];
                float closestDistance = 100.0f;
                if (ball1.Scored)
                {
                    closestDistance = 0.0f;
                    break;
                } else
                {
                    for (int y = 0; y < 6; y++)
                    {
                        if (ball1.DistHoles[y] < closestDistance)
                            closestDistance = ball1.DistHoles[y];
                    }
                }
                closestDistances[x] = closestDistance;          //Best 1 ball distance for one game
            }
            float[] unsorted = new float[closestDistances.Length];
            unsorted = closestDistances;
            Array.Sort(closestDistances);
            float meanValue = closestDistances[6];
            int numGood = 0;
            int numBad = 0;
            float[] bestOutputLayer = new float[games[0].Brain.GetComponent<AIB>().outputLayer.Length];

            for (int x = 0; x < games.Length; x++)              //Find all games that had the best distance
            {
                if (unsorted[x] > meanValue || numBad > 10)     //If they are all bad we keep just the last one
                               //Change this to better change when none are working
                {
                    numGood++;
                    games[x].Brain.transform.GetChild(1).GetComponent<SpriteRenderer>().color = Color.green;
                    //Survives!
                } else
                {
                    games[x].Brain.transform.GetChild(1).GetComponent<SpriteRenderer>().color = Color.red;
                    numBad++;
                    //Eliminated!
                }




                AIB nn = games[x].Brain.GetComponent<AIB>();
                Interactions.Data ball1 = games[x].Balls.GetComponent<Interactions>().ballData[ballObjectives[x]-1];
                Debug.Log("Closest Distance for "+x+": "+closestDistances[x]+", Scored: "+ball1.Scored);
                if (closestDistances[x] == closestDistances[0] || ball1.Scored)
                {
                    //This is a good game
                    for (int y = 0; y < nn.outputLayer.Length; y++)
                    {
                        bestOutputLayer[y] = nn.outputLayer[y];
                    }
                    games[x].Brain.transform.GetChild(1).GetComponent<SpriteRenderer>().color = Color.green;
                    numGood = numGood + 1;
                } else
                {
                    games[x].Brain.transform.GetChild(1).GetComponent<SpriteRenderer>().color = Color.red;
                    //This is a bad game
                    numBad = numBad + 1;
                }
            }
            Debug.Log("Good Games: "+numGood+", Bad Games: "+numBad);

            Vector3[] bestBallPos = new Vector3[10];

            for (int x = 0; x < games.Length; x++)
            {
                AIB nn = games[x].Brain.GetComponent<AIB>();
                GameObject balls = games[x].Balls;

                if (closestDistances[x] == closestDistances[0] || games[x].Balls.GetComponent<Interactions>().ballData[ballObjectives[x]-1].Scored)
                {
                    Debug.Log("Closest Distance: "+closestDistances[x]+", Scored:"+games[x].Balls.GetComponent<Interactions>().ballData[ballObjectives[x]-1].Scored);
                    for (int y = 0; y < nn.outputLayer.Length; y++)
                    {
                        nn.yLayer[y] = nn.outputLayer[y];           //It doesn't need to change that great shot
                        //Debug.Log("These should be the same: " + nn.yLayer[y] + " == " + nn.outputLayer[y]);
                    }
                    //Debug.Log("Power: "+nn.outputLayer[0]);
                    nn.yLayer[0] = nn.outputLayer[0]*1.1f;      //Let's try hitting it a little harder
                    if (nn.yLayer[0] > 1.0f)
                    {
                        nn.yLayer[0] = 1.0f;
                    }

                    for (int y = 0; y < balls.transform.childCount; y++)
                    {
                        bestBallPos[y] = balls.transform.GetChild(y).localPosition;
                    }

                } else
                {
                    for (int y = 0; y < nn.outputLayer.Length; y++)
                    {
                        nn.yLayer[y] = nn.outputLayer[y] + (bestOutputLayer[y]-nn.outputLayer[y])/2;           //It doesn't need to change that great shot
                    }
                }
                nn.UpdateOutputs();
            }

            //Copy the game
            yield return new WaitForSeconds(1.0f);
            for (int x = 0; x < games.Length; x++)
            {
                GameObject balls = games[x].Balls;
                for (int y = 0; y < balls.transform.childCount; y++)
                {
                    balls.transform.GetChild(y).localPosition = bestBallPos[y];
                }
            }

            //Learn from round
            for (int x = 0; x < games.Length; x++)
            {
                AIB nn = games[x].Brain.GetComponent<AIB>();
                nn.CalculateErrors();
                nn.BackPropagation();
                nn.GradientDescent();
                nn.ready = false;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
