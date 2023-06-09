using System;
using System.Windows;
using System.Windows.Input;

namespace MusicSearch
{
    public partial class PropertyNetwork : Window
    {
        private bool flag = false;
        private int countEpochs = 10;
        public PropertyNetwork()
        {
            InitializeComponent();
        }
        private void Process()
        {
            int countEpochs_buf;
            var epochs = Epochs.Text;
            bool epochs_flag = int.TryParse(epochs, out countEpochs_buf);
            if (epochs_flag)
            {
                if (countEpochs_buf <= 0)
                {
                    throw new Exception("Данные некорректны");
                }
                else
                {
                    countEpochs = countEpochs_buf;
                }
            }
            else
            {
                throw new Exception("Данные некорректны");
            }
        }
        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            Process();
            this.DialogResult = true;
        }
        public int CountEpochs
        {
            get { return countEpochs; }
        }
        public bool Flag
        {
            get { return flag; }
        }
        private void checkBoxChecked(object sender, RoutedEventArgs e)
        {
            flag = true;
        }

        private void checkBoxUnchecked(object sender, RoutedEventArgs e)
        {
            flag = false;
        }
        private void TextBoxNumber(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}
