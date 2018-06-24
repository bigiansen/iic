using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Linq;

using iic;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using iic.core;
using System.Globalization;
using System.Windows.Media;

namespace iic.gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txt_Threshold.SelectionBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        private void button_Compare_Click(object sender, RoutedEventArgs e)
        {
            var shownComparisons = new HashSet<Tuple<string, string>>(new ReverseTupleComparer());

            long total = BitmapComparer.GetPermutationCount();
            const int MAX_COUNT = 5000000;
            IDictionary<Tuple<string, string>, float> diffMap = null;
            int startIdx = 0;
        restart:
            GC.Collect();
            Action task = new Action(() =>
            {
                diffMap = BitmapComparer.CreateDiffMapFromDCTCache(MAX_COUNT, startIdx);
            });

            WorkWindow wwin = new WorkWindow($"Generating diff map...\n\n[{startIdx}] out of [{total}]", task);
            wwin.Owner = this;
            wwin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wwin.SetPBarScore(startIdx, total);
            wwin.ShowDialog();
            
            float threshold = float.Parse(txt_Threshold.Text, CultureInfo.InvariantCulture);

            var filteredDiffMap = diffMap.Where((kvp) => kvp.Value >= threshold);

            long idx = 0;
            
            foreach (var kvp in filteredDiffMap.Distinct())
            {
                string file1 = kvp.Key.Item1;
                string file2 = kvp.Key.Item2;

                var tuple = new Tuple<string, string>(file1, file2);

                if (shownComparisons.Contains(tuple))
                {
                    continue;
                }

                float diff = kvp.Value;

                CompWindow cwin = new CompWindow(file1, file2, diff);
                cwin.Owner = this;
                cwin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                cwin.ShowDialog();

                shownComparisons.Add(tuple);
                idx++;
            }
            if(diffMap.Count != 0)
            {
                startIdx += diffMap.Count;
                goto restart;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CompareDirectoryImages(string dir, WorkWindow wwin)
        {

        }

        private async void RunWorker(Action act)
        {
            Dispatcher.Invoke(() => IsEnabled = false);

            await Task.Run(act);

            Dispatcher.Invoke(() => IsEnabled = true);
        }

        private void button_Prehash(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a folder to generate the hashmap from.");
            FolderBrowserDialog diag = new FolderBrowserDialog();
            if(diag.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            string path = diag.SelectedPath;

            if (!Directory.Exists(path))
            {
                System.Windows.MessageBox.Show("INVALID PATH!");
                return;
            }

            WorkWindow win = new WorkWindow();
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.Show();
            win.SetSecondaryMessage(">> READY!");

            string text = path;

            System.Windows.MessageBox.Show("Select a file to save the hashmap to.");

            SaveFileDialog sfdiag = new SaveFileDialog();
            sfdiag.Title = "Serialized cache save location:";
            sfdiag.DefaultExt = ".hcd";
            sfdiag.AddExtension = true;
            sfdiag.Filter = "(Hashmap Cache Data File)|*.hcd";
            if (sfdiag.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            string fname = sfdiag.FileName;

            RunWorker(() =>
            {
                Prehash(text, win, fname);
                win.Dispatcher.Invoke(() => 
                {
                    win.SetPBarScore(100, 100);
                    win.SetSecondaryMessage(">> FINISHED!");
                    System.Windows.MessageBox.Show("Done!");
                    win.Close();
                });
            });            
        }

        private void Prehash(string dir, WorkWindow wwin, string fname)
        {       
            IEnumerable<string> files =
                Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                    .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"));

            ParallelOptions opts = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            int idx = 0;
            int total = files.Count();
            Parallel.ForEach(files, opts, (file) =>
            {
                wwin.SetPBarScore(idx, total);
                wwin.SetSecondaryMessage($"BUILDING CACHE...\n  [{idx}] out of [{files.Count()}]");
                BitmapComparer.CacheDiff(file);
                idx++;
            });

            BitmapComparer.SaveDiffCacheToDisk(fname);
        }

        private void button_RestoreCache(object sender, RoutedEventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            if(diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = diag.FileName;
                BitmapComparer.RestoreDiffCacheFromDisk(path);
                System.Windows.MessageBox.Show("DONE!");
            }            
        }
    }
}
