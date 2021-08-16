using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.Repo;

namespace TagsReportGeneratorApp.ViewModel
{
    public class ZoneGroupEditViewModel: ReactiveObject
    {
        public ZoneGroup ZoneGroup { get; set; }
        public EditMode Mode { get; }
        public Window RefWindow { get; }

        public string Title { get; set; }

        public ZoneRepository ZoneRepository { get; } = new ZoneRepository();
        public ZoneGroupRepository ZoneGroupRepository { get; } = new ZoneGroupRepository();
        public ObservableCollection<Zone> Zones { get; set; } = new ObservableCollection<Zone>();

        public ICommand SelectAll { get; }
        public ICommand DeselectAll { get; }
        public ICommand AddEditCommand { get; }

        public ZoneGroupEditViewModel(EditMode mode, ZoneGroup zoneGroup, Window p)
        {
            ZoneGroup = zoneGroup;
            Mode = mode;
            RefWindow = p;
            SelectAll = ReactiveCommand.Create(() => SetIsCheckedForTags(true));
            DeselectAll = ReactiveCommand.Create(() => SetIsCheckedForTags(false));
            AddEditCommand = ReactiveCommand.Create(AddEdit);


            ZoneRepository.FindAll().ForEach(z =>
            {
                z.IsChecked = false;
                Zones.Add(z);
            });

            switch (mode)
            {
                case EditMode.Create:
                    Title = "Создать";
                    break;
                case EditMode.Edit:
                    Title = "Изменить";
                    ZoneGroup.Zones.ForEach(z =>
                    {
                        var foundZone = Zones.FirstOrDefault(zone => zone.Uuid == z.Uuid);
                        if (foundZone != null)
                        {
                            foundZone.IsChecked = true;
                        }
                    });
                    break;
            }
        }

        private void AddEdit()
        {
            if (string.IsNullOrEmpty(ZoneGroup.Name))
            {
                MessageBox.Show("Название зоны не может быть пустым!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            switch (Mode)
            {
                case EditMode.Create:
                    ZoneGroup.Zones = new List<Zone>();
                    foreach (var zone in Zones)
                    {
                        if (zone.IsChecked)
                        {
                            ZoneGroup.Zones.Add(zone);
                        }
                    }
                    ZoneGroup.Id = ZoneGroupRepository.Create(ZoneGroup).AsInt32;
                    break;
                case EditMode.Edit:
                    ZoneGroup.Zones = new List<Zone>();
                    foreach (var zone in Zones)
                    {
                        if (zone.IsChecked)
                        {
                            ZoneGroup.Zones.Add(zone);
                        }
                    }
                    ZoneGroupRepository.Update(ZoneGroup, ZoneGroup);
                    break;
            }
            RefWindow.Close();
        }

        private void SetIsCheckedForTags(bool val)
        {
            foreach (var z in Zones)
            {
                z.IsChecked = val;
            }
        }
    }
}
