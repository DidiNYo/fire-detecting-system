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
            MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
        }
    }
}
