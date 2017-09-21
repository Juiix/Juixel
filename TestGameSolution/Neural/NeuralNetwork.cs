using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Neural
{
    public class NeuralNetwork
    {
        public int[] Layers;
        public float[][] Neurons;
        public float[][][] Weights;

        private static Random Random = new Random(Environment.TickCount);

        public NeuralNetwork(int[] Layers)
        {
            this.Layers = new int[Layers.Length];
            for (int i = 0; i < Layers.Length; i++)
                this.Layers[i] = Layers[i];

            InitNeurons();
            InitWeights();
        }

        public NeuralNetwork(NeuralNetwork CopyFrom)
        {
            Layers = new int[CopyFrom.Layers.Length];
            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = CopyFrom.Layers[i];

            InitNeurons();
            InitWeights();

            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    for (int k = 0; k < Weights[i][j].Length; k++)
                        Weights[i][j][k] = CopyFrom.Weights[i][j][k];
        }

        private void InitNeurons()
        {
            List<float[]> NeuronsList = new List<float[]>();

            for (int i = 0; i < Layers.Length; i++)
                NeuronsList.Add(new float[Layers[i]]);
            Neurons = NeuronsList.ToArray();
        }

        private void InitWeights()
        {
            List<float[][]> WeightsList = new List<float[][]>();

            for (int i = 1; i < Layers.Length; i++)
            {
                List<float[]> LayerWeightsList = new List<float[]>();
                int NeuronsInPreviousLayer = Layers[i - 1];

                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    float[] NeuronWeights = new float[NeuronsInPreviousLayer];

                    for (int k = 0; k < NeuronsInPreviousLayer; k++)
                    {
                        NeuronWeights[k] = (float)Random.NextDouble() - 0.5f;
                    }

                    LayerWeightsList.Add(NeuronWeights);
                }

                WeightsList.Add(LayerWeightsList.ToArray());
            }

            Weights = WeightsList.ToArray();
        }

        public float[] FeedForward(float[] Inputs)
        {
            for (int i = 0; i < Inputs.Length; i++)
                Neurons[0][i] = Inputs[i];

            for (int i = 1; i < Layers.Length; i++)
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    float Bias = 0;

                    for (int k = 0; k < Neurons[i - 1].Length; k++)
                        Bias += Weights[i - 1][j][k] * Neurons[i - 1][k];

                    Neurons[i][j] = (float)Math.Tanh(Bias);
                }

            return Neurons[Neurons.Length - 1];
        }

        public void Mutate()
        {
            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    for (int k = 0; k < Weights[i][j].Length; k++)
                    {
                        float Weight = Weights[i][j][k];

                        float R = (float)Random.NextDouble() * 1000f;

                        if (R < 4f)
                        {
                            Weight *= -1f;
                        }
                        else if (R < 8f)
                        {
                            Weight = (float)Random.NextDouble() - 0.5f;
                        }
                        else if (R < 12f)
                        {
                            Weight *= (float)Random.NextDouble() + 1f;
                        }
                        else if (R < 16f)
                        {
                            Weight *= (float)Random.NextDouble();
                        }

                        Weights[i][j][k] = Weight;
                    }
        }
    }
}