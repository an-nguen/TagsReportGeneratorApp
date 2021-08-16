using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using ReactiveUI;
using TagsReportGeneratorApp.Repo;
using TagsReportGeneratorApp.Model;
using TagsReportGeneratorApp.Models.Request;
using TagsReportGeneratorApp.Models.Response;
using TagsReportGeneratorApp.View;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Measurement = TagsReportGeneratorApp.Model.Measurement;

namespace TagsReportGeneratorApp.ViewModel
{
    public class MainViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private readonly Uri BaseAddress = new Uri("https://wirelesstag.net");

        private Dictionary<string, string> dataTypes = new Dictionary<string, string>()
        {
            {"Температура", "temperature"},
            {"Влажность", "cap"}
        };

        public AccountRepository AccountRepository { get; } = new AccountRepository();
        public TagRepository TagRepository { get; } = new TagRepository();
        public ZoneRepository ZoneRepository { get; } = new ZoneRepository();
        public ZoneTagRepository ZoneTagRepository { get; } = new ZoneTagRepository();
        public ZoneGroupRepository ZoneGroupRepository { get; } = new ZoneGroupRepository();
        public ConfigRepository ConfigRepository { get; } = new ConfigRepository();

        private string _savePath;
        public string SavePath { 
            get => _savePath;
            set
            {
                _savePath = value;
                OnPropertyChanged(nameof(SavePath));
            }
        }

        private DateTime _startDateTime = DateTime.Now;
        public DateTime StartDateTime
        {
            get => _startDateTime;
            set
            {
                _startDateTime = value;
                OnPropertyChanged("StartDateTime");
            }
        }

        private DateTime _endDateTime = DateTime.Now;
        public DateTime EndDateTime
        {
            get => _endDateTime;
            set
            {
                _endDateTime = value;
                OnPropertyChanged("EndDateTime");
            }
        }

        private bool _loading = false;

        public bool Loading
        {
            get => _loading;
            set
            {
                this.RaiseAndSetIfChanged(ref _loading, value);
            }
        }

        public Zone SelectedZone { get; set; }
        public ZoneGroup SelectedZoneGroup { get; set; }
        public ComboBoxItem SelectedDataType { get; set; }

        public ICommand SetMorningTimeCommand { get; }
        public ICommand SetEveningTimeCommand { get; }
        public ICommand SetWeekTimeCommand { get; }
        public ICommand SetFriToMonTimeCommand { get; }
        public ICommand OpenAccountManager { get; }
        public ICommand OpenZoneManager { get; }
        public ICommand OpenZoneGroupManager { get; }
        public ICommand LoadTagsFromCloud { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand SelectAllZones { get; }
        public ICommand DeselectAllZones { get; }

        public ObservableCollection<Zone> Zones { get; set; } = new ObservableCollection<Zone>();
        public ObservableCollection<ZoneGroup> ZoneGroups { get; set; } = new ObservableCollection<ZoneGroup>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public MainViewModel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            SetMorningTime();

            SetMorningTimeCommand = ReactiveCommand.Create(SetMorningTime);
            SetEveningTimeCommand = ReactiveCommand.Create(SetEveningTime);
            SetWeekTimeCommand = ReactiveCommand.Create(SetWeekTime);
            SetFriToMonTimeCommand = ReactiveCommand.Create(SetFridayToMondayTime);

            SelectAllZones = ReactiveCommand.Create(() =>
            {
                foreach (var zone in Zones)
                {
                    zone.IsChecked = true;
                }
            });


            DeselectAllZones = ReactiveCommand.Create(() =>
            {
                foreach (var zone in Zones)
                {
                    zone.IsChecked = false;
                }
            });

            OpenAccountManager = ReactiveCommand.Create(() =>
            {
                var wnd = new AccountManagerWindow();
                wnd.ShowDialog();
            });
            OpenZoneManager = ReactiveCommand.Create(() =>
            {
                var wnd = new ZoneManagerWindow();
                wnd.Events().Closing.Subscribe(x =>
                {
                    Zones.Clear();
                    ZoneRepository.FindAll().ForEach(z => Zones.Add(z));
                });
                wnd.ShowDialog();
            });
            OpenZoneGroupManager = ReactiveCommand.Create(() =>
            {
                var wnd = new ZoneGroupManagerWindow();
                wnd.Events().Closing.Subscribe(x =>
                {
                    ZoneGroups.Clear();
                    ZoneGroupRepository.FindAll().ForEach(z => ZoneGroups.Add(z));
                });
                wnd.ShowDialog();
            });
            LoadTagsFromCloud = ReactiveCommand.CreateFromTask(async () =>
            {
                Loading = true;
                var accounts = AccountRepository.FindAll();
                await LoadTagsRemotely(accounts);
                MessageBox.Show("Теги обновлены!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Loading = false;
            });
            GenerateReportCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Loading = true;
                if (SelectedDataType == null)
                {
                    MessageBox.Show("Не выбран тип данных!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var dataType = dataTypes[(SelectedDataType.Content as TextBlock).Text];
                await Task.Run(() =>
                {
                    if (GenerateReports(Zones, StartDateTime, EndDateTime, dataType))
                    {
                        MessageBox.Show("Отчёты сформированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
                Loading = false;
            });

            var savePathConfigParam = ConfigRepository.FindAll().Find(x => x.Parameter == "save_path");
            SavePath = (savePathConfigParam != null) ? savePathConfigParam.Value : @".";


            ZoneRepository.FindAll().ForEach(z => Zones.Add(z));
            ZoneGroupRepository.FindAll().ForEach(zg => ZoneGroups.Add(zg));
        }

        public void OnListViewSelectionChanged()
        {
            if (SelectedZone == null) return;
            Tags.Clear();
            FindTagsByZone(SelectedZone).ForEach(z => Tags.Add(z));
        }


        public void OnZoneGroupSelectionChanged()
        {
            if (SelectedZoneGroup == null) return;

            Tags.Clear();
            SelectedZoneGroup.Zones.ForEach(z =>
            {
                FindTagsByZone(z).ForEach(t => Tags.Add(t));
            });
        }


        public void OnZoneGroupDblClick()
        {
            if (SelectedZoneGroup == null) return;

            foreach (var zone in Zones)
            {
                zone.IsChecked = false;
            }
            SelectedZoneGroup.Zones.ForEach(z =>
            {
                var foundZone = Zones.FirstOrDefault(zone => zone.Uuid == z.Uuid);
                if (foundZone != null)
                {
                    foundZone.IsChecked = true;
                }
            });
        }


        public bool ValidateAndStoreSavePath()
        {
            var savePathConfigParameter = ConfigRepository.FindAll().Find(x => x.Parameter == "save_path");
            if (!Directory.Exists(SavePath))
            {
                MessageBox.Show($"Директория {SavePath} не существует", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SavePath = ".";
                return false;
            }

            if (savePathConfigParameter != null)
            {
                ConfigRepository.Update(savePathConfigParameter, new Config()
                {
                    Value = SavePath
                });
            }
            else
            {
                ConfigRepository.Create(new Config()
                {
                    Parameter = "save_path",
                    Value = SavePath
                });
            }

            return true;
        }

        private void SetMorningTime()
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = now.AddDays(-1).AddHours(16);
            EndDateTime = now.AddHours(9);
        }

        private void SetEveningTime()
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = now.AddHours(9);
            EndDateTime = now.AddHours(16);
        }


        private void SetWeekTime()
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Saturday).AddHours(9);
            EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
        }

        private void SetFridayToMondayTime()
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Tuesday).AddHours(16);
            EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
        }

        private void GetTagsRemotely(Account account)
        {
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            })
            {
                using (var client = new HttpClient(handler) { BaseAddress = BaseAddress })
                {
                    cookieContainer.Add(BaseAddress, new Cookie("WTAG", account.SessionId));
                    var content = new StringContent("{}", Encoding.UTF8, "application/json");
                    var result = client.PostAsync("/ethClient.asmx/GetTagList2", content).Result;
                    string responseBody = result.Content.ReadAsStringAsync().Result;

                    var jsonResponse =
                        JsonConvert.DeserializeObject<DefaultWstResponse<TagResponse>>(responseBody);
                    if (jsonResponse == null || jsonResponse.d.Count == 0)
                    {
                        return;
                    }

                    foreach (var tagResponse in jsonResponse.d)
                    {
                        var tag = new Tag(Guid.Parse(tagResponse.uuid), tagResponse.name)
                        {
                            TagManagerName = tagResponse.managerName,
                            TagManagerMAC = tagResponse.mac,
                            Account = account
                        };
                        var dbTag = TagRepository.FindAll().FirstOrDefault(t => t.Uuid == tag.Uuid);
                        if (dbTag != null)
                        {
                            continue;
                        }
                        TagRepository.Create(tag);
                    }

                }
            }
        }

        public async Task<bool> LoadTagsRemotely(List<Account> accounts)
        {
            var tags = TagRepository.FindAll();
            tags.ForEach(t => TagRepository.Delete(t));
            foreach (var account in accounts)
            {
                if (!await account.SignIn(BaseAddress))
                {
                    MessageBox.Show($"Не получилось загрузить список тегов {account.Email}", "Предупреждение", MessageBoxButton.OK);
                    continue;
                }
                GetTagsRemotely(account);
            }
            return true;
        }

        public List<Tag> FindTagsByZone(Zone zone)
        {
            var tags = new List<Tag>();

            var zoneTags = ZoneTagRepository.FindAll().FindAll(zt => zt.ZoneUuid == zone.Uuid);
            foreach (var zoneTag in zoneTags)
            {
                var tag = TagRepository.FindAll().First(t => t.Uuid == zoneTag.TagUuid);
                tags.Add(tag);
            }

            return tags;
        }

        private void LoadMeasurements(Zone zone, DateTime startDate, DateTime endDate)
        {
            string fromDateStr = startDate.ToString("M/d/yyyy HH:mm");
            string toDateStr = endDate.ToString("M/d/yyyy HH:mm");

            var tasks = new List<Task>();

            foreach (var tag in zone.Tags)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var request = new TemperatureRawDataRequest();
                    request.fromDate = fromDateStr;
                    request.toDate = toDateStr;
                    request.uuid = tag.Uuid.ToString();
                    var client = new HttpClient() { BaseAddress = BaseAddress };

                    var jsonRequestBody = JsonConvert.SerializeObject(request);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                    var response = client.PostAsync("/ethLogShared.asmx/GetTemperatureRawDataByUUID", content).Result;
                    if (response == null)
                    {
                        return;
                    }

                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var jsonResponse = JsonConvert.DeserializeObject<DefaultWstResponse<TemperatureRawDataResponse>>(responseBody);
                    tag.Measurements = new List<Measurement>();
                    if (jsonResponse.d == null) return;
                    foreach (var r in jsonResponse.d)
                    {
                        var dateTime = DateTime.Parse(r.time);
                        if (dateTime > startDate && dateTime < endDate)
                        {
                            tag.Measurements.Add(new Measurement(dateTime, Math.Round(r.temp_degC, 6), r.cap));
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

        }
        // Loads measurements and create xlsx files
        private bool GenerateReport(Zone zone, DateTime startDate, DateTime endDate, string dirName, string dataType)
        {

            zone.Tags = FindTagsByZone(zone);

            LoadMeasurements(zone, startDate, endDate);

            GenerateXlsxReportFile(zone, startDate, endDate, dirName, dataType);

            zone.Tags.Clear();

            return true;
        }
        private static double GetExcelDecimalValueForDate(DateTime date)
        {
            DateTime start = new DateTime(1900, 1, 1);
            TimeSpan diff = date - start;
            return diff.TotalDays + 2;
        }

        private void GenerateXlsxReportFile(Zone zone, DateTime StartDate, DateTime EndDate, string dirName, string dataType)
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy HH-mm");
            string path = $@"{dirName}\{zone.Name} - {date}.xlsx";


            using (var fs = File.Create(path))
            {
                using (var package = new ExcelPackage(fs))
                {
                    var chartSheet = package.Workbook.Worksheets.Add("График");
                    var tagsSheet = package.Workbook.Worksheets.Add("Теги");
                    var scatterLineChart0 = chartSheet.Drawings.AddChart("scatterLineChart0", eChartType.XYScatterSmoothNoMarkers);
                    scatterLineChart0.Title.Text = $"{zone.Name}\n{StartDate.ToString("dd.MM.yyyy HH:mm:ss")} - {EndDate.ToString("dd.MM.yyyy HH:mm:ss")}";
                    scatterLineChart0.Title.Font.Size = 18;
                    scatterLineChart0.SetSize(1024, 768);
                    scatterLineChart0.XAxis.Title.Text = "Время";
                    scatterLineChart0.XAxis.Format = "dd.MM.yyyy HH:mm:ss";
                    scatterLineChart0.XAxis.TextBody.Rotation = 270;
                    scatterLineChart0.XAxis.MinValue = GetExcelDecimalValueForDate(StartDate);
                    scatterLineChart0.XAxis.MaxValue = GetExcelDecimalValueForDate(EndDate);
                    switch (dataType)
                    {
                        case "temperature":
                            scatterLineChart0.YAxis.Title.Text = "Температура (ºС)";
                            break;
                        case "cap":
                            scatterLineChart0.YAxis.Title.Text = "Влажность (%)";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dataType), $"Неизвестный тип ${dataType}");
                    }
                    scatterLineChart0.YAxis.Title.Rotation = 270;
                    scatterLineChart0.YAxis.Format = "0.00";
                    scatterLineChart0.YAxis.MajorTickMark = eAxisTickMark.Out;
                    scatterLineChart0.YAxis.MinorTickMark = eAxisTickMark.None;
                    

                    scatterLineChart0.Legend.Position = eLegendPosition.Right;
                    int dateTimePointer = 1; // A
                    int valuePointer = 2; // B
                    double minValue = zone.Tags[0].Measurements[0].TemperatureValue;
                    double maxValue = zone.Tags[0].Measurements[0].TemperatureValue;
                    foreach (var tag in zone.Tags)
                    {
                        int rowNumber = 1;
                        tagsSheet.Cells[rowNumber, dateTimePointer].Value = "Дата Время";
                        tagsSheet.Cells[rowNumber, valuePointer].Value = tag.Name;
                        ExcelRange rangeLabel = tagsSheet.Cells[rowNumber, dateTimePointer, rowNumber, valuePointer];

                        foreach (var m in tag.Measurements)
                        {
                            if (m.TemperatureValue < minValue) minValue = m.TemperatureValue;
                            if (m.TemperatureValue > maxValue) maxValue = m.TemperatureValue;
                            rowNumber++;
                            tagsSheet.Cells[rowNumber, dateTimePointer].Value = m.DateTime;
                            tagsSheet.Cells[rowNumber, dateTimePointer].Style.Numberformat.Format = "dd.MM.yyyy HH:mm:ss";
                            switch (dataType)
                            {
                                case "temperature":
                                    tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.TemperatureValue, 6);
                                    break;
                                case "cap":
                                    tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.Cap, 6);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(dataType), $"Неизвестный тип ${dataType}");
                            }
                        }

                        scatterLineChart0.YAxis.MinValue = Math.Round(minValue - 1);
                        scatterLineChart0.YAxis.MaxValue = Math.Round(maxValue + 1);

                        var serie = scatterLineChart0.Series.Add(
                            tagsSheet.Cells[2, valuePointer, rowNumber, valuePointer],
                            tagsSheet.Cells[2, dateTimePointer, rowNumber, dateTimePointer]
                            );
                        serie.HeaderAddress = tagsSheet.Cells[1, valuePointer];

                        tagsSheet.Cells[rowNumber, dateTimePointer].AutoFitColumns();
                        tagsSheet.Cells[rowNumber, valuePointer].AutoFitColumns();

                        dateTimePointer += 2;
                        valuePointer += 2;
                        tag.Measurements.Clear();
                    }


                    package.Save();
                }
            }

        }

        public bool GenerateReports(IEnumerable<Zone> zones, DateTime startDateTime, DateTime endDateTime, string dataType)
        {
            if (!ValidateAndStoreSavePath())
            {
                return false;
            }
            try
            {
                List<Task> tasks = new List<Task>();
                foreach (var zone in zones)
                {
                    if (!zone.IsChecked) continue;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        if (!GenerateReport(zone, startDateTime, endDateTime, SavePath, dataType))
                        {
                            MessageBox.Show($"Не удалось сделать отчёт для {zone.Name}", "Ошибка",
                                MessageBoxButton.OK);
                        }
                    }));
                }

                var taskArray = tasks.ToArray();
                Task.WaitAll(taskArray);

                var startInfo = new ProcessStartInfo()
                {
                    Arguments = SavePath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                var exceptionPathLog = @".\exception.log";
                MessageBox.Show($"{exception.Message}\n{exception}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (File.Exists(exceptionPathLog))
                {
                    File.Delete(exceptionPathLog);
                }

                using (var sw = new StreamWriter(exceptionPathLog))
                {
                    sw.Write(exception);
                }
                Console.WriteLine(exception.Message);
                return false;
            }
            return true;
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
