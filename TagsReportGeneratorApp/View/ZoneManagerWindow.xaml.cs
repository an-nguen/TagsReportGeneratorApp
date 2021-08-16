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
    /// Interaction logic for ZoneManagerWindow.xaml
    /// </summary>
    public partial class ZoneManagerWindow : Window, IViewFor<ZoneManagerViewModel>
    {

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ZoneManagerViewModel)value;
        }

        public ZoneManagerViewModel ViewModel { get; set; }

        public ZoneManagerWindow()
        {
            InitializeComponent();
            ViewModel = new ZoneManagerViewModel();
            DataContext = ViewModel;
            ZoneListView.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(x => ZoneDelBtn.IsEnabled = ZoneModBtn.IsEnabled = x != null);
            ZoneListView.Events().MouseDoubleClick.Subscribe(x => ViewModel.OpenZoneEditWnd.Execute(null));
        }
    }
}
