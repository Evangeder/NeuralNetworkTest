using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : System.IComparable<NeuralNetwork>
{
    private int[]       _layers;
    private float[][]   _neurons;
    private float[][][] _weights;
    private float       _fitness;
    private float       _timeAlive;

    public NeuralNetwork(int[] layers)
    {
        _layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            _layers[i] = layers[i];

        InitNeurons();
        InitWeights();
    }

    public byte[] Dump()
    {
        System.IO.MemoryStream memStream = new System.IO.MemoryStream();
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(memStream);

        bw.Write(_layers.Length);
        for (int i = 0; i < _layers.Length; i++)
            bw.Write(_layers[i]);

        bw.Write(_neurons.Length);
        for (int i = 0; i < _neurons.Length; i++)
        {
            bw.Write(_neurons[i].Length);
            for (int j = 0; j < _neurons[i].Length; j++)
                bw.Write(_neurons[i][j]);
        }

        bw.Write(_weights.Length);
        for (int i = 0; i < _weights.Length; i++)
        {
            bw.Write(_weights[i].Length);
            for (int j = 0; j < _weights[i].Length; j++)
            {
                bw.Write(_weights[i][j].Length);
                for (int k = 0; k < _weights[i][j].Length; k++)
                    bw.Write(_weights[i][j][k]);
            }
        }

        return memStream.ToArray();
    }

    public void Load(byte[] dump)
    {
        System.IO.MemoryStream memStream = new System.IO.MemoryStream(dump);
        System.IO.BinaryReader br = new System.IO.BinaryReader(memStream);

        _layers = new int[br.ReadInt32()];
        for (int i = 0; i < _layers.Length; i++)
            _layers[i] = br.ReadInt32();

        _neurons = new float[br.ReadInt32()][];
        for (int i = 0; i < _neurons.Length; i++)
        {
            _neurons[i] = new float[br.ReadInt32()];
            for (int j = 0; j < _neurons[i].Length; j++)
                _neurons[i][j] = br.ReadSingle();
        }

        _weights = new float[br.ReadInt32()][][];
        for (int i = 0; i < _weights.Length; i++)
        {
            _weights[i] = new float[br.ReadInt32()][];
            for (int j = 0; j < _weights[i].Length; j++)
            {
                _weights[i][j] = new float[br.ReadInt32()];
                for (int k = 0; k < _weights[i][j].Length; k++)
                    _weights[i][j][k] = br.ReadSingle();
            }
        }
    }

    public NeuralNetwork(NeuralNetwork network)
    {
        _layers = new int[network._layers.Length];
        for (int i = 0; i < network._layers.Length; i++)
            _layers[i] = network._layers[i];

        SetFitness(network.GetFitness());

        InitNeurons();
        InitWeights();

        CopyWeights(network._weights);
    }

    private void CopyWeights(float[][][] weights)
    {
        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    _weights[i][j][k] = weights[i][j][k];
    }

    private void InitNeurons()
    {
        var neuronsList = new List<float[]>();
        for (int i = 0; i < _layers.Length; i++)
            neuronsList.Add(new float[_layers[i]]);
        _neurons = neuronsList.ToArray();
    }

    private void InitWeights()
    {
        var weightsList = new List<float[][]>();
        for (int i = 1; i < _layers.Length; i++)
        {
            var layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = _layers[i - 1];
            for (int j = 0; j < _neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                    neuronWeights[k] = Random.Range(-0.5f, 0.5f);

                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        _weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
            _neurons[0][i] = inputs[i];

        for (int i = 1; i < _layers.Length; i++)
        {
            for (int j = 0; j < _neurons[i].Length; j++)
            {
                float value = 0.25f;

                for (int k = 0; k < _neurons[i - 1].Length; k++)
                    value += _weights[i - 1][j][k] * _neurons[i - 1][k];

                _neurons[i][j] = (float)System.Math.Tanh(value);
            }
        }

        return _neurons[_neurons.Length - 1];
    }

    public void Mutate(bool firstMutation = false)
    {
        for (int i = 0; i < _weights.Length; i++)
            for (int j = 0; j < _weights[i].Length; j++)
                for (int k = 0; k < _weights[i][j].Length; k++)
                {
                    float weight = _weights[i][j][k];

                    int rand = firstMutation ? 4 : Random.Range(0, 30);
                    switch (rand)
                    {
                        case 0: weight *= -1f;                          break;
                        case 1: weight  = Random.Range(-0.5f, 0.5f);    break;
                        case 2: weight *= Random.Range(0f, 1f) + 1f;    break;
                        case 3: weight *= Random.Range(0f, 1f);         break;
                        default:
                        case 4: break;
                    }

                    _weights[i][j][k] = weight;
                }
    }

    public void Reward(float fit) => _fitness += fit;
    public void Punish(float fit) => _fitness -= fit;

    public void SetFitness(float fit) => _fitness = fit;
    public float GetFitness() => _fitness;

    public void SetTimeAlive(float t) => _timeAlive = t;
    public float GetTimeAlive() => _timeAlive;

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (_timeAlive > other._timeAlive)
        {
            if (_fitness > other._fitness) return 1;
            else return 0;
        }

        if      (_fitness >= other._fitness) return 0;
        else    return -1;
    }
}
