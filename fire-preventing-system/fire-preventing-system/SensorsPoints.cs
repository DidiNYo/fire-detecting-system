using Mapsui;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fire_preventing_system
{
    class SensorsPoints
    {
        //Initialize the map. Map type is IMapControl
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        //Creates the map with the layers needed.
        private static Map CreateMap()
        {
            Map map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePointLayer());

            //Settings of the starting map position. I think.
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Centroid, map.Resolutions[5]); 
            return map;
        }

        //Creates a layer with cities.
        private static MemoryLayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
                Style = CreateBitmapStyle()
            };
        }


        //This method reads JSON objects from the test.json and calls the deserialize method.
        private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
        {
            //Path of the file. Need to be set to embedded resource. Name is "namespace-name.file-name.json"
            string path = "fire_preventing_system.Resources.test.json"; 
            //Assembly - takes the current project.
            Assembly assembly = Assembly.GetExecutingAssembly();
            //Creates a stream.
            Stream stream = assembly.GetManifestResourceStream(path);
            
            IEnumerable<City> cities = DeserializeFromStream<City>(stream);

            return cities.Select(c =>
            {
                var feature = new Feature();
                var point = SphericalMercator.FromLonLat(c.Lng, c.Lat);
                feature.Geometry = point;
                feature["name"] = c.Name;
                feature["country"] = c.Country;
                return feature;
            });
        }

        //This method deserialize the JSON objects from the file to objects readable from C# and returns a list of them.
        public static IEnumerable<T> DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<List<T>>(jsonTextReader);
            }
        }

        private static SymbolStyle CreateBitmapStyle()
        {
            string path = "fire_preventing_system.Resources.pin.png"; //Image file. Embedded resource again.
            int bitmapId = GetBitmapIdForEmbeddedResource(path);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 1, SymbolOffset = new Offset(0, 0) }; //Setings of the image.
        } 

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream image = assembly.GetManifestResourceStream(imagePath);
            
            return BitmapRegistry.Instance.Register(image);
        }
    }
}
