using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.Repo;

namespace TagsReportGeneratorApp.ViewModel
{
    public enum EditMode
    {
        Create = 0,
        Edit
    }
    public class ZoneEditViewModel: ReactiveObject
    {
        public Zone Zone { get; set; }
        public EditMode Mode { get; }
        public Window RefWindow { get; }

        public string Title { get; set; }

        public TagRepository TagRepository { get; } = new TagRepository();
        public ZoneRepository ZoneRepository { get; } = new ZoneRepository();
        public ZoneTagRepository ZoneTagRepository { get; } = new ZoneTagRepository();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public ICommand SelectAll { get; }
        public ICommand DeselectAll { get; }
        public ICommand AddEditCommand { get; }

        public ZoneEditViewModel(EditMode mode, Zone zone, Window p)
        {
            Zone = zone;
            Mode = mode;
            RefWindow = p;
            SelectAll = ReactiveCommand.Create(() => SetIsCheckedForTags(true));
            DeselectAll = ReactiveCommand.Create(() => SetIsCheckedForTags(false));
            AddEditCommand = ReactiveCommand.Create(AddEdit);
            TagRepository.FindAll().ForEach(t => Tags.Add(t));

            switch (mode)
            {
                case EditMode.Create:
                    Title = "Создать";
                    break;
                case EditMode.Edit:
                    Title = "Изменить";
                    ZoneTagRepository.FindAll().FindAll(zt => zt.ZoneUuid == Zone.Uuid).ForEach(zt =>
                    {
                        var foundTag = Tags.FirstOrDefault(tag => tag.Uuid == zt.TagUuid);
                        if (foundTag != null)
                        {
                            foundTag.IsChecked = true;
                        }
                    });
                    break;
            }
        }

        private void AddEdit()
        {
            if (string.IsNullOrEmpty(Zone.Name))
            {
                MessageBox.Show("Название зоны не может быть пустым!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            switch (Mode)
            {
                case EditMode.Create:
                    Zone.Uuid = ZoneRepository.Create(Zone).AsGuid;
                    foreach (var tag in Tags)
                    {
                        if (tag.IsChecked)
                        {
                            var zoneTag = new ZoneTag()
                            {
                                TagUuid = tag.Uuid,
                                ZoneUuid = Zone.Uuid
                            };
                            ZoneTagRepository.Create(zoneTag);
                        }
                    }
                    break;
                case EditMode.Edit:
                    ZoneRepository.Update(Zone, Zone);
                    var zoneTagList = ZoneTagRepository.FindAll().FindAll(zt => zt.ZoneUuid == Zone.Uuid);
                    zoneTagList.ForEach(zt => ZoneTagRepository.Delete(zt));
                    foreach (var tag in Tags)
                    {
                        if (tag.IsChecked)
                        {
                            var zoneTag = new ZoneTag()
                            {
                                TagUuid = tag.Uuid,
                                ZoneUuid = Zone.Uuid
                            };
                            ZoneTagRepository.Create(zoneTag);
                        }
                    }
                    break;
            }
            RefWindow.Close();
        }

        private void SetIsCheckedForTags(bool val)
        {
            foreach (var tag in Tags)
            {
                tag.IsChecked = val;
            }
        }
    }
}
