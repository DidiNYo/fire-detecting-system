using ExternalServices;
using ExternalServices.Models;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace fire_detecting_system
{
    public class SensorsLocations
    {
        public SensorsLocations(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        //Creates the map with the layers needed.
        private Map CreateMap()
        {
            Map map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateSensorsLayer());
            
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Centroid, map.Resolutions[10]); 
            return map;
        }

        //Creates a layer with the sensors.
        private MemoryLayer CreateSensorsLayer()
        {
            return new MemoryLayer
            {
                Name = "Sensors",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetSensorsFromAPI()),
                Style = CreateBitmapStyle()
            };
        }

      //  APIService APIConnection;
        //This method gets the sensors data from the API. 
        private IEnumerable<IFeature> GetSensorsFromAPI()
        {
            APIService APIConnection = new APIService();
            List<OrganizationItem> sensors = Task.Run(() => APIConnection.GetOrganizationItems()).Result;


            return sensors.Select(s =>
            {
                Feature feature = new Feature();
                double longitude = double.Parse(s.Properties.Find(p => p.Type == "Longitude").Value, CultureInfo.InvariantCulture);
                double latitude = double.Parse(s.Properties.Find(p => p.Type == "Latitude").Value, CultureInfo.InvariantCulture);
                Point point = SphericalMercator.FromLonLat(longitude, latitude);
                feature.Geometry = point;

                return feature;
            });
        }

        private SymbolStyle CreateBitmapStyle()
        {
            string path = "fire_detecting_system.Resources.pin.png"; //Image file. Embedded resource.
            int bitmapId = GetBitmapIdForEmbeddedResource(path);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 1, SymbolOffset = new Offset(0, 0) }; //Setings of the image.
        } 

        private int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream image = assembly.GetManifestResourceStream(imagePath);
            
            return BitmapRegistry.Instance.Register(image);
        }
    }
}
