using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class AiManager : MonoBehaviour
{
    public GameObject HumanPrefab;

    public int populationSize = 20;
    private int generationNumber = 0;
    private int[] layers = new int[] { 25, 50, 50, 8 };
    private List<NeuralNetwork> nets;
    private List<WalkAi> humanList = null;

    public Text TextInfo;

    public LateCameraFollow lateCameraFollow;

    [HideInInspector] public float Countdown = -1;
    public float LearningTime = 20f;

    float bestLastFitness           = -1;
    float bestLastTimeAlive         = -1;
    float bestOverallFitness        = -1;
    float bestOverallTimeAlive      = -1;
    float bestOverallGenFitness     = -1;
    float bestOverallGenTimeAlive   = -1;

    bool nextGen = true;

    public Text TextTimeDelta;

    public void SetTimeDelta(Slider s)
    {
        TextTimeDelta.text = $"TimeScale: {s.value}";
        Time.timeScale = s.value;
    }

    NeuralNetwork netBestScore = null, netBestTime = null;

    public void SaveBestNetwork()
    {
        if (netBestScore != null)
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, "bestScoreNetwork.bin"), netBestScore.Dump());

        if (netBestTime != null)
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, "bestTimeNetwork.bin"), netBestTime.Dump());
    }

    public void BtnStartLearning(GameObject btnObj)
    {
        isStarted = true;
        Destroy(btnObj);
    }

    public WoD wod;

    public bool isStarted = false;

    enum CauseOfLastGenDeath
    {
        EveryoneDied,
        Timeout
    }

    CauseOfLastGenDeath cause = CauseOfLastGenDeath.Timeout;

    List<float> AverageScores = new List<float>();

    public void KillActive()
    {
        if (lateCameraFollow.TrackingObject != null)
            lateCameraFollow.TrackingObject.GetComponent<WalkAi>().Kill();
    }

    public int CalculateProgress(int current, int max)
    {
        return (int)(max != 0 ? current * 100 / max : 0);
    }

    List<float> FitGraph = new List<float>(); // scale 0-19
    List<float> TimeGraph = new List<float>(); // scale 0-19
    public Text TextGraph;
    public Text TextGraphTime;
    public List<int> graphValues = new List<int>();

    void DrawGraph(Text graphText, List<float> graph)
    {
        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < graph.Count; i++)
        {
            if (min > graph[i]) min = graph[i];
            if (max < graph[i]) max = graph[i];
        }

        graphValues = new List<int>();
        for (int i = 0; i < graph.Count; i++)
            graphValues.Add(CalculateProgress((int)graph[i], (int)max) / 20);

        char[,] table = new char[20, 100];
        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 100; j++)
                table[i, j] = ' ';

        for (int i = 0; i < graphValues.Count; i++)
            table[graphValues[i] - 19, i] = '-';

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 100; j++)
                sb.Append(table[i, j]);
            sb.Append('\n');
        }

        graphText.text = sb.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted) return;

        var sb = new StringBuilder();
        int alive = 0;

        sb.Append($"Time left: {System.Math.Round(Countdown, 1)}\n");
        sb.Append($"Generation {generationNumber}\n");
        Countdown -= Time.deltaTime;

        if (nets != null && nets.Count > 0 && humanList.Count > 0)
        {
            nextGen = true;
            foreach (var human in humanList)
                if (!human.dontReward)
                {
                    nextGen = false;
                    alive++;
                }

            if (nextGen)
                Countdown = -1;
        }

        if (Countdown < 0f)
        {
            lateCameraFollow.TrackingObject = null;

            if (generationNumber == 0)
            {
                sb.Append($"Best current fitness: 0\n");
                InitHumanNeuralNetworks();
            }
            else
            {
                if (nextGen)    cause = CauseOfLastGenDeath.EveryoneDied;
                else            cause = CauseOfLastGenDeath.Timeout;

                for (int i = 0; i < humanList.Count; i++)
                    AverageScores.Add(nets[i].GetFitness());

                nets.Sort();

                float lastMaxFitness = nets[nets.Count - 1].GetFitness();
                bestLastFitness = lastMaxFitness;
                bestLastTimeAlive = nets[nets.Count - 1].GetTimeAlive();

                while (FitGraph.Count >= 20)    FitGraph.RemoveAt(0);
                while (TimeGraph.Count >= 20)   TimeGraph.RemoveAt(0);

                FitGraph.Add(bestLastFitness);
                TimeGraph.Add(bestLastTimeAlive);

                DrawGraph(TextGraph, FitGraph);
                DrawGraph(TextGraphTime, TimeGraph);

                if (bestOverallFitness < lastMaxFitness)
                {
                    bestOverallFitness = lastMaxFitness;
                    bestOverallGenFitness = generationNumber;
                    netBestScore = nets[nets.Count - 1];
                }

                if (bestOverallTimeAlive < bestLastTimeAlive)
                {
                    bestOverallTimeAlive = bestLastTimeAlive;
                    bestOverallGenTimeAlive = generationNumber;
                    netBestTime = nets[nets.Count - 1];
                }

                for (int i = 0; i < populationSize / 2; i++)
                {
                    nets[i] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                    nets[i].Mutate(i == 0);

                    nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                }

                for (int i = 0; i < populationSize; i++)
                    nets[i].SetFitness(0);
            }

            generationNumber++;
            sb.Append($"Best current fitness: {generationNumber}\n");

            wod.Restart();

            Countdown = LearningTime;
            CreateHumanBodies();

            lateCameraFollow.TrackingObject = humanList[0].gameObject;
        }

        if (nets.Count > 0 && humanList.Count > 0 && Countdown > 0)
        {
            float bestScore = float.MinValue;
            int bestI = 0;
            for (int i = 0; i < nets.Count; i++)
            {
                var netScore = nets[i].GetFitness();
                if (netScore > bestScore && !humanList[i].dontReward)
                {
                    bestScore = netScore;
                    bestI = i;
                }
            }

            for (int i = 0; i < humanList.Count; i++)
            {
                if (i == bestI && bestScore >= 0)
                    humanList[i].MakeVisible();
                else
                    humanList[i].Shade();
            }

            if (bestScore >= 0)
                lateCameraFollow.TrackingObject = humanList[bestI].gameObject;
            else
                lateCameraFollow.TrackingObject = null;

            sb.Append($"Best current fitness: {System.Math.Round(bestScore, 2)}\n");
        }

        if (bestLastFitness >= 0)
        {
            sb.Append($"Best last fitness: {System.Math.Round(bestLastFitness, 2)}\n");
            sb.Append($"Best last time alive: {System.Math.Round(bestLastTimeAlive, 2)}\n");
        }

        if (bestOverallFitness >= 0 && bestOverallGenFitness > 0)
        {
            sb.Append($"Best overall fitness was {System.Math.Round(bestOverallFitness, 2)} in Gen {bestOverallGenFitness}\n");
            sb.Append($"Best overall time alive was {System.Math.Round(bestOverallTimeAlive, 2)}s in Gen {bestOverallGenTimeAlive}\n");
        }

        if (AverageScores.Count > 0)
        {
            float score = 0f;
            for (int i = 0; i < AverageScores.Count; i++)
                score += AverageScores[i];
            score /= AverageScores.Count;

            sb.Append($"Average overall was {score}\n");
        }

        if (generationNumber > 1)
            sb.Append($"Cause of last gen death: {(cause == CauseOfLastGenDeath.Timeout ? "Timeout" : "Everyone died")}\n");

        sb.Append($"Alive: {alive}/{populationSize}\n");

        TextInfo.text = sb.ToString();
    }

    void CreateHumanBodies()
    {
        if (humanList != null)
        {
            for (int i = 0; i < humanList.Count; i++)
                Destroy(humanList[i].gameObject);
        }
        humanList = new List<WalkAi>();

        for (int i = 0; i < populationSize; i++)
        {
            var obj = Instantiate(HumanPrefab, new Vector3(0, -1.8f, 0), Quaternion.identity).GetComponent<WalkAi>();
            obj.Init(nets[i]);
            humanList.Add(obj);
            obj.manager = this;
            obj.netInitalized = true;
        }
    }

    void InitHumanNeuralNetworks()
    {
        if (populationSize % 2 != 0) populationSize = 20;

        nets = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate(true);
            nets.Add(net);
        }
    }
}
