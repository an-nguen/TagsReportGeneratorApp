using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.Repo;
using TagsReportGeneratorApp.View;

namespace TagsReportGeneratorApp.ViewModel
{
    public class ZoneGroupManagerViewModel : ReactiveObject
    {
        public ZoneGroupRepository ZoneGroupRepository { get; } = new ZoneGroupRepository();

        public ZoneGroup SelectedZoneGroup { get; set; }

        public ObservableCollection<ZoneGroup> ZoneGroups { get; set; } = new ObservableCollection<ZoneGroup>();

        public ICommand OpenZoneAddWnd { get; }
        public ICommand OpenZoneEditWnd { get; }
        public ICommand DeleteZone { get; }


        public ZoneGroupManagerViewModel()
        {
            OpenZoneAddWnd = ReactiveCommand.Create(() =>
            {
                var wnd = new ZoneGroupEditWindow(EditMode.Create, new ZoneGroup());
                wnd.Events().Closing.Subscribe(x => UpdateZoneGroups());
                wnd.ShowDialog();
            });
            OpenZoneEditWnd = ReactiveCommand.Create(() =>
            {
                if (SelectedZoneGroup == null) return;
                var wnd = new ZoneGroupEditWindow(EditMode.Edit, SelectedZoneGroup);
                wnd.Events().Closing.Subscribe(x => UpdateZoneGroups());
                wnd.ShowDialog();
            });
            DeleteZone = ReactiveCommand.Create(() =>
            {
                ZoneGroupRepository.Delete(SelectedZoneGroup);
                UpdateZoneGroups();
            });
            UpdateZoneGroups();
        }

        public void UpdateZoneGroups()
        {
            ZoneGroups.Clear();
            ZoneGroupRepository.FindAll().ForEach(z => ZoneGroups.Add(z));
        }
    }
}
