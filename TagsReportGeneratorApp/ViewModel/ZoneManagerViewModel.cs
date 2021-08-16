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
    public class ZoneManagerViewModel : ReactiveObject
    {
        public ZoneRepository ZoneRepository { get; } = new ZoneRepository();

        public Zone SelectedZone { get; set; }

        public ObservableCollection<Zone> Zones { get; set; } = new ObservableCollection<Zone>();

        public ICommand OpenZoneAddWnd { get; }
        public ICommand OpenZoneEditWnd { get; }
        public ICommand DeleteZone { get; }


        public ZoneManagerViewModel ()
        {
            OpenZoneAddWnd = ReactiveCommand.Create(() =>
            {
                var wnd = new ZoneEditWindow(EditMode.Create, new Zone());
                wnd.Events().Closing.Subscribe(x => UpdateZones());
                wnd.ShowDialog();
            });
            OpenZoneEditWnd = ReactiveCommand.Create(() =>
            {
                if (SelectedZone == null) return;
                var wnd = new ZoneEditWindow(EditMode.Edit, SelectedZone);
                wnd.Events().Closing.Subscribe(x => UpdateZones());
                wnd.ShowDialog();
            });
            DeleteZone = ReactiveCommand.Create(() =>
            {
                ZoneRepository.Delete(SelectedZone);
                UpdateZones();
            });
            UpdateZones();
        }

        public void UpdateZones()
        {
            Zones.Clear();
            ZoneRepository.FindAll().ForEach(z => Zones.Add(z));
        }
    }
}
