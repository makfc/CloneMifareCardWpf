using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

            UpdateListView();
        }
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = ((Binding)headerClicked.Column.DisplayMemberBinding).Path.Path;
                    Sort(header, direction);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(listView_card.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        public static readonly string mfdDirectory = Environment.CurrentDirectory + "/Resources/mfd/";

        public void UpdateListView()
        {
            DirectoryInfo info = new DirectoryInfo(mfdDirectory);
            FileInfo[] mfdFiles = info.GetFiles().Where(p => p.Name != "ive_key.mfd").OrderByDescending(p => p.LastWriteTime).ToArray();

            List<Card> items = new List<Card>();

            foreach (FileInfo file in mfdFiles)
            {
                string fileName = file.Name;
                BinaryReader reader = new BinaryReader(new FileStream(mfdDirectory + fileName, FileMode.Open, FileAccess.Read, FileShare.None));

                Card card = new Card();
                card.FileName = fileName;
                card.LastWriteTime = file.LastWriteTime;

                reader.BaseStream.Position = 0x0;
                card.UID = BitConverter.ToString(reader.ReadBytes(4)).Replace("-", "");

                reader.BaseStream.Position = 0x200;
                card.CampusId = Encoding.Default.GetString(reader.ReadBytes(2));

                reader.BaseStream.Position = 0x202;
                card.StudentId = Encoding.Default.GetString(reader.ReadBytes(9));

                reader.BaseStream.Position = 0x20E;
                card.ExpiryDate = Encoding.Default.GetString(reader.ReadBytes(8));
                reader.Close();

                items.Add(card);
            }
            listView_card.ItemsSource = items;
            //listView_card.SelectedIndex = 0;
        }

        public String NfcCall(string nfcrun, string Args, object sender)
        {
            string output = "";
            try
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
                while (!process.HasExited)
                {
                    output = process.StandardOutput.ReadToEnd();
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        textBox_log.AppendText(output + "\n");
                        UpdateListView();
                    }));
                }
                process.WaitForExit();
                //Log.WriteLog(output);
            }
            catch (Exception ex)
            {
                // Log the exception
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    textBox_log.AppendText(ex.Message + "\n");
                }));
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                //(sender as Button).IsEnabled = true;
                controlContainer.IsEnabled = true;
                listView_card.Focus();
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

        private void listView_card_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                textBox_writeFileName.Text = (listView_card.SelectedItem as Card).FileName;
            }
            catch { }
        }

        private void button_openFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(mfdDirectory);
        }

        private void Button_edit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Card card = button.DataContext as Card;
            EditMfdFileWindow newWindow = new EditMfdFileWindow(card);
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.DataContext = this;
            newWindow.ShowDialog();
            UpdateListView();
        }

        private void Button_delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Card card = button.DataContext as Card;
            // Delete a file by using File class static method...
            if (File.Exists(mfdDirectory + card.FileName))
            {
                // Use a try block to catch IOExceptions, to
                // handle the case of the file already being
                // opened by another process.
                try
                {
                    File.Delete(mfdDirectory + card.FileName);
                }
                catch (IOException ex)
                {
                    textBox_log.AppendText(ex.Message + "\n");
                    return;
                }
            }
            UpdateListView();
        }
    }

    public class Card
    {
        public string FileName { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string UID { get; set; }
        public string CampusId { get; set; }
        public string StudentId { get; set; }
        public string ExpiryDate { get; set; }
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
