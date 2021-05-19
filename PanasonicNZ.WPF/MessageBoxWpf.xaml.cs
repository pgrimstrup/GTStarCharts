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

namespace PanasonicNZ.WPF
{
    /// <summary>
    /// Interaction logic for MessageBoxWpf.xaml
    /// </summary>
    public partial class MessageBoxWpf : Window
    {
        internal MessageBoxButton Buttons { get; private set; }
        internal MessageBoxResult? Result { get; private set; }

        internal MessageBoxWpf()
        {
            InitializeComponent();
        }

        internal void Init(Window owner, string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            this.Owner = owner ?? Application.Current.MainWindow;
            this.Title = title;
            this.MessageText.Text = message;
            this.Buttons = buttons;

            switch(buttons)
            {
                case MessageBoxButton.OK:
                    Button1.Content = "OK";
                    Button1.IsDefault = true;
                    Button1.IsCancel = true;
                    
                    Button2.Visibility = Visibility.Collapsed;
                    Button3.Visibility = Visibility.Collapsed;
                    break;

                case MessageBoxButton.OKCancel:
                    Button1.Content = "OK";
                    Button1.IsDefault = true;
                    Button2.Content = "Cancel";
                    Button2.IsCancel = true;

                    Button3.Visibility = Visibility.Collapsed;
                    break;

                case MessageBoxButton.YesNo:
                    Button1.Content = "Yes";
                    Button1.IsDefault = true;
                    Button2.Content = "No";
                    Button2.IsCancel = true;

                    Button3.Visibility = Visibility.Collapsed;
                    break;

                case MessageBoxButton.YesNoCancel:
                    Button1.Content = "Yes";
                    Button1.IsDefault = true;
                    Button2.Content = "No";
                    Button3.Content = "Cancel";
                    Button3.IsCancel = true;
                    break;
            }

            switch(image)
            {
                case MessageBoxImage.Information:
                    InformationIcon.Visibility = Visibility.Visible;
                    break;
                case MessageBoxImage.Question:
                    QuestionIcon.Visibility = Visibility.Visible;
                    break;
                case MessageBoxImage.Warning:
                    WarningIcon.Visibility = Visibility.Visible;
                    break;
                case MessageBoxImage.Error:
                    ErrorIcon.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            switch (Buttons)
            {
                case MessageBoxButton.OK: Result = MessageBoxResult.OK; break;
                case MessageBoxButton.OKCancel: Result = MessageBoxResult.OK; break;
                case MessageBoxButton.YesNo: Result = MessageBoxResult.Yes; break;
                case MessageBoxButton.YesNoCancel: Result = MessageBoxResult.Yes; break;
            }
            DialogResult = true;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            switch (Buttons)
            {
                case MessageBoxButton.OKCancel: Result = MessageBoxResult.Cancel; break;
                case MessageBoxButton.YesNo: Result = MessageBoxResult.No; break;
                case MessageBoxButton.YesNoCancel: Result = MessageBoxResult.No; break;
            }
            DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            switch (Buttons)
            {
                case MessageBoxButton.YesNoCancel: Result = MessageBoxResult.Cancel; break;
            }
            DialogResult = true;
        }

        public static MessageBoxResult? Show(string message)
        {
            return Show(null, message, "", MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(string message, string title)
        {
            return Show(null, message, title, MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(string message, string title, MessageBoxButton buttons)
        {
            return Show(null, message, title, buttons, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            return Show(null, message, title, buttons, image);
        }

        public static MessageBoxResult? Show(Window owner, string message)
        {
            return Show(owner, message, "", MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(Window owner, string message, string title)
        {
            return Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(Window owner, string message, string title, MessageBoxButton buttons)
        {
            return Show(owner, message, title, buttons, MessageBoxImage.None);
        }
        public static MessageBoxResult? Show(Window owner, string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            MessageBoxWpf dlg = new MessageBoxWpf();
            dlg.Init(owner, message, title, buttons, image);
            dlg.Button1.Focus();
            if (dlg.ShowDialog() == true)
                return dlg.Result;
            return null;
        }

    }
}
