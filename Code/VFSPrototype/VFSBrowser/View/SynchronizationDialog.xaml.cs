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
using VFSBrowser.ViewModel;

namespace VFSBrowser.View
{
    /// <summary>
    /// Interaction logic for SynchronizationDialog.xaml
    /// </summary>
    internal partial class SynchronizationDialog : Window
    {
        readonly string[] _images = new[] { 
            "http://imgs.xkcd.com/comics/flowchart.png",
            "http://imgs.xkcd.com/comics/bonding.png",
            "http://imgs.xkcd.com/comics/ineffective_sorts.png", 
            "http://imgs.xkcd.com/comics/encoding.png", 
            "http://imgs.xkcd.com/comics/footnote_labyrinths.png",
            "http://imgs.xkcd.com/comics/einstein.png",
            "http://imgs.xkcd.com/comics/integration_by_parts.png",
            "http://imgs.xkcd.com/comics/estimation.png"
        };

        private int _imageIndex;
        private readonly SynchronizationViewModel _viewModel;

        public SynchronizationDialog(SynchronizationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
        }

        private void ProhibitClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_viewModel.Closed;
            Comic.Source = new BitmapImage(new Uri(_images[_imageIndex++ % _images.Length]));
            Explanation.Content = "Sorry, synchronization cannot be aborted, it's too dangerous";
        }
    }
}
