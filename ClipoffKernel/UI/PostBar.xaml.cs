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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clipoff.UI
{
    /// <summary>
    /// Interaction logic for PostBar.xaml
    /// </summary>
    public partial class PostBar : UserControl
    {
        public PostBar(ToolBar toolBar)
        {
            InitializeComponent();
            this.toolBar = toolBar;
            this.Width = this.textBox.Width = toolBar.Width;
        }
        ToolBar toolBar;
        public string Message
        {
            get
            {
                return this.textBox.Text.Trim();
            }
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            toolBar.Post();
        }
    }
}
