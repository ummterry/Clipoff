using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clipoff.UI
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.chbFacebook.IsChecked = Clipoff.Config.UseFacebook;
            this.chbTwitter.IsChecked = Clipoff.Config.UseTwitter;
            this.chbWeibo.IsChecked = Clipoff.Config.UseWeibo;
            this.chbRenren.IsChecked = Clipoff.Config.UseRenren;
            this.chbHotkey.IsChecked = Clipoff.Config.UseHotkey;
            this.chbAutoRun.IsChecked = Clipoff.Config.AutoRun;
            char hotKey = Clipoff.Config.HotKey;
            if (char.IsDigit(hotKey))
            {
                this.cmbHotkey.SelectedIndex = hotKey - '0' + 26;
            }
            else if (char.IsLetter(hotKey))
            {
                this.cmbHotkey.SelectedIndex = hotKey - 'A';
            }
            this.cmbHotkey.SelectionChanged += cmbHotkey_SelectionChanged;
        }

        void cmbHotkey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Clipoff.Config.HotKey = ((ComboBoxItem)this.cmbHotkey.SelectedItem).Content.ToString()[0];
            sayOK();
        }

        void chbFacebook_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.UseFacebook = this.chbFacebook.IsChecked.Value;
            sayOK();
        }
        void chbTwitter_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.UseTwitter = this.chbTwitter.IsChecked.Value;
            sayOK();
        }
        void chbWeibo_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.UseWeibo = this.chbWeibo.IsChecked.Value;
            sayOK();
        }
        void chbRenren_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.UseRenren = this.chbRenren.IsChecked.Value;
            sayOK();
        }
        void chbHotkey_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.UseHotkey = this.chbHotkey.IsChecked.Value;
            sayOK();
        }
        void chbAutoRun_Click(object sender, RoutedEventArgs e)
        {
            Clipoff.Config.AutoRun = this.chbAutoRun.IsChecked.Value;
            sayOK();
        }

        void sayOK()
        {
            this.txtMessage.Text = "Changes Saved";
        }

        #region title bar

        private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }

        private void Activate_Title_Icons(object sender, MouseEventArgs e)
        {
            Close_btn.Fill = (ImageBrush)TitleBar.Resources["Close_act"];
            Min_btn.Fill = (ImageBrush)TitleBar.Resources["Min_act"];
        }

        private void Deactivate_Title_Icons(object sender, MouseEventArgs e)
        {
            Close_btn.Fill = (ImageBrush)TitleBar.Resources["Close_inact"];
            Min_btn.Fill = (ImageBrush)TitleBar.Resources["Min_inact"];
        }

        private void Close_pressing(object sender, MouseButtonEventArgs e)
        {
            Close_btn.Fill = (ImageBrush)TitleBar.Resources["Close_pr"];
        }

        private void Min_pressing(object sender, MouseButtonEventArgs e)
        {
            Min_btn.Fill = (ImageBrush)TitleBar.Resources["Min_pr"];
        }

        private void EXIT(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MINIMIZE(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion

    }
}
