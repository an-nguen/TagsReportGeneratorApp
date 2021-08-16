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
using ReactiveUI;
using TagsReportGeneratorApp.ViewModel;

namespace TagsReportGeneratorApp.View
{
    /// <summary>
    /// Interaction logic for ZoneGroupManagerWindow.xaml
    /// </summary>
    public partial class ZoneGroupManagerWindow : Window, IViewFor<ZoneGroupManagerViewModel>
    {
        public ZoneGroupManagerWindow()
        {
            InitializeComponent();
            ViewModel = new ZoneGroupManagerViewModel();
            DataContext = ViewModel;

            ZoneGroupDataGrid.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(x => ZoneDelBtn.IsEnabled = ZoneModBtn.IsEnabled = x != null);
            ZoneGroupDataGrid.Events().MouseDoubleClick.Subscribe(x => ViewModel.OpenZoneEditWnd.Execute(null));
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ZoneGroupManagerViewModel)value;
        }

        public ZoneGroupManagerViewModel ViewModel { get; set; }
    }
}
