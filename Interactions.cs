using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : MonoBehaviour {

    public PlayGame pg;
    
    public Transform[] holes;
    public Transform holesDir;

    public bool[] ballsActive;
    public bool[] checkedBall;
    public Data[] ballData;

	// Use this for initialization
	void Start () {
		ballsActive = new bool[10] {true,true,true,true,true,true,true,true,true,true};
        checkedBall = new bool[10] {false,false,false,false,false,false,false,false,false,false};
        ballData = new Data[9];
        holes = new Transform[6];
        SetDrag(0.5f);
        for (int x = 0; x < holesDir.childCount; x++) 
        {
            holes[x] = holesDir.GetChild(x);
        }
    }
	
    public class Data
    {
        public float DistCue { get; set; }
        public float AngleCue { get; set; }
        public float[] DistHoles { get; set; }
        public bool Scored { get; set; }

        public Data(float distCue, float angleCue, float[] distHoles)
        {
            DistCue = distCue;
            AngleCue = angleCue;
            DistHoles = new float[distHoles.Length];
            Scored = false;
            for (int x = 0; x < distHoles.Length; x++)
            {
                DistHoles[x] = distHoles[x];
            }
        }
    }

    // Update is called once per frame
    void Update () 
    {
        DealWithBalls();
        UpdateData();
	}

    private void UpdateData()
    {
        Transform ball;
        Transform cue = gameObject.transform.GetChild(0);
        for (int x = 0; x < ballData.Length; x++)
        {
            ball = gameObject.transform.GetChild(x+1);
            float[] holesDist = new float[6];
            float cueDist = Mathf.Abs(Vector2.Distance(cue.localPosition, ball.localPosition));
            float cueAngle = pg.AngleBetween(cue.localPosition, ball.localPosition);
            for (int y = 0; y < 6; y++)
            {
                float distance = Mathf.Abs(Vector3.Distance(holes[y].localPosition, ball.localPosition));
                holesDist[y] = distance;
            }
            Data temp = new Data(cueDist, cueAngle, holesDist);
            ballData[x] = temp;
        }
    }

    private void DealWithBalls()
    {
        for (int x = 0; x < ballsActive.Length; x++)
        {
            if (!CheckIfAlive(gameObject.transform.GetChild(x), x))
                gameObject.transform.GetChild(x).GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        CheckResetCue();
        bool checkBefore = true;
        for (int x = 1; x < ballsActive.Length; x++)
        {
            Transform ball = gameObject.transform.GetChild(x);
            bool checker = CheckIfAlive(ball, x);
            if (!checker && checkBefore && !checkedBall[x])
            {
                Debug.Log("Scored: " + ball.gameObject.name);
                checkedBall[x] = true;
                ballData[x-1].Scored = true;
            }
            else if (!checker && !checkedBall[x])
            {
                Debug.Log("Scored: " + ball.gameObject.name + " in wrong order");
                checkedBall[x] = true;
                ballData[x-1].Scored = true;
            }
            if (!ballsActive[x])
            {
                checkBefore = checkBefore && !ballsActive[x];
                continue;
            }
            else
            {
                checkBefore = false;
            }
        }
    }

    public void CheckResetCue(bool overRide = false)    //When overRide == true, the cue doesn't reset
    {
        if (!ballsActive[0] && !overRide || Mathf.Abs(gameObject.transform.GetChild(0).localPosition.x) > 5.0f || Mathf.Abs(gameObject.transform.localPosition.y) > 2.5f)
        {
            //Debug.Log("Resetting Cue");
            gameObject.transform.GetChild(0).localPosition = new Vector2(2.5f, 0);
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            gameObject.transform.GetChild(0).GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            ballsActive[0] = true;
            pg.RackBalls();
        }
        float xVal = gameObject.transform.GetChild(0).localPosition.x;
        float yVal = gameObject.transform.GetChild(0).localPosition.y;

        if (Mathf.Abs(gameObject.transform.GetChild(0).localPosition.x)>5)
        {
            xVal = (gameObject.transform.GetChild(0).localPosition.x/gameObject.transform.GetChild(0).localPosition.x);
        }
    }

    private void SetDrag(float val)
    {
        for (int x = 0; x < ballsActive.Length; x++)
        {
            Rigidbody2D body = gameObject.transform.GetChild(x).GetComponent<Rigidbody2D>();
            body.drag = val;
        }
    }

    private bool CheckIfAlive(Transform ball, int index)
    {
        bool allGood = true;
        for (int x = 0; x < 6; x++)
        {
            float distance = Mathf.Abs(Vector3.Distance(holes[x].localPosition, ball.localPosition));
            if (distance < 0.3f)
            {
                ball.gameObject.SetActive(false);
                ballsActive[index] = false;
                return false;
            }
        }
        if (allGood)
        {
            ball.gameObject.SetActive(true);
            ballsActive[index] = true;
        }
        return true;
    }
}
