using PB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PB
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            // 清除 InkCanvas 上的所有笔迹  
            inkCanvas.Strokes.Clear();
        }

        private void Btn_Identify_Click(object sender, RoutedEventArgs e)
        {
            //ConvertToMatrix();
            Compute();
        }

        private int[,] matrix = new int[28, 28];

        private void ConvertToMatrix()
        {
            // 清空矩阵  
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    matrix[i, j] = 0;
                }
            }

            // 遍历 InkCanvas 上的所有墨迹  
            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                foreach (StylusPoint point in stroke.StylusPoints)
                {
                    int x = (int)point.X / 10;
                    int y = (int)point.Y / 10;

                    if (x >= 0 && x < 28 && y >= 0 && y < 28)
                    {
                        // 将对应的矩阵元素设置为 1  
                        matrix[y, x] = 1;
                    }
                }
            }

            string str = string.Empty;
            // 打印矩阵  
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    str += (matrix[i, j] + "   ");
                }
                str += ("\r\n");
            }
            MessageBox.Show(str);
        }

        public void Compute()
        {
            double[,] x = new double[,] { { 1 }, { 2 } };
            int inputCount = x.GetLength(0);
            PBModel pBModel = new PBModel(x, 2, 2);
            pBModel.ComputeU();
            pBModel.ComputeY();
        }
    }
}
