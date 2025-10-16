using System;

namespace SimpleNeuralNet
{
    class NeuralNetwork
    {
        double[,] weightsInputHidden;
        double[,] weightsHiddenOutput;
        double[] hiddenBias;
        double[] outputBias;

        double learningRate = 0.5;
        Random rand = new Random();

        public NeuralNetwork(int inputSize, int hiddenSize, int outputSize)
        {
            weightsInputHidden = new double[inputSize, hiddenSize];
            weightsHiddenOutput = new double[hiddenSize, outputSize];
            hiddenBias = new double[hiddenSize];
            outputBias = new double[outputSize];

            InitWeights(weightsInputHidden);
            InitWeights(weightsHiddenOutput);
            InitWeights(hiddenBias);
            InitWeights(outputBias);
        }

        void InitWeights(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    matrix[i, j] = rand.NextDouble() * 2 - 1; // random -1 to 1
        }

        void InitWeights(double[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] = rand.NextDouble() * 2 - 1;
        }

        double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        double SigmoidDerivative(double x) => x * (1 - x);

        public double[] FeedForward(double[] inputs)
        {
            double[] hidden = new double[hiddenBias.Length];
            double[] outputs = new double[outputBias.Length];

            // Input → Hidden
            for (int j = 0; j < hidden.Length; j++)
            {
                double sum = hiddenBias[j];
                for (int i = 0; i < inputs.Length; i++)
                    sum += inputs[i] * weightsInputHidden[i, j];
                hidden[j] = Sigmoid(sum);
            }

            // Hidden → Output
            for (int k = 0; k < outputs.Length; k++)
            {
                double sum = outputBias[k];
                for (int j = 0; j < hidden.Length; j++)
                    sum += hidden[j] * weightsHiddenOutput[j, k];
                outputs[k] = Sigmoid(sum);
            }

            return outputs;
        }

        public void Train(double[] inputs, double[] targets)
        {
            // Forward pass
            double[] hidden = new double[hiddenBias.Length];
            double[] outputs = new double[outputBias.Length];

            for (int j = 0; j < hidden.Length; j++)
            {
                double sum = hiddenBias[j];
                for (int i = 0; i < inputs.Length; i++)
                    sum += inputs[i] * weightsInputHidden[i, j];
                hidden[j] = Sigmoid(sum);
            }

            for (int k = 0; k < outputs.Length; k++)
            {
                double sum = outputBias[k];
                for (int j = 0; j < hidden.Length; j++)
                    sum += hidden[j] * weightsHiddenOutput[j, k];
                outputs[k] = Sigmoid(sum);
            }

            // Calculate output errors
            double[] outputErrors = new double[outputs.Length];
            for (int k = 0; k < outputs.Length; k++)
                outputErrors[k] = targets[k] - outputs[k];

            // Backpropagate output → hidden
            for (int k = 0; k < outputs.Length; k++)
            {
                double delta = outputErrors[k] * SigmoidDerivative(outputs[k]);
                outputBias[k] += delta * learningRate;

                for (int j = 0; j < hidden.Length; j++)
                    weightsHiddenOutput[j, k] += hidden[j] * delta * learningRate;
            }

            // Calculate hidden layer errors
            double[] hiddenErrors = new double[hidden.Length];
            for (int j = 0; j < hidden.Length; j++)
            {
                double error = 0;
                for (int k = 0; k < outputs.Length; k++)
                    error += outputErrors[k] * weightsHiddenOutput[j, k];
                hiddenErrors[j] = error;
            }

            // Backpropagate hidden → input
            for (int j = 0; j < hidden.Length; j++)
            {
                double delta = hiddenErrors[j] * SigmoidDerivative(hidden[j]);
                hiddenBias[j] += delta * learningRate;

                for (int i = 0; i < inputs.Length; i++)
                    weightsInputHidden[i, j] += inputs[i] * delta * learningRate;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var nn = new NeuralNetwork(2, 2, 1);

            double[][] inputs = {
                new double[] {0, 0},
                new double[] {0, 1},
                new double[] {1, 0},
                new double[] {1, 1}
            };

            double[][] targets = {
                new double[] {0},
                new double[] {1},
                new double[] {1},
                new double[] {0}
            };

            // Train
            for (int epoch = 0; epoch < 10000; epoch++)
            {
                for (int i = 0; i < inputs.Length; i++)
                    nn.Train(inputs[i], targets[i]);
            }

            // Test
            Console.WriteLine("Testing XOR:");
            foreach (var input in inputs)
            {
                var output = nn.FeedForward(input);
                Console.WriteLine($"{input[0]} XOR {input[1]} = {Math.Round(output[0], 3)}");
            }
        }
    }
}
