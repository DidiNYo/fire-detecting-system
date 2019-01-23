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

        private List<IFeature> features;

        public SensorsLocations(IMapControl mapControl)
        {
            APIConnection = new APIService();
            features = new List<IFeature>();
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
                features.Add(new Feature(feature));
                return feature;
            });
        }

        public void AddLabelsLayer(IFeature clickedFeature)
        {           
            if (clickedFeature != null)
            {
                CreateLabelLayer(clickedFeature);
                map.Layers.Add(LabelLayer);
            }

        }

        public void RemoveLabelLayer()
        {
            if (LabelLayer != null)
            {
                map.Layers.Remove(LabelLayer);
            }
        }

        //Creates a layer with labels
        private void CreateLabelLayer(IFeature clickedFeature)
        {
            if(clickedFeature != null)
            {
                LabelLayer = new MemoryLayer
                {
                    Name = "Labels",
                    IsMapInfoLayer = true,
                    DataSource = new MemoryProvider(GetLabels(clickedFeature)),
                    Style = null
                };
            }
        }

        // Initialize the labels.
        private IFeature GetLabels(IFeature clickedFeature)
        {
            if(clickedFeature != null)
            {
                Point clickedPoint = (Point)clickedFeature.Geometry;
                foreach (Feature feature in features)
                {
                    Point currentPoint = (Point)feature.Geometry;
                    if (clickedPoint.X == currentPoint.X && clickedPoint.Y == currentPoint.Y)
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
                        return feature;
                    }
                }
            }
            return null;
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
