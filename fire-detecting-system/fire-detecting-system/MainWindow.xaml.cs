﻿using ExternalServices;
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

        int numberOfClicks;

        IFeature clickedFeature;

        int zoomLevel;

        Point centerPoint;

        List<string> names;

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
            centerPoint = SphericalMercator.FromLonLat(GetSettings.GetSettingsInstance.SettingsData.XCoord, GetSettings.GetSettingsInstance.SettingsData.YCoord);
            mainModel.Coords.XCoordinate = (GetSettings.GetSettingsInstance.SettingsData.XCoord).ToString(CultureInfo.InvariantCulture);
            mainModel.Coords.YCoordinate = (GetSettings.GetSettingsInstance.SettingsData.YCoord).ToString(CultureInfo.InvariantCulture);

            cmbBoxSign.ItemsSource = LoadComboBoxSign();
            cmbBoxZoomLevel.ItemsSource = LoadComboBoxZoomLevel();
        }

        private void OnUpdateCompleted(object sender, EventArgs e)
        {
            MainMap.RefreshData();
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await mainModel.Sensors.InitializeAsync(MainMap, mainModel.APIConnection);
            names = new List<string>();
            foreach (var item in mainModel.Sensors.Sensors)
            {
                names.Add(item.Name);
            }
            cmbBoxSensor.ItemsSource = names;
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

            MainMap.Navigator.NavigateTo(centerPoint, MainMap.Map.Resolutions[zoomLevel]);
           
            //Return to the first tab after changed
            MainTabs.SelectedIndex = 0;
        }

        private string[] LoadComboBoxZoomLevel()
        {
            ZoomLevel zoomLevels = new ZoomLevel();
            return zoomLevels.Levels;
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
