﻿using System;
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
using VFSBrowser.ViewModel;

namespace VFSBrowser.View
{
    /// <summary>
    /// Interaction logic for DiskInfoDialog.xaml
    /// </summary>
    public partial class DiskInfoDialog : Window
    {
        public DiskInfoDialog(DiskInfoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            //TextBox.Focus();
            //TextBox.SelectedText = viewModel.Text;
        }
    }
}
