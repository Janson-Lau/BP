using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PB.Views
{
    /// <summary>
    /// InkCanvasView.xaml 的交互逻辑
    /// </summary>
    public partial class InkCanvasView : Window
    {
        public InkCanvasView()
        {
            InitializeComponent();

            foreach (InkCanvasEditingMode mode in Enum.GetValues(typeof(InkCanvasEditingMode)))
            {
                this.combMode.Items.Add(mode);
            }
            this.combMode.SelectedIndex = 1;
            this.combMode.SelectionChanged += CombMode_SelectionChanged;
        }

        private void CombMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.inkCanvas.EditingMode = (InkCanvasEditingMode)this.combMode.SelectedItem;
        }
    }
}
