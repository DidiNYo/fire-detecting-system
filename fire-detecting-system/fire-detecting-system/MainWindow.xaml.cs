using ExternalServices;
using ExternalServices.Models;
using fire_detecting_system.Models;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.UI;
using Mapsui.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace fire_detecting_system
{
    public partial class MainWindow
    {
        MainViewModel mainModel;

        //SensorsLocations sensors;

        int numberOfClicks;

        IFeature clickedFeature;

        int zoomLevel;

        Point centerPoint;

        public MainWindow()
        {
            InitializeComponent();

            mainModel = new MainViewModel();

            //Visualize the sensors on the map.
            Loaded += OnLoaded;

            //For labels.
            numberOfClicks = 0;

            //Set mainModel to be source for the DataContext property          
            DataContext = mainModel;

            //Subscribe for clicked left mouse button event
            MainMap.Info += MaiMaplOnInfo;

            //Default zoom level
            zoomLevel = GetSettings.GetSettingsInstance.SettingsData.ZoomLevel;
            mainModel.Zoom.Level = zoomLevel.ToString();

            //Default center point
            mainModel.Coords.XCoordinate = (GetSettings.GetSettingsInstance.SettingsData.XCoord).ToString(CultureInfo.InvariantCulture);
            mainModel.Coords.YCoordinate = (GetSettings.GetSettingsInstance.SettingsData.YCoord).ToString(CultureInfo.InvariantCulture);

            cmbBoxSensor.ItemsSource = LoadComboBoxSensorsNames();
            cmbBoxSign.ItemsSource = LoadComboBoxSign();
            cmbBoxZoomLevel.ItemsSource = LoadComboBoxZoomLevel();
        }

        private void OnUpdateCompleted(object sender, EventArgs e)
        {
            MainMap.RefreshData();
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            mainModel.Sensors = new SensorsLocations();
            await mainModel.Sensors.InitializeAsync(MainMap, mainModel.APIConnection);
            mainModel.Sensors.OnUpdateCompleted += OnUpdateCompleted;
        }

        //Show label on clicked sensor
        private void MaiMaplOnInfo(object sender, MapInfoEventArgs args)
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

                //Return to the first tab after changed
                MainTabs.SelectedIndex = 0;
            }
        }

        private void Btn_ClickApplyZoomLevel(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomLevel = Convert.ToInt32(mainModel.Zoom.Level);

            if (centerPoint != null)
            {
                MainMap.Navigator.NavigateTo(new Point(centerPoint.X, centerPoint.Y), MainMap.Map.Resolutions[zoomLevel]);
            }
            else
            {
                MainMap.Navigator.NavigateTo(MainMap.Map.Layers[1].Envelope.Centroid, MainMap.Map.Resolutions[zoomLevel]);
            }

            //Return to the first tab after changed
            MainTabs.SelectedIndex = 0;
        }

        private string[] LoadComboBoxZoomLevel()
        {
            ZoomLevel zoomLevels = new ZoomLevel();
            return zoomLevels.Levels;
        }

        //test
        private List<string> LoadComboBoxSensorsNames()
        {
            List<string> names = new List<string>();
            names.Add("Park Vitosha South");
            names.Add("Sensor Yarebkovitsa");
            names.Add("Chalin Valog T-1");
            names.Add("Sensor Lisets");
            names.Add("Chalin Valog CO2-1");
            names.Add("Portable Camera");
            names.Add("ASPires-Geo Camera");
            names.Add("ASPires-Geo Weather Station");
            names.Add("MqttTestSensor");
            names.Add("Chalin Valog DP-1");
            names.Add("Chalin Valog DP-2");
            names.Add("Chalin Valog DP-3");
            names.Add("Chalin Valog CO-1");
            names.Add("Park Bansko");
            names.Add("Fulda Camera");
            names.Add("Sensor Е0072632");
            names.Add("Sensor E0072715");
            names.Add("Sensor E0000712");
            names.Add("Sensor E0000713\r\n");
            return names;
        }

        private List<char> LoadComboBoxSign()
        {
            List<char> signs = new List<char>();
            signs.Add('>');
            signs.Add('<');
            return signs;
        }
    }
}
