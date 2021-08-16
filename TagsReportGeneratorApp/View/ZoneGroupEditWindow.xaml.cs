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
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.ViewModel;

namespace TagsReportGeneratorApp.View
{
    /// <summary>
    /// Interaction logic for ZoneGroupEditWindow.xaml
    /// </summary>
    public partial class ZoneGroupEditWindow : Window, IViewFor<ZoneGroupEditViewModel>
    {
        public ZoneGroupEditWindow(EditMode mode, ZoneGroup zg)
        {
            InitializeComponent();
            ViewModel = new ZoneGroupEditViewModel(mode, zg, this);
            DataContext = ViewModel;
            Title = ViewModel.Title;
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ZoneGroupEditViewModel)value;
        }

        public ZoneGroupEditViewModel ViewModel { get; set; }
    }
}
