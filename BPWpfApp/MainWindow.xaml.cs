using BPClassLibrary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BPWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
            tb_Input.Text = "";
        }

        private void Btn_Identify_Click(object sender, RoutedEventArgs e)
        {
            ConvertToMatrix();
            var input = matrix.ToArray().Select(e => (double)e).ToArray();
            var targetOutput = ToArray(1);
            BPFactory bPFactory = new BPFactory(input, 16, 2, 10, targetOutput);

            string log = bPFactory.Learn();
            log += bPFactory.Work();

            for (int i = 0; i < bPFactory.OutputNodes.Count; i++)
            {
                var node = bPFactory.OutputNodes[i];
                string s = $"{i}:{node.Value}\r\n";
                log += s;
            }
            tb_Output.Text = log;
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
                    str += (matrix[i, j] + "  ");
                }
                str += ("\r\n");
            }
            tb_Input.Text = str;
            //MessageBox.Show(str);
        }

        /// <summary>
        /// 概率
        /// </summary>
        /// <param name="n">0-9</param>
        /// <returns></returns>
        public double[] ToArray(int n)
        {
            double[] array = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            array[n] = 1;
            return array;
        }
    }
}