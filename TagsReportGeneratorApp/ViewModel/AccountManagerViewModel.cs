using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.Repo;

namespace TagsReportGeneratorApp.ViewModel
{
    public class AccountManagerViewModel: ReactiveObject
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public Account SelectedAccount { get; set; }

        public AccountRepository AccountRepository { get; } = new AccountRepository();
        public ObservableCollection<Account> Accounts { get; set; } = new ObservableCollection<Account>();

        public ICommand CreateAccount { get; }
        public ICommand RemoveAccount { get; }

        public AccountManagerViewModel()
        {
            CreateAccount = ReactiveCommand.Create(() =>
            {
                var account = new Account(Email, Password);
                AccountRepository.Create(account);
                UpdateAccountCollection();
            });
            RemoveAccount = ReactiveCommand.Create(() =>
            {
                AccountRepository.Delete(SelectedAccount);
                UpdateAccountCollection();
            });

            UpdateAccountCollection();
        }

        public void UpdateAccountCollection()
        {
            Accounts.Clear();
            AccountRepository.FindAll().ForEach(a => Accounts.Add(a));
        }
    }
}
