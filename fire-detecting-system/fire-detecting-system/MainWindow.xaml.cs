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
        MainModel main_model = new MainModel(); 

        public MainWindow()
        {
            InitializeComponent();

            //Example with cities.
            SensorsPoints points = new SensorsPoints();

            //MainMap - name of the map. Defined in MainWindow.xaml
            points.Setup(MainMap);

            //Set main_model to be source for the DataContext property
            DataContext = main_model;
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            var x_coord = Convert.ToDouble(main_model.Coords.X_coordinate);
            var y_coord = Convert.ToDouble(main_model.Coords.Y_coordinate);

            MainMap.Map.Transformation = new MinimalTransformation();

            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            var point = (Mapsui.Geometries.Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Mapsui.Geometries.Point(x_coord,y_coord)));

            //Map is centered with coordinates provided by the user
            MainMap.Navigator.NavigateTo(new Mapsui.Geometries.Point(point.X, point.Y), 19);
        }


    }


}
