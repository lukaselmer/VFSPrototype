using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VFSBrowser.Annotations;
using VFSBrowser.ViewModel;

namespace VFSBrowser.View
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(InputViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException("viewModel");

            InitializeComponent();
            DataContext = viewModel;
            TextBox.Focus();
            TextBox.SelectedText = viewModel.Text;
        } 
    }
}
