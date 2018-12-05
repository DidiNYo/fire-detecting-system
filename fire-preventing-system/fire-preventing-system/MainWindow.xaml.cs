using BruTile.Predefined;
using Mapsui.Layers;

namespace fire_preventing_system
{
    public partial class MainWindow 
    {
        public MainWindow()
        {           
            InitializeComponent();
            //Initialize map
            SensorsPoints point = new SensorsPoints();
            point.Setup(MyMapControl);

            //MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
        }
    }
}
