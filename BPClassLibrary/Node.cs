using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BPClassLibrary
{
    //public class Link
    //{
    //    public Node PreviousNode { get; set; }

    //    public Node NextNode { get; set; }

    //    public double Weight { get; set; }
    //}

    public class Node
    {
        public int Index { get; set; }

        public List<Node> PreviousNodes { get; set; }

        public List<double> PreviousWeights { get; set; }

        //public List<Node> NextNodes { get; set; }

        //public List<double> NextWeights { get; set; }

        public double Rate { get; set; } = 0.1;

        /// <summary>
        /// 阈值
        /// </summary>
        public double Theta { get; set; }

        public double Value { get; set; }

        public double TargetValue { get; set; }

        public double Loss => TargetValue - Value;

        //public Func<double, double> Function;

        public Activations Activations { get; set; } = Activations.Sigmoid;

        public void ForwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double x = 0;
                foreach (Node node in PreviousNodes)
                {
                    x += node.Value * PreviousWeights[node.Index];
                }
                x += Theta;
                switch (Activations)
                {
                    case Activations.Sigmoid:
                        x = BPMath.Sigmoid(x);
                        break;

                    case Activations.Tanh:
                        x = BPMath.Tanh(x);
                        break;

                    case Activations.ReLU:
                        x = BPMath.ReLU(x);
                        break;

                    case Activations.None:
                    default:
                        break;
                }
                Value = x;
            }
        }

        public void BackwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double totalWeight = 0;
                double loss = TargetValue - Value;
                switch (Activations)
                {
                    case Activations.None:

                        // Y1 = V11*X1+V21*X2
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əV11/əY1 = X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                            totalWeight += PreviousWeights[node.Index];
                        }
                        foreach (Node node in PreviousNodes)
                        {
                            if (node?.PreviousNodes?.Count > 0)
                            {
                                node.Value = node.Value + (PreviousWeights[node.Index] / totalWeight) * loss;
                            }
                        }
                        break;

                    case Activations.Sigmoid:
                        // Y1 = σ(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əV11/əY1 = Y1*(1-Y1)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*Y1*(1-Y1)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                            totalWeight += PreviousWeights[node.Index];
                        }
                        foreach (Node node in PreviousNodes)
                        {
                            if (node?.PreviousNodes?.Count > 0)
                            {
                                node.Value = node.Value + (PreviousWeights[node.Index] / totalWeight) * loss;
                            }
                        }
                        break;

                    case Activations.Tanh:
                        break;

                    case Activations.ReLU:
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public class BPFactory
    {
        public int MaxCount { get; set; } = 1000;

        public double Tolerence { get; set; } = 0.1;

        public List<Node> InputNodes { get; set; }

        public List<List<Node>> NodeListList { get; set; }

        public List<Node> OutputNodes { get; set; }

        public BPFactory(double[] inputs, int rows, int cols, int outputsCount, params double[] targetValues)
        {
            InputNodes = new List<Node>();
            for (int i = 0; i < inputs.Length; i++)
            {
                Node node = new Node();
                node.Value = inputs[i];
                node.Index = i;
                InputNodes.Add(node);
                Console.WriteLine($"InputNode{i}:{node.Value}");
            }

            NodeListList = new List<List<Node>>();
            List<Node> previousNodes = InputNodes;
            for (int i = 0; i < cols; i++)
            {
                var column = new List<Node>();
                for (int j = 0; j < rows; j++)
                {
                    Node node = new Node();
                    node.Index = j;
                    node.PreviousNodes = previousNodes;
                    node.PreviousWeights = new List<double>();
                    for (int k = 0; k < previousNodes.Count; k++)
                    {
                        node.PreviousWeights.Add((k + 1) * 0.1);
                    }
                    Console.WriteLine($"Node{i}:{node.Value}");
                    column.Add(node);
                }
                NodeListList.Add(column);
                previousNodes = column;
            }

            OutputNodes = new List<Node>();
            for (int i = 0; i < outputsCount; i++)
            {
                Node node = new Node();
                node.Index = i;
                node.PreviousNodes = previousNodes;
                node.PreviousWeights = new List<double>();
                for (int k = 0; k < previousNodes.Count; k++)
                {
                    node.PreviousWeights.Add((k + 1) * 0.1);
                }
                if (targetValues.Length > i)
                {
                    node.TargetValue = targetValues[i];
                }
                Console.WriteLine($"OutputNode{i}:{node.TargetValue}");
                OutputNodes.Add(node);
            }
        }

        public string ForwardPropagation()
        {
            string log = string.Empty;

            NodeListList.ForEach(list => list.ForEach(node => node.ForwardPropagation()));

            OutputNodes.ForEach(node => node.ForwardPropagation());

            for (int i = 0; i < OutputNodes.Count; i++)
            {
                Console.WriteLine($"OutputNode{i}:{OutputNodes[i].Value}");
                log += $"OutputNode{i}:{OutputNodes[i].Value}\r\n";
            }
            return log;
        }

        public void BackwardPropagation()
        {
            string log = string.Empty;

            OutputNodes.ForEach(node => node.BackwardPropagation());

            NodeListList.ForEach(list => list.ForEach(node => node.BackwardPropagation()));
        }

        public string Learn()
        {
            string log = string.Empty;
            int count = 0;
            while (true)
            {
                log += ForwardPropagation();
                BackwardPropagation();
                bool flag = OutputNodes.All(e => Math.Abs(e.Loss) <= Tolerence);
                count++;
                if (count > MaxCount || flag)
                {
                    Console.WriteLine($"count of learn:{count}");
                    log += $"count of learn:{count}\r\n";
                    break;
                }
            }
            return log;
        }

        public string Work()
        {
            string log = string.Empty;
            log += ForwardPropagation();
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                Console.WriteLine($"OutputNode{i}:{OutputNodes[i].Value}");
                log += $"OutputNode{i}:{OutputNodes[i].Value}\r\n";
            }
            return log;
        }
    }
}