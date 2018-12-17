using BruTile.Predefined;
using fire_detecting_system.Models;
using Mapsui.Layers;
using Mapsui.Projection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace fire_detecting_system
{
    public partial class MainWindow 
    {
        MainViewModel mainModel = new MainViewModel(); 

        public MainWindow()
        {
            InitializeComponent();

            //Example with cities.
            SensorsPoints points = new SensorsPoints();

            //MainMap - name of the map. Defined in MainWindow.xaml
            points.Setup(MainMap);

            //Set mainModel to be source for the DataContext property
            DataContext = mainModel;
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            var xCoord = Convert.ToDouble(mainModel.Coords.XCoordinate);
            var yCoord = Convert.ToDouble(mainModel.Coords.YCoordinate);

            MainMap.Map.Transformation = new MinimalTransformation();

            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            var point = (Mapsui.Geometries.Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Mapsui.Geometries.Point(xCoord,yCoord)));

            //Map is centered with coordinates provided by the user
            MainMap.Navigator.NavigateTo(new Mapsui.Geometries.Point(point.X, point.Y), 19);
        }


    }


}
