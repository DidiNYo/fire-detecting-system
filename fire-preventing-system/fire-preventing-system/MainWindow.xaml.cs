using BruTile.Predefined;
using Mapsui.Layers;

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
    }
}
