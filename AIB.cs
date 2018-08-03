using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class AIB : MonoBehaviour {

    //BILLIARDS
    public Interactions interact;
    public PlayGame play;
    public GameObject neuronSprite;

    public float startingInputs = 0.5f;
    public float startingWeights = 0.5f;
    public int hidden1 = 10;
    public float bias1 = 0.25f;
    public int hidden2 = 10;
    public float bias2 = 0.25f;
    public float outBias = 0.2f;

    public int addConstants = 0;

    private float learningRate = 0.01f;

    public GameObject neuron;
    public GameObject weight;

    public float[] inputLayer;
    public float[] outputLayer;
    public float[] yLayer;

    public float runningAverageCost = 0.0f;

    //For input improvement  
    public float[] muscleDer0;
    //Input = before then after

    private float[,] weightLayer0;
    private float[] biasLayer1;
    private float[] hiddenLayer1;
    private float[,] weightLayer1;
    private float[] biasLayer2;
    private float[] hiddenLayer2;
    private float[,] weightLayer2;
    private float[] outputBias;

    private int numberOfTrains = 0;
    private float hugeTotalCost = 0.0f;

    //For robust backprop (just to make sure it works)
    private float[] hiddenLayer1a;
    private float[] hiddenLayer2a;
    private float[] outputLayera;

    //Derivative Weights (used for gradient descent)
    private float[] derInputs;
    private float[,] derWeights0;
    private float[,] derWeights1;
    private float[,] derWeights2;
    private float[] derBias2;
    private float[] derBias1;
    private float[] derBias0;

    private float[] oldDers;   //To compare the net for a forward-backward network

    private float[] error;
    private float[] cost;

    public bool ready = false;
    public int currentGoal = 1;

    //private List<> derWeights0;
    public bool zoomedIn = false;
    public bool zoomedInTable = false;

    public void RunBilliardAI()
    {
        SpawnNeuralNet();
        RunOnce(false);
        UpdateInputs();
        ready = true;
        //StartCoroutine(AIPlays());
    }
    /*
    IEnumerator AIPlays()
    {
        ready = false;
        play.playerShooting = false;            //AI controls now
        Debug.Log("Starting AI");
        yield return new WaitForSeconds(10.0f);     //Prepare your butts
        for (int x = 0; x < 100; x++)
        {
            Debug.Log("Running Forward");
            RunOnce(false);
            float dist = outputLayer[0];
            float xA = outputLayer[1];
            float yA = outputLayer[2];
            float angle = 360.0f*Mathf.Atan(yA/xA)/(2*Mathf.PI);
            Debug.Log("Shooting");
            yield return StartCoroutine(play.Shoot(angle, false, dist));    //Shoot!
            yield return new WaitForSeconds(1.0f);
        }
        ready = true;
    }
    */
    private void Update()
    {
        /*
        if (ready)
            UpdateInputs();      
        */
    }

    private void UpdateInputs()
    {
        float offset = 0.28f;    //makes 'input == 0' at contact
        float maxDistance = 10.0f;

        //Debug.Log(interact.ballData.Length);
        int x = 0;
        for (x = 0; x < interact.ballData.Length; x++)
        {

            if (!interact.ballsActive[x + 1])
            {
                Debug.Log("The " + x + "+1 ball isn't active so we set the inputs to 0");
                inputLayer[x] = 0.0f;
                inputLayer[x*2+interact.ballData.Length] = 0.0f;
                inputLayer[x*2+1+interact.ballData.Length] = 0.0f;

                for (int y = 0; y < interact.holes.Length; y++)
                {
                    inputLayer[x + (2+y)*interact.ballData.Length] = 0.0f;
                }
                break;
            } else
            {
                inputLayer[x] = sigmoid(interact.ballData[x].DistCue - offset - (maxDistance / 2));       //Distance to cue
                                                                                                          //Debug.Log("CueDist: " + inputLayer[x] + " sigmoid: " + sigmoid(inputLayer[x]));
                float xA = Mathf.Cos(Mathf.PI * 2 * interact.ballData[x].AngleCue / 360.0f);
                float yA = Mathf.Sin(Mathf.PI * 2 * interact.ballData[x].AngleCue / 360.0f);

                if (xA < 0.0f)      //Setting x angle
                {
                    inputLayer[x * 2 + interact.ballData.Length] = -sigmoid(xA);
                }
                else
                {
                    inputLayer[x * 2 + interact.ballData.Length] = sigmoid(xA);
                }
                if (yA < 0.0f)      //Setting y angle
                {
                    inputLayer[x * 2 + 1 + interact.ballData.Length] = sigmoid(yA);
                }
                else
                {
                    inputLayer[x * 2 + 1 + interact.ballData.Length] = -sigmoid(yA);
                }
                //Debug.Log("CueXAngle: " + xA + " Sigmoid: " + sigmoid(xA));
                //Debug.Log("CueYAngle: " + yA + " Sigmoid: " + sigmoid(yA));
                for (int y = 0; y < interact.holes.Length; y++)
                {
                    inputLayer[x + (2 + y) * interact.ballData.Length] = sigmoid(interact.ballData[x].DistHoles[y]);
                }
            }
        }
        inputLayer[x+(6+2)*interact.ballData.Length] = sigmoid(currentGoal-1-4);
        //Debug.Log(inputLayer[x + (6 + 2) * interact.ballData.Length]);
        for (x = 0; x < inputLayer.Length; x++)
        {
            if (Mathf.Abs(inputLayer[x]) > 1.0f)
            {
                Debug.Log("Error, input out of range [-1,1]");
                Debug.Log(inputLayer[x]);
            }
        }
    }

    private void NumberfyNeurons(bool on)
    {
        for (int x = 0; x < inputLayer.Length; x++)
        {
            TextMeshPro texty = gameObject.transform.GetChild(2).GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if (on) {
                texty.SetText(inputLayer[x].ToString("F2"));
            } else {
                texty.SetText("");
            }
        }
        for (int x = 0; x < hiddenLayer1.Length; x++)
        {
            TextMeshPro texty = gameObject.transform.GetChild(3).GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if (on) {
                texty.SetText(hiddenLayer2[x].ToString("F2"));
            } else {
                texty.SetText("");
            }
        }
        for (int x = 0; x < hiddenLayer2.Length; x++)
        {
            TextMeshPro texty = gameObject.transform.GetChild(4).GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if (on) {
                texty.SetText(hiddenLayer2[x].ToString("F2"));
            } else {
                texty.SetText("");
            }
        }
        for (int x = 0; x < outputLayer.Length; x++)
        {
            TextMeshPro texty = gameObject.transform.GetChild(5).GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if (on) {
                texty.SetText(outputLayer[x].ToString("F2"));
            } else {
                texty.SetText("");
            }
        }
        for (int x = 0; x < yLayer.Length; x++)
        {
            TextMeshPro texty = gameObject.transform.GetChild(6).GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if (on)
            {
                texty.SetText(yLayer[x].ToString("F2"));
            }
            else
            {
                texty.SetText("");
            }
        }
    }

    public void ZoomIn()
    {
        if (!zoomedIn)
        {
            //gameObject.transform.localPosition = -gameObject.transform.parent.parent.localPosition;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.localScale = new Vector3(7f, 7f, 0f);
            zoomedIn = true;
            NumberfyNeurons(true);
        }
        else
        {
            gameObject.transform.localPosition = new Vector3(0, -5f, 0);
            gameObject.transform.localScale = new Vector3(1f,1f, 0);
            zoomedIn = false;
            NumberfyNeurons(false);
        }
    }

    private void ColorNeurons(Transform neuronLayer, float[] neurons)
    {
        for (int x = 0; x < neurons.Length; x++)
        {
            float r,g,b;
            r = 1.0f;
            g = sigmoid(neurons[x]);
            //Debug.Log(g);
            b = 0.0f;
            neuronLayer.GetChild(x).GetComponent<SpriteRenderer>().color =  new Color(r,g,b);
            TextMeshPro texty = neuronLayer.GetChild(x).GetChild(0).GetComponent<TextMeshPro>();
            if(zoomedIn)
            {
                texty.SetText(neurons[x].ToString("F2"));
            }
            else
            {
                texty.SetText("");
            }
        }
    }

    private void ColorWeights(Transform weightLayer, float[,] weights)
    {
        int currentWeight = 0;
        for (int x = 0; x < weights.GetLength(1); x++)
        {
            for (int y = 0; y < weights.GetLength(0); y++)
            {
                float r,g,b,a;
                r = g = b = sigmoid(weights[y,x]);
                a = Mathf.Abs(2*(sigmoid((weights[y,x]))-0.5f));
                weightLayer.GetChild(currentWeight).GetComponent<SpriteRenderer>().color = new Color(r,g,b,a);
                currentWeight++;
            }
        } 
    }

    private void DrawWeights(Transform macroLayer, float[,] microLayer, int inLayer, int sortingOrder = 7)
    {
        for(int x = 0; x < microLayer.GetLength(1); x++)
        {
            for(int y = 0; y < microLayer.GetLength(0); y++)
            {
                float verticalPosition = (gameObject.transform.GetChild(inLayer).transform.GetChild(x).localPosition.y + gameObject.transform.GetChild(inLayer+1).transform.GetChild(y).localPosition.y)/2;
                float angle = 360.0f*(Mathf.Atan(verticalPosition-gameObject.transform.GetChild(inLayer).transform.GetChild(x).localPosition.y)/1)/(2*Mathf.PI);
                float scale = 2*Mathf.Sqrt(Mathf.Pow(verticalPosition-gameObject.transform.GetChild(inLayer).transform.GetChild(x).localPosition.y,2) + 1);
                GameObject weightObj = Instantiate(weight, new Vector3(0.0f,verticalPosition,0.0f), macroLayer.rotation, macroLayer);
                weightObj.transform.Rotate(new Vector3(0, 0, angle));
                weightObj.transform.localPosition = new Vector3(0.0f,verticalPosition,0.0f);
                weightObj.transform.localScale = new Vector3(scale,0.1f,1.0f);
                weightObj.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            }
        }
    }

    public void DrawNeurons(Transform macroLayer, float[] microLayer, int numNeurons, int sortingOrder = 8, int numRows = 1)
    {
        float scale = 0.25f;
        float height = 3.0f;
        float positionMax = height - scale;
        float room = positionMax - height / 2;
        int remainderNeurons = numNeurons % numRows;
        scale = numRows*(room/(numNeurons));                //Might as well use scale again
        for (int row = 1; row <= numRows; row++)
        {
            int neuronsPerRow = numNeurons / numRows;
            if (remainderNeurons > 0)
            {
                neuronsPerRow++;
            }
            for (int x = 0; x < neuronsPerRow; x++)
            {
                float verticalPosition = 0;
                if (neuronsPerRow % 2 == 1)
                {
                    if (x == 0)
                    {
                        verticalPosition = 0;
                    }
                    else if (x % 2 == 1)
                    {
                        verticalPosition = (x + 1) * scale;
                    }
                    else
                    {
                        x = x - 1;
                        verticalPosition = (x + 1) * scale;
                        x++;
                    }
                }
                else
                {
                    if (x % 2 == 1)
                    {
                        x = x - 1;
                        verticalPosition = (x + 1) * scale;
                        x++;
                    }
                    else
                    {
                        verticalPosition = (x + 1) * scale;
                    }
                }
                neuron = Instantiate(neuronSprite, macroLayer.position, macroLayer.rotation, macroLayer);
                neuron.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                if (x % 2 == 1)
                {
                    verticalPosition = verticalPosition * -1;
                }
                macroLayer.transform.GetChild(x).transform.localPosition = new Vector3(0, verticalPosition, 0);
                macroLayer.transform.GetChild(x).GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            }
        }
    }

    public void SpawnNeuralNet()
    {
        int index = 1;  //0 = Just cueDist & cueAngle   1 = cueDist, cueAngle, & distHoles
        switch (index)
        {
            case 0:
                inputLayer = new float[interact.ballData.Length*3+1];
                break;
            case 1:
            default:
                inputLayer = new float[interact.ballData.Length*3+interact.holes.Length*interact.ballData.Length+1];
                break;
        }
        UpdateInputs();

        outputLayer = new float[3];
        outputLayera = new float[outputLayer.Length];
        yLayer = new float[outputLayer.Length];
        System.Random rnd = new System.Random();

        weightLayer0 = new float[hidden1,inputLayer.Length];
        for (int x = 0; x < weightLayer0.GetLength(0); x++)
        {
            for (int y = 0; y < weightLayer0.GetLength(1); y++)
            {
               weightLayer0[x,y] = 2*((float)rnd.NextDouble())-1;
            }
        }
        
        //Maybe not needed
        hiddenLayer1 = new float[hidden1+addConstants];
        hiddenLayer1a = new float[hidden1+addConstants];
        for (int x = 0; x < hiddenLayer1.Length; x++)
        {
            hiddenLayer1[x] = 0.5f;
        }
        for (int x = 0; x < addConstants; x++)
        {
            hiddenLayer1[hiddenLayer1.Length-1-x] = 1.0f;
        }
        biasLayer1 = new float[hidden1+addConstants];
        for (int x = 0; x < biasLayer1.Length; x++)
        {
            biasLayer1[x] = 0.0f;// bias1;
        }

        weightLayer1 = new float[hidden2, hidden1+addConstants];
        //Debug.Log("WeightLayer1(0): "+weightLayer1.GetLength(0));
        //sDebug.Log("WeightLayer1(1): "+weightLayer1.GetLength(1));
        for (int x = 0; x < weightLayer1.GetLength(0); x++)
        {
            for (int y = 0; y < weightLayer1.GetLength(1); y++)
            {
                //weightLayer1[x,y] = startingWeights;
                weightLayer1[x,y] = 2*((float)rnd.NextDouble())-1;
            }
        }

        //Maybe not needed
        hiddenLayer2 = new float[hidden2+addConstants];
        hiddenLayer2a = new float[hidden2+addConstants];
        for (int x = 0; x < hiddenLayer2.Length; x++)
        {
            hiddenLayer2[x] = 0.5f;
        }
        for (int x = 0; x < addConstants; x++)
        {
            hiddenLayer2[hiddenLayer2.Length-1-x] = 1.0f;
        }

        biasLayer2 = new float[hidden2+addConstants];
        for (int x = 0; x < biasLayer2.Length; x++)
        {
            biasLayer2[x] = 0.0f;   //bias2;
        }

        weightLayer2 = new float[outputLayer.Length, hidden2+addConstants];
        for (int x = 0; x < weightLayer2.GetLength(0); x++)
        {
            for (int y = 0; y < weightLayer2.GetLength(1); y++)
            {
                //weightLayer2[x, y] = startingWeights;
                weightLayer2[x, y] = 2*((float)rnd.NextDouble())-1;
            }
        }

        for (int x = 0; x < outputLayer.Length; x++)
        {
            outputLayer[x] = 0.5f;
        }
        outputBias = new float[outputLayer.Length];
        for (int x = 0; x < outputBias.Length; x++)
        {
            outputBias[x] = 0.0f;//outBias;
        }

        for (int x = 0; x < yLayer.Length; x++)
        {
            yLayer[x] = 0.5f;
        }

        DrawNeurons(gameObject.transform.GetChild(2).transform, inputLayer, inputLayer.Length);
        DrawNeurons(gameObject.transform.GetChild(3).transform, hiddenLayer1, hiddenLayer1.Length);
        DrawNeurons(gameObject.transform.GetChild(4).transform, hiddenLayer2, hiddenLayer2.Length);
        DrawNeurons(gameObject.transform.GetChild(5).transform, outputLayer, outputLayer.Length);
        DrawNeurons(gameObject.transform.GetChild(6).transform, yLayer, yLayer.Length);

        DrawWeights(gameObject.transform.GetChild(7).transform, weightLayer0, 2);
        DrawWeights(gameObject.transform.GetChild(8).transform, weightLayer1, 3);
        DrawWeights(gameObject.transform.GetChild(9).transform, weightLayer2, 4);

        ColorWeights(gameObject.transform.GetChild(7).transform, weightLayer0);
        ColorWeights(gameObject.transform.GetChild(8).transform, weightLayer1);
        ColorWeights(gameObject.transform.GetChild(9).transform, weightLayer2);

        ColorNeurons(gameObject.transform.GetChild(2).transform, inputLayer);
        ColorNeurons(gameObject.transform.GetChild(3).transform, hiddenLayer1);
        ColorNeurons(gameObject.transform.GetChild(4).transform, hiddenLayer2);
        ColorNeurons(gameObject.transform.GetChild(5).transform, outputLayer);
        ColorNeurons(gameObject.transform.GetChild(6).transform, yLayer);
    }

    public void UpdateOutputs()
    {
        DrawNeurons(gameObject.transform.GetChild(5).transform, outputLayer, outputLayer.Length);
        DrawNeurons(gameObject.transform.GetChild(6).transform, yLayer, yLayer.Length);
    }

    public void UpdateNN(int sortingLevel = 7)
    {
        for (int y = 2; y < 10; y++)
        {
            for (int x = 0; x < gameObject.transform.GetChild(y).childCount; x++)
            {
                if (y < 7)
                {
                    gameObject.transform.GetChild(y).GetChild(x).GetComponent<SpriteRenderer>().sortingOrder = sortingLevel+1;
                }
                else
                {
                    gameObject.transform.GetChild(y).GetChild(x).GetComponent<SpriteRenderer>().sortingOrder = sortingLevel;
                }
            }
        }
    }

    public float sigmoid(float value)
    {
        return 1.0f/(1.0f + Mathf.Pow((float)Math.E, -value));
    }

    public float sigmoidDerivative(float value)
    {
        float s = sigmoid(value);
        return s*(1.0f - s);
    }

    public void BackPropagation(bool goingBackwards = false)
    {
        float[] hiddenDerivative2 = new float[hiddenLayer2.Length];
        derWeights2 = new float[outputLayer.Length,hiddenLayer2.Length];
        derWeights1 = new float[hiddenLayer2.Length,hiddenLayer1.Length];
        derWeights0 = new float[hiddenLayer1.Length,inputLayer.Length];
        derInputs = new float[inputLayer.Length];

        derBias2 = new float[outputLayer.Length];
        derBias1 = new float[hiddenLayer2.Length];
        derBias0 = new float[hiddenLayer1.Length];

        for (int x = 0; x < outputLayer.Length; x++)
        {
            float costDerivative = 1.0f*error[x];
            //May want to check this ******* sigDer of sigDer to get z(L) in a(L)=sig(z(L))
            float sigDerivative = sigmoidDerivative(outputLayer[x]);
            //Debug.Log("sigDerivative: "+sigDerivative);
            derBias2[x] = sigDerivative * costDerivative;
            for (int y = 0; y < hiddenLayer2.Length; y++)
            {
                derWeights2[x,y] = hiddenLayer2[y]*sigDerivative*costDerivative;
                //Debug.Log(derWeights2[x,y]);
                    //cost derived by weight2 values
                hiddenDerivative2[y] += weightLayer2[x,y]*sigDerivative*costDerivative;
                    //cost derived by hidden2 neuron values
            }
        }
        float[] hiddenDerivative1 = new float[hiddenLayer1.Length];
        for (int y = 0; y < hiddenLayer2.Length-addConstants; y++)
        {
            //Debug.Log(hiddenLayer2[y]);
            float sigDerivative2 = sigmoidDerivative(hiddenLayer2[y]);
            derBias1[y] = sigDerivative2 * hiddenDerivative2[y];
            for (int z = 0; z < hiddenLayer1.Length; z++)
            {
                //Debug.Log(hiddenDerivative2[y]);
                derWeights1[y,z] = hiddenLayer1[z]*sigDerivative2*hiddenDerivative2[y];
                hiddenDerivative1[z] += weightLayer1[y, z] * sigDerivative2 * hiddenDerivative2[y];
            }
        }

        for (int z = 0; z < hiddenLayer1.Length-addConstants; z++)
        {
            float sigDerivative3 = sigmoidDerivative(hiddenLayer1[z]);
            //Debug.Log(hiddenLayer1a[z]);
            //Debug.Log("sigDerivative3: "+sigDerivative3);     //Make sure != 0
            derBias0[z] = sigDerivative3 * hiddenDerivative1[z];
            for (int a = 0; a < inputLayer.Length; a++)
            {
                //Debug.Log(hiddenDerivative1[z]);
                derWeights0[z,a] = inputLayer[a]*sigDerivative3*hiddenDerivative1[z];
                //Debug.Log(derWeights0[z,a]);
                derInputs[a] += weightLayer0[z,a]*sigDerivative3*hiddenDerivative1[z];
                //Debug.Log("DerWeights0: "+derWeights0[z,a]);
            }
        }
    }

    public void GradientDescent()
    {
        for (int x = 0; x < outputLayer.Length; x++)
        {
            for (int y = 0; y < hiddenLayer2.Length; y++)
            {
                //Debug.Log("Weight: "+weightLayer2[x,y]+" derWeights0: "+derWeights2[x,y]);
                weightLayer2[x,y] = weightLayer2[x,y]-derWeights2[x,y]*learningRate;
                outputBias[x] = outputBias[x]-derBias2[x]*learningRate;
            }
        }
        for (int x = 0; x < hiddenLayer2.Length-addConstants; x++)
        {
            for (int y = 0; y < hiddenLayer1.Length; y++)
            {
                weightLayer1[x,y] = weightLayer1[x,y]-derWeights1[x,y]*learningRate;
                biasLayer2[x] = biasLayer2[x]-derBias1[x]*learningRate;
            }
        }
        for (int x = 0; x < hiddenLayer1.Length-addConstants; x++)
        {
            for (int y = 0; y < inputLayer.Length; y++)
            {
                //Debug.Log("derWeights0("+x+","+y+"): "+ derWeights0[x,y]);
                //Debug.Log("weightLayer0("+x+","+y+"): "+ weightLayer0[x,y]);
                weightLayer0[x,y] = weightLayer0[x,y]-derWeights0[x,y]*learningRate;
                biasLayer1[x] = biasLayer1[x]-derBias0[x]*learningRate;
            }
        }
        ColorWeights(gameObject.transform.GetChild(7).transform, weightLayer0);
        ColorWeights(gameObject.transform.GetChild(8).transform, weightLayer1);
        ColorWeights(gameObject.transform.GetChild(9).transform, weightLayer2);
    }

    public void RunNetwork(bool goingBackwards = false)
    {
        UpdateInputs();
        numberOfTrains++;

        for (int x = 0; x < hiddenLayer1.Length-addConstants; x++)
        {
            hiddenLayer1a[x] = 0.0f;
            for (int y = 0; y < inputLayer.Length; y++)
            {
                hiddenLayer1a[x] += inputLayer[y] * weightLayer0[x, y]+biasLayer1[x];
            }
            hiddenLayer1a[x] += biasLayer1[x];
            hiddenLayer1[x] = sigmoid(hiddenLayer1a[x]);
            hiddenLayer1[x] = hiddenLayer1[x]*2-1;
        }

        for (int x = 0; x < hiddenLayer2.Length-addConstants; x++)
        {
            hiddenLayer2a[x] = 0.0f;
            for (int y = 0; y < hiddenLayer1.Length; y++)
            {
                hiddenLayer2a[x] += hiddenLayer1[y] * weightLayer1[x, y]+biasLayer2[x];
            }
            hiddenLayer2a[x] += biasLayer2[x];
            hiddenLayer2[x] = sigmoid(hiddenLayer2a[x]);
            hiddenLayer2[x] = hiddenLayer2[x]*2-1;
        }

        for (int x = 0; x < outputLayer.Length; x++)
        {
            outputLayera[x] = 0.0f;
            for (int y = 0; y < hiddenLayer2.Length; y++)
            {
                outputLayera[x] += hiddenLayer2[y] * weightLayer2[x, y]+outputBias[x];
            }
            outputLayera[x] += outputBias[x];
            outputLayer[x] = sigmoid(outputLayera[x]);
            if (x > 0)
                outputLayer[x] = outputLayer[x]*2-1;
            //Debug.Log(outputLayera[x]);
        }

        //Color updated neurons
        ColorNeurons(gameObject.transform.GetChild(2).transform, inputLayer);
        ColorNeurons(gameObject.transform.GetChild(3).transform, hiddenLayer1);
        ColorNeurons(gameObject.transform.GetChild(4).transform, hiddenLayer2);
        ColorNeurons(gameObject.transform.GetChild(5).transform, outputLayer);
    }

    public void CalculateErrors()
    {
        error = new float[outputLayer.Length];
        cost = new float[error.Length];
        float totalCost = 0.0f;
        for (int x = 0; x < outputLayer.Length; x++)
        {
            error[x] = (outputLayer[x] - yLayer[x])/2;
            cost[x] = Mathf.Pow(error[x], 2);
            totalCost += cost[x];
        }
        ColorNeurons(gameObject.transform.GetChild(6).transform, yLayer);
    }

    
    public void RunOnce(bool goingBackwards)
    {
        for (int x = 0; x < 1; x++)
        {
            //Run for results
            RunNetwork(goingBackwards);
            //Run Back-Propagation to find the changes we want
            BackPropagation(goingBackwards);
            //Run Gradient Descent to implent thoughtful changes
            GradientDescent();
            ColorWeights(gameObject.transform.GetChild(7).transform, weightLayer0);
            ColorWeights(gameObject.transform.GetChild(8).transform, weightLayer1);
            ColorWeights(gameObject.transform.GetChild(9).transform, weightLayer2);
        }
    }
}
