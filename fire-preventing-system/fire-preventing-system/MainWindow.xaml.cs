using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace fire_preventing_system
{
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();

            //Example with cities.
            SensorsPoints points = new SensorsPoints();

            //MainMap - name of the map. Defined in MainWindow.xaml
            points.Setup(MainMap);

            Mapsui.Geometries.Point point = new Mapsui.Geometries.Point();
            DataContext = point;
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            //We need to cast the sender as a XAML button element to get acces to its attributes
            var userCoordinates = sender as Button;
            var y_coord = Convert.ToDouble(userCoordinates.DataContext.ToString());
            var x_coord = Convert.ToDouble(userCoordinates.Tag.ToString());


            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            MainMap.Map.Transformation = new MinimalTransformation();

            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            var point =  (Mapsui.Geometries.Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Mapsui.Geometries.Point(x_coord,y_coord)));

            //Map is centered with coordinates provided by the user
            MainMap.Navigator.CenterOn(new Mapsui.Geometries.Point(point.X, point.Y));
        }


    }


}
