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

        SensorsLocations sensors;

        int numberOfClicks;

        IFeature clickedFeature;

        int zoomLevel;

        Point centerPoint;

        public MainWindow()
        {
            InitializeComponent();

            mainModel = new MainViewModel();

            //Visualize the sensors on the map.
            sensors = new SensorsLocations(MainMap, mainModel.APIConnection);

            //For labels.
            numberOfClicks = 0;

            //Set mainModel to be source for the DataContext property          
            DataContext = mainModel;

            //Subscribe for clicked left mouse button event
            MainMap.Info += MaiMaplOnInfo;

            //Default zoom level
            zoomLevel = GetConfiguration.ConfigurationInstance.ConfigurationData.ZoomLevel;

            //For testing.
            APIService APIConnection = new APIService();
        }

        //Show label on clicked sensor
        private void MaiMaplOnInfo(object sender, MapInfoEventArgs args)
        {
            ++numberOfClicks;
            if (numberOfClicks == 1)
            {
                //2. Remove it.
                if(clickedFeature != null)
                {
                    sensors.HideLabel(clickedFeature);
                }
                sensors.DisplayLabel(args.MapInfo.Feature);
                clickedFeature = args.MapInfo.Feature;
            }
            if (numberOfClicks == 2)
            {
                if (clickedFeature != null)
                {
                    sensors.HideLabel(clickedFeature);
                    clickedFeature = null;
                    //1. If second click is on a new feature show label for it.
                    if (args.MapInfo.Feature != null)
                    {
                        sensors.DisplayLabel(args.MapInfo.Feature);
                        clickedFeature = args.MapInfo.Feature;
                    }
                }
                numberOfClicks = 0;
            }

        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            double xCoord = Convert.ToDouble(mainModel.Coords.XCoordinate, CultureInfo.InvariantCulture);
            double yCoord = Convert.ToDouble(mainModel.Coords.YCoordinate, CultureInfo.InvariantCulture);

            MainMap.Map.Transformation = new MinimalTransformation();

            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            centerPoint = (Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Point(xCoord, yCoord)));

            //Map is centered with coordinates provided by the user
            MainMap.Navigator.NavigateTo(new Point(centerPoint.X, centerPoint.Y), MainMap.Map.Resolutions[zoomLevel]);

            //Return to the first tab after changed
            MainTabs.SelectedIndex = 0;
        }

        private void Btn_ClickApplyZoomLevel(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomLevel = Convert.ToInt32(mainModel.Zoom.Level);
            
            if(centerPoint != null)
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
    }
}
