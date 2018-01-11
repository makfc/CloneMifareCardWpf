using System;
using System.Collections.Generic;
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

namespace CloneMifareCardWpf
{
    /// <summary>
    /// Interaction logic for EditMfdFileWindow.xaml
    /// </summary>
    public partial class EditMfdFileWindow : Window
    {
        Card card;

        public EditMfdFileWindow(Card card)
        {
            InitializeComponent();

            this.card = card;
            textBox_FileName.Text = card.FileName;
            textBox_LastWriteTime.Text = card.LastWriteTime.ToString();
            textBox_UID.Text = card.UID;
            textBox_CampusId.Text = card.CampusId;
            textBox_StudentId.Text = card.StudentId;
            textBox_ExpiryDate.Text = card.ExpiryDate;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            byte[] uid = StringToByteArray(textBox_UID.Text);
            byte[] campusId = Encoding.ASCII.GetBytes(textBox_CampusId.Text);
            byte[] studentId = Encoding.ASCII.GetBytes(textBox_StudentId.Text);
            byte[] expiryDate = Encoding.ASCII.GetBytes(textBox_ExpiryDate.Text);

            try
            {
                File.Move(MainWindow.mfdDirectory + card.FileName, MainWindow.mfdDirectory + textBox_FileName.Text);
                using (var stream = new FileStream(MainWindow.mfdDirectory + textBox_FileName.Text, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Seek(0x0, SeekOrigin.Begin);
                    stream.Write(uid, 0, 4);

                    stream.Seek(0x200, SeekOrigin.Begin);
                    stream.Write(campusId, 0, 2);

                    stream.Seek(0x202, SeekOrigin.Begin);
                    stream.Write(studentId, 0, 9);

                    stream.Seek(0x20E, SeekOrigin.Begin);
                    stream.Write(expiryDate, 0, 8);
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
