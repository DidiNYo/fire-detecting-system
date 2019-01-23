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
using System;
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
        private APIService APIConnection;

        private Map map;

        private List<OrganizationItem> sensors;

        private MemoryLayer LabelLayer;      

        public SensorsLocations(IMapControl mapControl)
        {
            APIConnection = new APIService();
            sensors = Task.Run(() => APIConnection.GetOrganizationItemsAsync()).Result;
            map = new Map();
            mapControl.Map = CreateMap();
            AddSensorsLayer();
        }

        //Creates the main map
        private Map CreateMap()
        {
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Centroid, map.Resolutions[10]);
            return map;
        }

        //Adds layer with the sensors
        private void AddSensorsLayer()
        {
            map.Layers.Add(CreateSensorsLayer());
        }

        public void AddLabelsLayer(Point point)
        {        
            CreateLabelLayer(point);
            map.Layers.Add(LabelLayer);
        }

        public void RemoveLabelLayer()
        {
            map.Layers.Remove(LabelLayer);
        }

        //Creates a layer with the sensors.
        private MemoryLayer CreateSensorsLayer()
        {
            return new MemoryLayer
            {
                Name = "Sensors",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetSensors()),
                Style = null
            };
        }

        //Creates a layer with labels
        private void CreateLabelLayer(Point point)
        {
            LabelLayer = new MemoryLayer
            {
                Name = "Labels",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetLabels(point)),
                Style = null
            };
        }

        // Initialize the labels.
        private IEnumerable<IFeature> GetLabels(Point clickedPoint)
        {           
            return sensors.Select(s =>
            {
                Feature feature = new Feature();
                double longitude = double.Parse(s.Properties.Find(p => p.Type == "Longitude").Value, CultureInfo.InvariantCulture);
                double latitude = double.Parse(s.Properties.Find(p => p.Type == "Latitude").Value, CultureInfo.InvariantCulture);
                Point point = SphericalMercator.FromLonLat(longitude, latitude);
                feature.Geometry = point;
                LabelStyle label = new LabelStyle
                {
                    Text = "Some random example text",
                    BackColor = new Brush(Color.Gray),
                    ForeColor = Color.Black,
                    MaxWidth = 10,
                    WordWrap = LabelStyle.LineBreakMode.WordWrap,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    Offset = new Offset(0, -40)
                };
                feature.Styles.Add(label);
                
                return feature;
            });

        }

        //The sensor position is marked with small red dot.
        private static IStyle SmallRedDot()
        {
            return new SymbolStyle {
                SymbolScale = 0.2,
                Fill = new Brush { Color = Color.Red }
            };
        }

        //Adding red outline to the dot.
        private static IStyle RedOutline()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.5f,
                Fill = null,
                Outline = new Pen { Color = Color.Red }
            };
        }

        //Initialize the sensors.
        private IEnumerable<IFeature> GetSensors()
        {
            return sensors.Select(s =>
            {
                Feature feature = new Feature();
                double longitude = double.Parse(s.Properties.Find(p => p.Type == "Longitude").Value, CultureInfo.InvariantCulture);
                double latitude = double.Parse(s.Properties.Find(p => p.Type == "Latitude").Value, CultureInfo.InvariantCulture);
                Point point = SphericalMercator.FromLonLat(longitude, latitude);
                feature.Geometry = point;
                feature.Styles.Add(SmallRedDot());
                feature.Styles.Add(RedOutline());
                return feature;
            });
        }
    }
}
