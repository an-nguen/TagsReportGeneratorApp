using System.Windows;
using ReactiveUI;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.ViewModel;

namespace TagsReportGeneratorApp.View
{
    /// <summary>
    /// Interaction logic for ZoneEditWindow.xaml
    /// </summary>
    public partial class ZoneEditWindow : Window, IViewFor<ZoneEditViewModel>
    {

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ZoneEditViewModel)value;
        }

        public ZoneEditViewModel ViewModel { get; set; }

        public ZoneEditWindow(EditMode mode, Zone zone)
        {
            InitializeComponent();
            ViewModel = new ZoneEditViewModel(mode, zone, this);
            DataContext = ViewModel;
            Title =  ViewModel.Title;
        }

    }
}
