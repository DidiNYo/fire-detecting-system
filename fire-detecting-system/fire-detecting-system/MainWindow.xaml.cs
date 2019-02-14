using ExternalServices;
using ExternalServices.Models;
using fire_detecting_system.Models;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace fire_detecting_system
{
    public partial class MainWindow
    {
        MainViewModel mainModel;

        int numberOfClicks;

        IFeature clickedFeature;

        int zoomLevel;

        Point centerPoint;

        List<AlarmRule> alarms;

        public MainWindow()
        {
            InitializeComponent();

            mainModel = new MainViewModel();

            //Load sensors on the map
            Loaded += OnLoaded;

            //For labels.
            numberOfClicks = 0;

            //Set mainModel to be source for the DataContext property          
            DataContext = mainModel;

            //Subscribe for clicked left mouse button event
            MainMap.Info += MainMaplOnInfo;

            //Default zoom level
            cmbBoxZoomLevel.ItemsSource = LoadComboBoxZoomLevel();
            zoomLevel = GetSettings.GetSettingsInstance.SettingsData.ZoomLevel;
            mainModel.Zoom.Level = zoomLevel.ToString();

            //Default center point
            centerPoint = SphericalMercator.FromLonLat(GetSettings.GetSettingsInstance.SettingsData.XCoord, GetSettings.GetSettingsInstance.SettingsData.YCoord);
            mainModel.Coords.XCoordinate = (GetSettings.GetSettingsInstance.SettingsData.XCoord).ToString(CultureInfo.InvariantCulture);
            mainModel.Coords.YCoordinate = (GetSettings.GetSettingsInstance.SettingsData.YCoord).ToString(CultureInfo.InvariantCulture);

        }

        private void OnUpdateCompleted(object sender, EventArgs e)
        {
            MainMap.RefreshData();
        }

        private void OnNotificationRaised(object sender, EventArgs e)
        {
           MainMap.Refresh();
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await mainModel.Sensors.InitializeAsync(MainMap, mainModel.APIConnection);
            mainModel.Sensors.OnUpdateCompleted += OnUpdateCompleted;
            mainModel.Sensors.OnNotificationRaised += OnNotificationRaised;


            IEnumerable<string> names = mainModel.Sensors.Sensors.Select(n => n.Name);
            cmbBoxSensor.ItemsSource = names;

            cmbBoxSign.ItemsSource = LoadComboBoxSign();

            string fileResult;
            if (File.Exists("AlarmRules.json") == false)
            {
                File.Create("AlarmRules.json");
                fileResult = null;
            }
            else
            {
                fileResult = File.ReadAllText("AlarmRules.json");
            }

            if (string.IsNullOrEmpty(fileResult) == false)
            {
                alarms = JsonConvert.DeserializeObject<List<AlarmRule>>(fileResult);
            }
            else
            {
                alarms = new List<AlarmRule>();
            }
            lstDefinedAlarms.ItemsSource = alarms;
        }

        //Show label on clicked sensor
        private void MainMaplOnInfo(object sender, MapInfoEventArgs args)
        {
            ++numberOfClicks;
            if (numberOfClicks == 1)
            {
                //2. Remove it.
                if (clickedFeature != null)
                {
                    mainModel.Sensors.HideLabel(clickedFeature);
                }
                mainModel.Sensors.DisplayLabel(args.MapInfo.Feature);
                clickedFeature = args.MapInfo.Feature;
            }
            if (numberOfClicks == 2)
            {
                if (clickedFeature != null)
                {
                    mainModel.Sensors.HideLabel(clickedFeature);
                    clickedFeature = null;
                    //1. If second click is on a new feature show label for it.
                    if (args.MapInfo.Feature != null)
                    {
                        mainModel.Sensors.DisplayLabel(args.MapInfo.Feature);
                        clickedFeature = args.MapInfo.Feature;
                    }
                }
                numberOfClicks = 0;
            }
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mainModel.Coords.XCoordinate) == false && string.IsNullOrEmpty(mainModel.Coords.YCoordinate) == false)
            {
                double xCoord = Convert.ToDouble(mainModel.Coords.XCoordinate, CultureInfo.InvariantCulture);
                double yCoord = Convert.ToDouble(mainModel.Coords.YCoordinate, CultureInfo.InvariantCulture);

                //We need to transfer from one coordinate reference system to another - from CRS to CRS
                //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
                centerPoint = SphericalMercator.FromLonLat(xCoord, yCoord);

                //Map is centered with coordinates provided by the user
                MainMap.Navigator.NavigateTo(centerPoint, MainMap.Map.Resolutions[zoomLevel]);

                SaveSettingsInFile(centerPoint, zoomLevel);

                //Return to the first tab after changed
                MainTabs.SelectedIndex = 0;
            }
        }

        private void Btn_ClickApplyZoomLevel(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomLevel = Convert.ToInt32(mainModel.Zoom.Level);

            MainMap.Navigator.NavigateTo(centerPoint, MainMap.Map.Resolutions[zoomLevel]);

            SaveSettingsInFile(centerPoint, zoomLevel);

            //Return to the first tab after changed
            MainTabs.SelectedIndex = 0;
        }

        private void CmbBoxSensorName_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmbBoxSensor.SelectedValue != null)
            {
                string name = cmbBoxSensor.SelectedValue.ToString();
                if (mainModel.Sensors.LastMeasurements[name] != null)
                {
                    LastMeasurement currentMeasurment = mainModel.Sensors.LastMeasurements[name];
                    if (currentMeasurment.Values.Count > 0)
                    {
                        IEnumerable<string> data = currentMeasurment.Values.Select(d => d.Name);
                        cmbBoxMeasurement.ItemsSource = data;
                    }
                    else
                    {
                        cmbBoxMeasurement.ItemsSource = null;
                    }
                }
            }
        }

        private string[] LoadComboBoxZoomLevel()
        {
            ZoomLevel zoomLevels = new ZoomLevel();
            return zoomLevels.Levels;
        }

        private string[] LoadComboBoxSign()
        {
            string[] signs = { ">", "<", "=", ">=", "<=" };
            return signs;
        }

        private void Btn_ClickClear(object sender, System.Windows.RoutedEventArgs e)
        {
            cmbBoxSensor.SelectedItem = null;
            cmbBoxMeasurement.SelectedItem = null;
            cmbBoxSign.SelectedItem = null;
            alarmValue.Text = "";
        }

        private void SaveSettingsInFile(Point currentPoint, int zoomLevel)
        {
            Point changedPoint = SphericalMercator.ToLonLat(currentPoint.X, currentPoint.Y);
            Settings changedSettings = new Settings
            {
                ZoomLevel = zoomLevel,
                YCoord = changedPoint.Y,
                XCoord = changedPoint.X
            };

            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(changedSettings));
        }

        private void Btn_ClickAddNewAlarm(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mainModel.Rule.SensorName) == false &&
                string.IsNullOrEmpty(mainModel.Rule.MeasurementType) == false &&
                string.IsNullOrEmpty(mainModel.Rule.Sign) == false)
            {
                alarms.Add(new AlarmRule(mainModel.Rule.SensorName,
                                           mainModel.Rule.MeasurementType,
                                           mainModel.Rule.Sign,
                                           mainModel.Rule.Value));

            }

            File.WriteAllText("AlarmRules.json", JsonConvert.SerializeObject(alarms));
            lstDefinedAlarms.Items.Refresh();
        }

        private void Btn_ClickDeleteDefinedAlarm(object sender, System.Windows.RoutedEventArgs e)
        {
            AlarmRule selectedRule = ((Button)sender).DataContext as AlarmRule;
            if (alarms.Contains(selectedRule) == true)
            {
                if (System.Windows.MessageBox.Show("Are you sure you want do delete the selected alarm rule?", "Confirmation",
                    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    alarms.Remove(selectedRule);
                    File.WriteAllText("AlarmRules.json", JsonConvert.SerializeObject(alarms));
                    lstDefinedAlarms.Items.Refresh();
                }
            }
        }
    }
}
