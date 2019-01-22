using ExternalServices;
using ExternalServices.Models;
using fire_detecting_system.Models;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace fire_detecting_system
{
    public partial class MainWindow
    {
        MainViewModel mainModel;
        SensorsLocations sensors;
        int numberOfClicks;

        public MainWindow()
        {
            InitializeComponent();
            //Visualize the sensors on the map.
            sensors = new SensorsLocations(MainMap);
            
            //For labels.
            numberOfClicks = 0;

            //Set mainModel to be source for the DataContext property
            mainModel = new MainViewModel();
            DataContext = mainModel;

            //Subscribe for clicked left mouse button event
            MainMap.MouseLeftButtonDown += MapControlOnMouseLeftButtonDown;

            //For testing.
            APIService APIConnection = new APIService();
            List<LastMeasurement> lastValues = Task.Run(() => APIConnection.GetLastMeasurementsAsync()).Result;
        }
        
        //Show labels on click
        private void MapControlOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            Point clickedPoint = args.GetPosition(MainMap).ToMapsui();
            ++numberOfClicks;
            if(numberOfClicks == 1)
            {
                sensors.AddLabelsLayer(clickedPoint);
            }
            else
            {
                sensors.RemoveLabelLayer();
                numberOfClicks = 0;
            }                       
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            var xCoord = Convert.ToDouble(mainModel.Coords.XCoordinate, CultureInfo.InvariantCulture);
            var yCoord = Convert.ToDouble(mainModel.Coords.YCoordinate, CultureInfo.InvariantCulture);

            MainMap.Map.Transformation = new MinimalTransformation();

            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            var point = (Mapsui.Geometries.Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Mapsui.Geometries.Point(xCoord, yCoord)));

            //Map is centered with coordinates provided by the user
            MainMap.Navigator.NavigateTo(new Mapsui.Geometries.Point(point.X, point.Y), 19);
        }
    }
}
