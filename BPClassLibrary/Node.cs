using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace BPClassLibrary
{
    public class Node
    {
        /// <summary>
        /// 节点索引值
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 前节点
        /// </summary>
        [JsonIgnore]
        public List<Node> PreviousNodes { get; set; }

        /// <summary>
        /// 前权重
        /// </summary>
        public List<double> PreviousWeights { get; set; }

        //public List<Node> NextNodes { get; set; }

        //public List<double> NextWeights { get; set; }

        /// <summary>
        /// 后一级的导数
        /// </summary>
        [JsonIgnore]
        public double Deta { get; set; } = 1;

        /// <summary>
        /// 学习率
        /// </summary>
        [JsonIgnore]
        public double Rate { get; set; } = 0.025;

        /// <summary>
        /// 阈值
        /// </summary>
        public double Theta { get; set; } = 0;

        /// <summary>
        /// 节点值
        /// </summary>
        [JsonIgnore]
        public double Value { get; set; }

        /// <summary>
        /// 目标节点值
        /// </summary>
        [JsonIgnore]
        public double TargetValue { get; set; }

        /// <summary>
        /// 误差值
        /// </summary>
        [JsonIgnore]
        public double Error { get; set; }

        /// <summary>
        /// 激活函数
        /// </summary>
        [JsonIgnore]
        public Activations Activations { get; set; } = Activations.Sigmoid;

        /// <summary>
        /// 前向传播
        /// </summary>
        public void ForwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double x = 0;
                foreach (Node node in PreviousNodes)
                {
                    x += (node.Value * PreviousWeights[node.Index]);
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

                    case Activations.PureLin:
                    default:
                        break;
                }
                Value = x;
                Console.WriteLine($"Node{Index}:{Value}");
            }
        }

        /// <summary>
        /// 反向传播
        /// </summary>
        public void BackwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double dθ = 0;
                double loss = TargetValue - Value;

                // 权重、阈值反向传播修正
                switch (Activations)
                {
                    case Activations.PureLin:
                        //PureLin(X)'=(X)'=1

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 1
                        // əδ/əθ = 2*(Y-Y1)*(-1)*1
                        // θ = θ-η*əδ/əθ

                        dθ = 2 * (TargetValue - Value) * (-1) * 1;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Sigmoid:
                        // Sigmoid(X)'=(1/(1+e^-x))'=[1/(1+e^-x)]*[1-1/(1+e^-x)]

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = Y1*(1-Y1)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*Y1*(1-Y1)*X1
                        // V11 = V11-η*əδ/əV11
                        var d = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) * node.Value;
                            double dw = d * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = Y1*(1-Y1)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*Y1*(1-Y1)
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        dθ = d;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Tanh:
                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = (1-Y1^2)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*(1-Y1^2)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = (1-Y1^2)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*(1-Y1^2)
                        // θ = θ-η*əδ/əθ
                        dθ = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value);
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.ReLU:
                        // ReLU(X)' = Max(0, x)'= 0(X<=0), 1(X>0)

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = 0(Y1<=0), X1(Y1>0)
                        // əδ/əV11 = 0(Y1<=0), 2*(Y-Y1)*(-1)*X1(Y1>0)
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 0;
                            if (Value > 0)
                            {
                                dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            }
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 0(Y1<=0), 1(Y1>0)
                        // əδ/əθ = 0(Y1<=0), 2*(Y-Y1)*(-1)*1(Y1>0)
                        // θ = θ-η*əδ/əθ
                        if (Value > 0)
                        {
                            dθ = 2 * (TargetValue - Value) * (-1);
                            Theta = Theta - Rate * dθ;
                        }
                        break;

                    default:
                        break;
                }

                // 误差反向传播分配
                double totalWeight = PreviousWeights.Sum();
                foreach (Node node in PreviousNodes)
                {
                    if (node?.PreviousNodes?.Count > 0)
                    {
                        node.TargetValue = node.TargetValue + (PreviousWeights[node.Index] / totalWeight) * loss;
                    }
                }
            }
        }


        public void NodeListListBackwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double dθ = 0;
                double loss = TargetValue - Value;

                // 权重、阈值反向传播修正
                switch (Activations)
                {
                    case Activations.PureLin:
                        //PureLin(X)'=(X)'=1

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*X1
                        // V11 = V11-η*əδ/əV11

                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            node.Deta = Deta * PreviousWeights[node.Index];
                            double dw = Deta * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 1
                        // əδ/əθ = 2*(Y-Y1)*(-1)*1
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * 1;
                        dθ = Deta;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Sigmoid:
                        // Sigmoid(X)'=(1/(1+e^-x))'=[1/(1+e^-x)]*[1-1/(1+e^-x)]

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = Y1*(1-Y1)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*Y1*(1-Y1)*X1
                        // V11 = V11-η*əδ/əV11
                        //var d = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        var d = Deta * Value * (1 - Value);
                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) * node.Value;
                            node.Deta = d * PreviousWeights[node.Index];
                            double dw = d * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = Y1*(1-Y1)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*Y1*(1-Y1)
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        dθ = d;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Tanh:
                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = (1-Y1^2)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*(1-Y1^2)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = (1-Y1^2)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*(1-Y1^2)
                        // θ = θ-η*əδ/əθ
                        dθ = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value);
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.ReLU:
                        // ReLU(X)' = Max(0, x)'= 0(X<=0), 1(X>0)

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = 0(Y1<=0), X1(Y1>0)
                        // əδ/əV11 = 0(Y1<=0), 2*(Y-Y1)*(-1)*X1(Y1>0)
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 0;
                            if (Value > 0)
                            {
                                dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            }
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 0(Y1<=0), 1(Y1>0)
                        // əδ/əθ = 0(Y1<=0), 2*(Y-Y1)*(-1)*1(Y1>0)
                        // θ = θ-η*əδ/əθ
                        if (Value > 0)
                        {
                            dθ = 2 * (TargetValue - Value) * (-1);
                            Theta = Theta - Rate * dθ;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void OutPutBackwardPropagation()
        {
            if (PreviousNodes?.Count > 0)
            {
                double dθ = 0;
                double loss = TargetValue - Value;

                // 权重、阈值反向传播修正
                switch (Activations)
                {
                    case Activations.PureLin:
                        //PureLin(X)'=(X)'=1

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*X1
                        // V11 = V11-η*əδ/əV11

                        var d1 = 2 * (TargetValue - Value) * (-1) ;
                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            node.Deta = d1 * PreviousWeights[node.Index];
                            double dw = d1 * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 1
                        // əδ/əθ = 2*(Y-Y1)*(-1)*1
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * 1;
                        dθ = d1;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Sigmoid:
                        // Sigmoid(X)'=(1/(1+e^-x))'=[1/(1+e^-x)]*[1-1/(1+e^-x)]

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = Y1*(1-Y1)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*Y1*(1-Y1)*X1
                        // V11 = V11-η*əδ/əV11
                        var d = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) / 10;
                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) * node.Value;
                            node.Deta = d * PreviousWeights[node.Index];
                            double dw = d * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = Y1*(1-Y1)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*Y1*(1-Y1)
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        dθ = d;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Tanh:
                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = (1-Y1^2)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*(1-Y1^2)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = (1-Y1^2)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*(1-Y1^2)
                        // θ = θ-η*əδ/əθ
                        dθ = 2 * (TargetValue - Value) * (-1) * (1 - Value * Value);
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.ReLU:
                        // ReLU(X)' = Max(0, x)'= 0(X<=0), 1(X>0)

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = 0(Y1<=0), X1(Y1>0)
                        // əδ/əV11 = 0(Y1<=0), 2*(Y-Y1)*(-1)*X1(Y1>0)
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 0;
                            if (Value > 0)
                            {
                                dw = 2 * (TargetValue - Value) * (-1) * node.Value;
                            }
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 0(Y1<=0), 1(Y1>0)
                        // əδ/əθ = 0(Y1<=0), 2*(Y-Y1)*(-1)*1(Y1>0)
                        // θ = θ-η*əδ/əθ
                        if (Value > 0)
                        {
                            dθ = 2 * (TargetValue - Value) * (-1);
                            Theta = Theta - Rate * dθ;
                        }
                        break;

                    default:
                        break;
                }

                //// 误差反向传播分配
                //double totalWeight = PreviousWeights.Sum();
                //foreach (Node node in PreviousNodes)
                //{
                //    if (node?.PreviousNodes?.Count > 0)
                //    {
                //        node.TargetValue = node.TargetValue + (PreviousWeights[node.Index] / totalWeight) * loss;
                //    }
                //}
            }
        }

        #region 误差传播算法
        public void ComputeError()
        {
            Error = TargetValue - Value;
        }

        public void BackwardError()
        {
            if (PreviousNodes?.Count > 0)
            {
                // 误差反向传播分配              
                foreach (Node node in PreviousNodes)
                {
                    if (node?.PreviousNodes?.Count > 0)
                    {
                        node.Error = node.Error + PreviousWeights[node.Index] * Error;
                    }
                }
            }
        }

        public void BackwardWeight()
        {
            if (PreviousNodes?.Count > 0)
            {
                double dθ = 0;

                // 权重、阈值反向传播修正
                switch (Activations)
                {
                    case Activations.PureLin:
                        //PureLin(X)'=(X)'=1

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * Error * (-1) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = PureLin(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 1
                        // əδ/əθ = 2*(Y-Y1)*(-1)*1
                        // θ = θ-η*əδ/əθ

                        dθ = 2 * Error * (-1) * 1;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Sigmoid:
                        // Sigmoid(X)'=(1/(1+e^-x))'=[1/(1+e^-x)]*[1-1/(1+e^-x)]

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = Y1*(1-Y1)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*Y1*(1-Y1)*X1
                        // V11 = V11-η*əδ/əV11
                        var d = 2 * Error * (-1) * Value * (1 - Value);
                        foreach (Node node in PreviousNodes)
                        {
                            //double dw = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value) * node.Value;
                            double dw = d * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Sigmoid(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = Y1*(1-Y1)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*Y1*(1-Y1)
                        // θ = θ-η*əδ/əθ

                        //dθ = 2 * (TargetValue - Value) * (-1) * Value * (1 - Value);
                        dθ = d;
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.Tanh:
                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = (1-Y1^2)*X1
                        // əδ/əV11 = 2*(Y-Y1)*(-1)*(1-Y1^2)*X1
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 2 * Error * (-1) * (1 - Value * Value) * node.Value;
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = Tanh(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = (1-Y1^2)
                        // əδ/əθ = 2*(Y-Y1)*(-1)*(1-Y1^2)
                        // θ = θ-η*əδ/əθ
                        dθ = 2 * Error * (-1) * (1 - Value * Value);
                        Theta = Theta - Rate * dθ;
                        break;

                    case Activations.ReLU:
                        // ReLU(X)' = Max(0, x)'= 0(X<=0), 1(X>0)

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əV11 = 0(Y1<=0), X1(Y1>0)
                        // əδ/əV11 = 0(Y1<=0), 2*(Y-Y1)*(-1)*X1(Y1>0)
                        // V11 = V11-η*əδ/əV11
                        foreach (Node node in PreviousNodes)
                        {
                            double dw = 0;
                            if (Value > 0)
                            {
                                dw = 2 * Error * (-1) * node.Value;
                            }
                            PreviousWeights[node.Index] = PreviousWeights[node.Index] - Rate * dw;
                        }

                        // Y1 = ReLU(V11*X1+V21*X2+θ)
                        // δ = (Y-Y1)^2
                        // əδ/əY1 = 2*(Y-Y1)*(-1)
                        // əY1/əθ = 0(Y1<=0), 1(Y1>0)
                        // əδ/əθ = 0(Y1<=0), 2*(Y-Y1)*(-1)*1(Y1>0)
                        // θ = θ-η*əδ/əθ
                        if (Value > 0)
                        {
                            dθ = 2 * Error * (-1);
                            Theta = Theta - Rate * dθ;
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        #endregion
    }

    public class BPFactory
    {
        /// <summary>
        /// 最大迭代次数
        /// </summary>
        public int MaxCount { get; set; } = 50;

        /// <summary>
        /// 目标容差
        /// </summary>
        public double Tolerence { get; set; } = 0.1;

        /// <summary>
        /// 输入节点
        /// </summary>
        [JsonIgnore]
        public List<Node> InputNodes { get; set; }

        /// <summary>
        /// 隐藏层神经元
        /// </summary>
        public List<List<Node>> NodeListList { get; set; }

        /// <summary>
        /// 输出节点
        /// </summary>
        public List<Node> OutputNodes { get; set; }

        public void Link(int inputsCount)
        {
            InputNodes = new List<Node>();
            for (int i = 0; i < inputsCount; i++)
            {
                Node node = new Node();
                node.Index = i;
                InputNodes.Add(node);
            }

            List<Node> previousNodes = InputNodes;
            foreach (var nodeList in NodeListList)
            {
                foreach (var node in nodeList)
                {
                    node.PreviousNodes = previousNodes;
                }
                previousNodes = nodeList;
            }
            foreach (var node in OutputNodes)
            {
                node.PreviousNodes = previousNodes;
            }
        }

        public BPFactory(int inputsCount, double[] inputs, int rows, int cols, int outputsCount, double[] targetValues, bool test = false)
        {
            Random random = new Random();

            InputNodes = new List<Node>();
            for (int i = 0; i < inputsCount; i++)
            {
                Node node = new Node();
                if (inputs?.Length > i)
                {
                    node.Value = inputs[i];
                }
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
                        if (!test)
                        {
                            var r = random.NextDouble();
                            var n = (2 * r - 1) / Math.Sqrt(previousNodes.Count);
                            node.PreviousWeights.Add(n);
                        }
                        else
                        {
                            node.PreviousWeights.Add(0.1);
                        }
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
                    if (!test)
                    {
                        var r = random.NextDouble();
                        var n = (2 * r - 1) / Math.Sqrt(previousNodes.Count);
                        node.PreviousWeights.Add(n);
                    }
                    else
                    {
                        node.PreviousWeights.Add(0.1);
                    }
                }
                if (targetValues?.Length > i)
                {
                    node.TargetValue = targetValues[i];
                }
                Console.WriteLine($"Output{i}:{node.TargetValue}");
                OutputNodes.Add(node);
            }
        }

        public BPFactory()
        {
        }

        public void SetInputNodes(double[] inputs)
        {
            for (int i = 0; i < InputNodes.Count; i++)
            {
                Node node = InputNodes[i];
                if (inputs?.Length > i)
                {
                    node.Value = inputs[i];
                }
            }
        }

        public void SetOutputNodes(double[] targetValues)
        {
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                Node node = OutputNodes[i];
                if (targetValues?.Length > i)
                {
                    node.TargetValue = targetValues[i];
                }
            }
        }

        public string ForwardPropagation()
        {
            string log = string.Empty;

            NodeListList.ForEach(list => list.ForEach(node => node.ForwardPropagation()));

            OutputNodes.ForEach(node => node.ForwardPropagation());

            //for (int i = 0; i < OutputNodes.Count; i++)
            //{
            //    //Console.WriteLine($"Output{i}:{OutputNodes[i].Value}");
            //    log += $"{i}: {OutputNodes[i].Value}\r\n";
            //}
            //log += $"\r\n";
            return log;
        }

        public void BackwardPropagation()
        {
            string log = string.Empty;

            OutputNodes.ForEach(node => node.OutPutBackwardPropagation());

            //从后往前
            for (int i = NodeListList.Count - 1; i > -1; i--)
            {
                var nodeList = NodeListList[i];
                for (int j = 0; j < nodeList.Count; j++)
                {
                    var node = nodeList[j];
                    node.NodeListListBackwardPropagation();
                }
            }
        }

        #region 误差传播算法
        public void BackwardError()
        {
            string log = string.Empty;
            foreach (var node in OutputNodes)
            {
                node.ComputeError();
            }
            foreach (var nodeList in NodeListList)
            {
                foreach (var node in nodeList)
                {
                    node.Error = 0;
                }
            }

            OutputNodes.ForEach(node => node.BackwardError());

            //从后往前
            for (int i = NodeListList.Count - 1; i > -1; i--)
            {
                var nodeList = NodeListList[i];
                for (int j = 0; j < nodeList.Count; j++)
                {
                    var node = nodeList[j];
                    node.BackwardError();
                }
            }
        }

        public void BackwardWeight()
        {
            OutputNodes.ForEach(node => node.BackwardWeight());
            for (int i = NodeListList.Count - 1; i > -1; i--)
            {
                var nodeList = NodeListList[i];
                for (int j = 0; j < nodeList.Count; j++)
                {
                    var node = nodeList[j];
                    node.BackwardWeight();
                }
            }
        }

        public string Learn1()
        {
            string log = string.Empty;
            int count = 0;
            ForwardPropagation();
            while (true)
            {
                BackwardError();
                BackwardWeight();
                bool flag = OutputNodes.All(e => Math.Abs(e.TargetValue - e.Value) <= Tolerence);
                count++;
                if (count > MaxCount || flag)
                {
                    //Console.WriteLine($"计算迭代次数:{count}");                    
                    break;
                }
            }
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                //Console.WriteLine($"Output{i}:{OutputNodes[i].Value}");
                log += $"{i}: {OutputNodes[i].Value}\r\n";
            }
            log += $"\r\n计算迭代次数:{count}\r\n";
            return log;
        }
        #endregion

        public string Learn()
        {
            string log = string.Empty;
            int count = 0;
            ForwardPropagation();
            while (true)
            {
                BackwardPropagation();
                ForwardPropagation();
                bool flag = OutputNodes.All(e => Math.Abs(e.TargetValue - e.Value) <= Tolerence);
                count++;
                if (count > 50 || flag)
                {
                    //Console.WriteLine($"计算迭代次数:{count}");                    
                    break;
                }
            }
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                //Console.WriteLine($"Output{i}:{OutputNodes[i].Value}");
                log += $"{i}: {OutputNodes[i].Value}\r\n";
            }
            log += $"\r\n计算迭代次数:{count}\r\n";
            return log;
        }

        public string Work()
        {
            string log = string.Empty;
            log += ForwardPropagation();
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                Console.WriteLine($"Output{i}:{OutputNodes[i].Value}");
                log += $"Output{i}:{OutputNodes[i].Value}\r\n";
            }
            return log;
        }
    }
}