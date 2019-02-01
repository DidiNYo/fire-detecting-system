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
using System.Linq;
using System.Threading.Tasks;

namespace fire_detecting_system
{
    public class SensorsLocations
    {
        private Map map;

        private List<OrganizationItem> sensors;

        private Dictionary<Point, Feature> features;

        public SensorsLocations(IMapControl mapControl, APIService APIConnection)
        {
            features = new Dictionary<Point, Feature>();
            sensors = Task.Run(() => APIConnection.GetOrganizationItemsAsync()).Result;
            map = new Map();
            mapControl.Map = CreateMap();
            AddSensorsLayer();
            AddLabelLayer();
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

        //Creates a layer with the sensors.
        private MemoryLayer CreateSensorsLayer()
        {
            return new MemoryLayer
            {
                Name = "Sensors",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(InitializeSensors()),
                Style = null
            };
        }

        //Initialize the sensors.
        private IEnumerable<IFeature> InitializeSensors()
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
                features.Add(point, new Feature(feature));
                return feature;
            });
        }

        private void AddLabelLayer()
        {
            map.Layers.Add(CreateLabelLayer());
        }


        //Creates a layer with labels
        private MemoryLayer CreateLabelLayer()
        {
            return new MemoryLayer
            {
                Name = "Labels",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(InitializeLabels()),
                Style = null
            };
        }

        //Remove label from clicked feature
        public void HideLabel(IFeature clickedFeature)
        {
            Point clickedPoint = (Point)clickedFeature.Geometry;
            Feature feature = features[clickedPoint];
            if (feature != null)
            {
                feature.Styles.Last().Enabled = false;
            }
        }

        //Show label 
        public void DisplayLabel(IFeature clickedFeature)
        {
            if(clickedFeature != null)
            {
                Point clickedPoint = (Point)clickedFeature.Geometry;
                Feature feature = features[clickedPoint];
                if (feature != null)
                {
                    feature.Styles.Last().Enabled = true;
                }
            }
        }

        // Initialize the labels.
        private IEnumerable<IFeature> InitializeLabels()
        {
            return features.Values.Select(feature =>
                {
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
                    feature.Styles.Last().Enabled = false;
                    return feature;
                });
        }

        //The sensor position is marked with small red dot.
        private static IStyle SmallRedDot()
        {
            return new SymbolStyle
            {
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
    }
}
