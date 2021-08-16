using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
    /// Interaction logic for AccountManagerWindow.xaml
    /// </summary>
    public partial class AccountManagerWindow : Window, IViewFor<AccountManagerViewModel>
    {

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AccountManagerViewModel)value;
        }

        public AccountManagerViewModel ViewModel { get; set; }

        public AccountManagerWindow()
        {
            InitializeComponent();
            ViewModel = new AccountManagerViewModel();
            this.DataContext = ViewModel;
            this.AccountListView.WhenAnyValue(x => x.SelectedItem).Subscribe(x =>
            {
                this.RemoveBtn.IsEnabled = x != null;
            });
        }


    }
}
