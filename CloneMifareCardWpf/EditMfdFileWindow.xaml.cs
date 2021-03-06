﻿using System;
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
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte CalcBCC(byte[] uid)
        {
            if (uid.Length != 4)
                throw new Exception("UID length is not 4 bytes.");

            byte bcc = uid[0];
            for (int i = 1; i < uid.Length; i++)
                bcc = (byte)(bcc ^ uid[i]);

            return bcc;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] uid = StringToByteArray(textBox_UID.Text);
                byte bcc = CalcBCC(uid);
                byte[] campusId = Encoding.ASCII.GetBytes(textBox_CampusId.Text);
                byte[] studentId = Encoding.ASCII.GetBytes(textBox_StudentId.Text);
                byte[] expiryDate = Encoding.ASCII.GetBytes(textBox_ExpiryDate.Text);


                File.Move(MainWindow.mfdDirectory + card.FileName, MainWindow.mfdDirectory + textBox_FileName.Text);
                using (var stream = new FileStream(MainWindow.mfdDirectory + textBox_FileName.Text, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Seek(0x0, SeekOrigin.Begin);
                    stream.Write(uid, 0, 4);

                    stream.WriteByte(bcc);

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
