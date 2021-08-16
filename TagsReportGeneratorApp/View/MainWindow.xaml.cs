using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using OfficeOpenXml;
using ReactiveUI;
using TagsReportGeneratorApp.ViewModel;

namespace TagsReportGeneratorApp.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            ZoneListView.Events().SelectionChanged.Subscribe(x => ViewModel.OnListViewSelectionChanged());
            ZoneGroupDataGrid.Events().MouseDoubleClick.Subscribe(x => ViewModel.OnZoneGroupDblClick());
            ZoneGroupDataGrid.Events().SelectionChanged.Subscribe(x => ViewModel.OnZoneGroupSelectionChanged());
            SavePathTextBox.Events().LostFocus.Subscribe(x => ViewModel.ValidateAndStoreSavePath());
            ViewModel.WhenAnyValue(x => x.Loading).Subscribe(x =>
            {
                FormProgressBar0.IsIndeterminate = x;
            });
        }
    }
}
