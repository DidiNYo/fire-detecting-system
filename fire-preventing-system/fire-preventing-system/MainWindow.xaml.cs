using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projection;
using System.Windows;

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
        }

        private void Btn_ClickSaveCoords(object sender, System.Windows.RoutedEventArgs e)
        {
            //We need to transfer from one coordinate reference system to another - from CRS to CRS
            MainMap.Map.Transformation = new MinimalTransformation();

            //Mapsui.Geometries.Point(x, y); where x is the X-axis(E-coord) and y is the Y-axis(N-coord)
            var point =  (Mapsui.Geometries.Point)(MainMap.Map.Transformation.Transform("EPSG:4326", "EPSG:3857", new Mapsui.Geometries.Point(31.00, 30.00)));

            //The geographical center of the Earth is the Great Pyramid of Gizeh with coordinates 30°00′N 31°00′E
            MainMap.Navigator.CenterOn(new Mapsui.Geometries.Point(point.X, point.Y));
        }
    }
}
