using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

namespace iic.gui
{
    public partial class CompWindow : Window
    {
        private string _path1, _path2;

        private void button_use1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void button_use2_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(_path1);
            Close();
        }

        public CompWindow(string path1, string path2, float diff)
        {
            InitializeComponent();

            var uriSource1 = new Uri(path1, UriKind.RelativeOrAbsolute);
            var uriSource2 = new Uri(path2, UriKind.RelativeOrAbsolute);
            BitmapImage bmpimg1 = new BitmapImage(uriSource1);
            BitmapImage bmpimg2 = new BitmapImage(uriSource2);

            string restxt1 = $"Dimensions: {bmpimg1.Width} x {bmpimg1.Height}";
            string restxt2 = $"Dimensions: {bmpimg2.Width} x {bmpimg2.Height}";

            txt_res1.Text = restxt1;
            txt_res2.Text = restxt2;

            img1.Source = bmpimg1;
            img2.Source = bmpimg2;

            _path1 = "Path:\n" + path1;
            _path2 = "Path:\n" + path2;

            txt_Dir1.Text = path1;
            txt_Dir2.Text = path2;

            txt_Diff.Text = "Similarity: " + (diff*100).ToString("F", CultureInfo.InvariantCulture)+"/100";
        }
    }
}
