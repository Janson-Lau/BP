using BPClassLibrary;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Size = System.Windows.Size;

namespace BPWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static BPFactory bPFactory { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            if (!File.Exists(BPPath))
            {
                bPFactory = new BPFactory(28 * 28, null, 16, 2, 10, null);
            }
            else
            {
                bPFactory = JsonUtils.JsonToObject<BPFactory>(BPPath);
                bPFactory.Link(28 * 28);
            }
        }

        #region Common

        public string MatrixToString(double[,] matrix)
        {
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
            return str;
        }

        private double[,] ConvertToMatrix(InkCanvas inkCanvas)
        {
            var img = ConvertInkCanvasToBitmap(inkCanvas);
            return ConvertImageToMatrix(img);

            //double[,] matrix = new double[28, 28];
            //// 遍历 InkCanvas 上的所有墨迹
            //foreach (Stroke stroke in inkCanvas.Strokes)
            //{
            //    foreach (StylusPoint point in stroke.StylusPoints)
            //    {
            //        int x = (int)(Math.Round(point.X) / 10);
            //        int y = (int)(Math.Round(point.Y) / 10);

            //        if (x >= 0 && x < 28 && y >= 0 && y < 28)
            //        {
            //            // 将对应的矩阵元素设置为 1
            //            matrix[y, x] = 1;
            //        }
            //    }
            //}
            //return matrix;
        }

        public BitmapSource ConvertInkCanvasToBitmapSource(InkCanvas inkCanvas)
        {
            // 确保InkCanvas不为空
            if (inkCanvas == null) throw new ArgumentNullException("inkCanvas");

            // 创建一个RenderTargetBitmap对象，用于将InkCanvas的内容渲染成位图
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            // 渲染InkCanvas的内容到RenderTargetBitmap
            inkCanvas.Measure(new Size(inkCanvas.ActualWidth, inkCanvas.ActualHeight));
            inkCanvas.Arrange(new Rect(new Size(inkCanvas.ActualWidth, inkCanvas.ActualHeight)));
            renderTargetBitmap.Render(inkCanvas);

            // 创建一个DrawingVisual对象，用于绘制变换后的RenderTargetBitmap
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawImage(renderTargetBitmap, new Rect(0, 0, 28, 28));
                // 创建一个缩放矩阵
                //Matrix scaleMatrix = Matrix.Identity;
                //scaleMatrix.Scale(28.0 / inkCanvas.ActualWidth, 28.0 / inkCanvas.ActualHeight);

                // 应用缩放矩阵
                //context.PushTransform(new MatrixTransform(scaleMatrix));
                //context.Pop();
            }

            // 将DrawingVisual对象渲染到RenderTargetBitmap
            RenderTargetBitmap transformedBitmap = new RenderTargetBitmap(28, 28, 96, 96, PixelFormats.Pbgra32);
            transformedBitmap.Render(visual);

            img_Test.Source = transformedBitmap;

            return transformedBitmap;
        }

        public Bitmap ConvertInkCanvasToBitmap(InkCanvas inkCanvas)
        {
            var scaledBitmapSource = ConvertInkCanvasToBitmapSource(inkCanvas);
            Bitmap bitmap = BitmapSourceToBitmap(scaledBitmapSource);
            return bitmap;
        }

        private Bitmap ConvertInkCanvasToBitmap1(InkCanvas inkCanvas)
        {
            // 使用 RenderTargetBitmap 渲染 InkCanvas
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)inkCanvas.ActualWidth,
                (int)inkCanvas.ActualHeight,
                96, 96,
                System.Windows.Media.PixelFormats.Pbgra32);

            renderTargetBitmap.Render(inkCanvas);

            // 将 RenderTargetBitmap 缩放到 28x28 像素
            BitmapSource scaledBitmapSource = new TransformedBitmap(renderTargetBitmap,
                new ScaleTransform(28.0 / renderTargetBitmap.PixelWidth,
                                   28.0 / renderTargetBitmap.PixelHeight));

            // 将 BitmapSource 转换为 Bitmap
            Bitmap bitmap = BitmapSourceToBitmap(scaledBitmapSource);

            img_Test.Source = renderTargetBitmap;

            return bitmap;
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // 将 BitmapSource 编码并保存到 MemoryStream
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                // 返回 Bitmap
                return new Bitmap(stream);
            }
        }

        public double[,] ConvertImageToMatrix(string imagePath)
        {
            Bitmap bitmap = new Bitmap(imagePath);
            return ConvertImageToMatrix(bitmap);
        }

        public double[,] ConvertImageToMatrix(Bitmap bitmap)
        {
            // 创建一个 28x28 的矩阵
            double[,] matrix = new double[28, 28];

            // 加载图像
            using (bitmap)
            {
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        // 获取每个像素的颜色
                        System.Drawing.Color pixelColor = bitmap.GetPixel(j, i);

                        // 将黑色像素设为 1，白色像素设为 0（可根据需要调整阈值）
                        if (pixelColor.R < 128) // 假设是黑白图片
                        {
                            matrix[i, j] = 0;  // 黑色
                        }
                        else
                        {
                            matrix[i, j] = 1;  // 白色
                        }

                        //// 将颜色转换为灰度值
                        ////double grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0 / 255.0;
                        //int grayValue = pixelColor.R; // 灰度值，假设 R、G、B 相同
                        //// 将像素的颜色值映射到矩阵
                        //if (grayValue < 85)
                        //{
                        //    matrix[i, j] = 1;  // 黑色
                        //}
                        //else if (grayValue >= 85 && grayValue < 170)
                        //{
                        //    matrix[i, j] = 0.5;  // 灰色
                        //}
                        //else
                        //{
                        //    matrix[i, j] = 0;  // 白色
                        //}
                    }
                }
            }
            return matrix;
        }

        /// <summary>
        /// 概率
        /// </summary>
        /// <param name="n">0-9</param>
        /// <returns></returns>
        public double[] ToArray(int n)
        {
            double[] array = new double[] { 0.001, 0.001, 0.001, 0.001, 0.001, 0.001, 0.001, 0.001, 0.001, 0.001 };
            array[n] = 0.999;
            return array;
        }

        private BitmapImage SetImageSource(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath); // 指定图片路径
                bitmap.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片时发生错误: {ex.Message}");
            }
            return bitmap;
        }

        #endregion Common

        #region 训练

        private void Btn_Clear_Practise_Click(object sender, RoutedEventArgs e)
        {
            // 清除 InkCanvas 上的所有笔迹
            inkCanvas_Practise.Strokes.Clear();
            tb_Matrix_Practise.Text = "";
            tb_Log_Practise.Text = "";
            tb_TargetOutput.Text = "";
            img_Test.Source = null;
        }

        private void Btn_Identify_Practise_Click(object sender, RoutedEventArgs e)
        {
            var flag = int.TryParse(tb_TargetOutput.Text, out int n);
            if (flag && n >= 0 && n <= 9)
            {
                var matrix = ConvertToMatrix(inkCanvas_Practise);
                tb_Matrix_Practise.Text = MatrixToString(matrix);
                var input = matrix.ToArray().Select(e => e).ToArray();
                var targetOutput = ToArray(n);
                bPFactory.SetInputNodes(input);
                bPFactory.SetOutputNodes(targetOutput);
                string log = bPFactory.Learn();
                tb_Log_Practise.Text = log;
            }
            else
            {
                MessageBox.Show("请输入0-9");
            }
        }

        #endregion 训练

        #region 批量训练

        public static string CommonAssemblyLocation => Assembly.GetExecutingAssembly().Location;

        public static string AssemblyDirectory => System.IO.Path.GetDirectoryName(CommonAssemblyLocation);

        public static string ImagesDirectory => System.IO.Path.Combine(AssemblyDirectory, "images");

        public static string BPPath => System.IO.Path.Combine(AssemblyDirectory, "BP.json");

        public void Practise()
        {
            string[] imagesDirectories = Directory.GetDirectories(ImagesDirectory);
            List<(int N, string Path)> imageList = new List<(int, string)>();
            foreach (var imagesDirectory in imagesDirectories)
            {
                // 获取文件夹名称
                string folderName = System.IO.Path.GetFileName(imagesDirectory);
                bool flag = int.TryParse(folderName, out int n);
                if (flag && n >= 0 && n <= 9)
                {
                    string[] imgFiles = Directory.GetFiles(imagesDirectory, "*.jpg");
                    foreach (var img in imgFiles)
                    {
                        (int N, string Path) image = (n, img);
                        imageList.Add(image);
                    }
                }
                else
                {
                    MessageBox.Show("请输入0-9");
                }
            }

            var rnd = new Random();
            var randomizedList = imageList.OrderBy(x => rnd.NextDouble()).ToList();
            int count = 0;
            foreach (var item in randomizedList)
            {
                var matrix = ConvertImageToMatrix(item.Path);
                var input = matrix.ToArray().Select(e => e).ToArray();
                var targetOutput = ToArray(item.N);
                bPFactory.SetInputNodes(input);
                bPFactory.SetOutputNodes(targetOutput);

                string log = bPFactory.Learn();
                log += "\r\n训练次数" + count + "\r\n目标值：" + item.N;
                tb_Log_Practise_Auto.Dispatcher.Invoke(
                     new Action<System.Windows.DependencyProperty, object>(tb_Log_Practise_Auto.SetValue),
                     System.Windows.Threading.DispatcherPriority.Background,
                     System.Windows.Controls.TextBox.TextProperty, log);

                var matrixString = MatrixToString(matrix);
                tb_Matrix_Practise_Auto.Dispatcher.Invoke(
                   new Action<System.Windows.DependencyProperty, object>(tb_Matrix_Practise_Auto.SetValue),
                   System.Windows.Threading.DispatcherPriority.Background,
                   System.Windows.Controls.TextBlock.TextProperty, matrixString);

                var image = SetImageSource(item.Path);
                images_Practise_Auto.Dispatcher.Invoke(
                    new Action<System.Windows.DependencyProperty, object>(images_Practise_Auto.SetValue),
                    System.Windows.Threading.DispatcherPriority.Background,
                    System.Windows.Controls.Image.SourceProperty, image);

                var nStr = item.N.ToString();
                tb_TargetOutput_Auto.Dispatcher.Invoke(
                          new Action<System.Windows.DependencyProperty, object>(tb_TargetOutput_Auto.SetValue),
                          System.Windows.Threading.DispatcherPriority.Background,
                          System.Windows.Controls.TextBlock.TextProperty, nStr);

                count++;
                //count = randomizedList.IndexOf(item);                
                if (count > 50)
                {
                    break;
                }
            }
        }

        private void Btn_Identify_Practise_Auto_Click(object sender, RoutedEventArgs e)
        {
            Practise();
            JsonUtils.ObjectToJson(bPFactory, BPPath);
            MessageBox.Show("训练完成！");      
        }

        #endregion 批量训练

        #region 工作

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            // 清除 InkCanvas 上的所有笔迹
            inkCanvas.Strokes.Clear();
            tb_Matrix.Text = "";
            tb_Output.Text = "";
            tb_Log.Text = "";
        }

        private void Btn_Identify_Click(object sender, RoutedEventArgs e)
        {
            var matrix = ConvertToMatrix(inkCanvas);
            tb_Matrix.Text = MatrixToString(matrix);
            var input = matrix.ToArray().Select(e => e).ToArray();
            bPFactory.SetInputNodes(input);
            string log = bPFactory.Work();

            var outputNode = bPFactory.OutputNodes.OrderByDescending(e => e.Value).First();
            var index = bPFactory.OutputNodes.IndexOf(outputNode);
            tb_Output.Text = index.ToString();
            tb_Log.Text = log;
        }

        #endregion 工作
    }
}