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

namespace iic.gui
{
    /// <summary>
    /// Interaction logic for WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window
    {
        public WorkWindow()
        {
            InitializeComponent();
        }

        public WorkWindow(string message, Action task)
        {
            InitializeComponent();
            SetSecondaryMessage(message);
            RunTask(task);
        }

        private void RunTask(Action task)
        {
            Task.Run(() =>
            {
                task();
                Dispatcher.Invoke(() => Close());
            });
        }

        public void SetPBarScore(long value, long total)
        {
            Dispatcher.Invoke(() =>
            {
                pbar.IsIndeterminate = false;
                pbar.Maximum = total;
                pbar.Value = value;
            });
        }

        public void SetSecondaryMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                txt_Msg2.Text = msg;
            });
        }

        private void pbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
