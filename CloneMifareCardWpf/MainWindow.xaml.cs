using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CloneMifareCardWpf
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

        public String NfcCall(string nfcrun, string Args, object sender)
        {
            string fileName = Environment.CurrentDirectory + "/Resources/" + nfcrun;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                //(sender as Button).IsEnabled = false;
                controlContainer.IsEnabled = false;
                textBox_log.AppendText($"{fileName} {Args}\n");
            }));
            Process process = new Process
            {
                StartInfo ={
                    CreateNoWindow=true,
                    UseShellExecute=false,
                    RedirectStandardOutput=true,
                    FileName=fileName,
                    Arguments=Args
                }
            };
            process.Start();
            string output = "";
            while (!process.HasExited)
            {
                output = process.StandardOutput.ReadToEnd();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    textBox_log.AppendText(output + "\n");
                }));
            }
            process.WaitForExit();
            //Log.WriteLog(output);
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                //(sender as Button).IsEnabled = true;
                controlContainer.IsEnabled = true;
            }));
            return output;
        }

        public void NfcCallAsync(string nfcrun, string Args, object sender)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(() => NfcCall(nfcrun, Args, sender));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start();
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
                textBox_log.AppendText(objException.Message + "\n");
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
                textBox_log.AppendText(objException.Message + "\n");
            }
            catch (Exception objException)
            {
                // Log the exception
                textBox_log.AppendText(objException.Message + "\n");
            }
        }

        private void button_checkDevice_Click(object sender, RoutedEventArgs e)
        {
            NfcCallAsync("nfc-list", "", sender);
        }

        private void button_read_Click(object sender, RoutedEventArgs e)
        {
            NfcCallAsync("nfc-mfclassic", $"r a Resources/mfd/{textBox_readFileName.Text} Resources/mfd/ive_key.mfd f", sender);
        }

        private void button_write_Click(object sender, RoutedEventArgs e)
        {
            NfcCallAsync("nfc-mfclassic", $"W a Resources/mfd/{textBox_writeFileName.Text} Resources/mfd/ive_key.mfd f", sender);
        }
    }

    public class ScrollingTextBox : TextBox
    {

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            CaretIndex = Text.Length;
            ScrollToEnd();
        }

    }
}
