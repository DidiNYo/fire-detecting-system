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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fire_detecting_system
{
    public class SensorsLocations : ObservableObject
    {
        private Map map;

        public List<OrganizationItem> Sensors { get; private set; }

        private Dictionary<string, Feature> features;

        private ILayer labelLayer;

        public SensorsLocations()
        {

        }

        public async Task InitializeAsync(IMapControl mapControl, APIService APIConnection)
        {
            features = new Dictionary<string, Feature>();
            Sensors = await APIConnection.GetOrganizationItemsAsync();
            Dictionary<string, LastMeasurement> lastMeasurements = await APIConnection.GetLastMeasurementsAsync();
            map = new Map();
            mapControl.Map = CreateMap();
            AddSensorsLayer();
            AddLabelLayer(lastMeasurements);
            CallGetLastMeasurements(APIConnection);
        }

        private void CallGetLastMeasurements(APIService APIConnection)
        {
            Task.Run(async () =>
              {
                  var newMeasurements = new Dictionary<string, LastMeasurement>();
                  while (true)
                  {
                      newMeasurements = await APIConnection.GetLastMeasurementsAsync();
                      UpdateLabels(newMeasurements.Values);
                      await Task.Delay(GetConfiguration.ConfigurationInstance.ConfigurationData.SecondsToRefresh);
                  }
              });
        }

        public event EventHandler OnUpdateCompleted;

        public void UpdateLabels(IEnumerable<LastMeasurement> measurements)
        {
            foreach (LastMeasurement measurement in measurements)
            {
                if (features.ContainsKey(measurement.OrganizationItemName))
                {
                    Feature currentFeature = features[measurement.OrganizationItemName];
                    (currentFeature.Styles.Last() as LabelStyle).Text = measurement.ToString();
                }
            }
            OnUpdateCompleted?.Invoke(this, new EventArgs());
        }

        //Creates the main map
        private Map CreateMap()
        {
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            double xCoord = GetSettings.GetSettingsInstance.SettingsData.XCoord;
            double yCoord = GetSettings.GetSettingsInstance.SettingsData.YCoord;
            Point center = SphericalMercator.FromLonLat(xCoord, yCoord);
            map.Home = n => n.NavigateTo(center, map.Resolutions[GetSettings.GetSettingsInstance.SettingsData.ZoomLevel]);
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
            return Sensors.Select(s =>
            {
                Feature feature = new Feature();
                double longitude = double.Parse(s.Properties.Find(p => p.Type == "Longitude").Value, CultureInfo.InvariantCulture);
                double latitude = double.Parse(s.Properties.Find(p => p.Type == "Latitude").Value, CultureInfo.InvariantCulture);
                Point point = SphericalMercator.FromLonLat(longitude, latitude);
                feature["Name"] = s.Name;
                feature.Geometry = point;
                feature.Styles.Add(SmallRedDot());
                feature.Styles.Add(RedOutline());
                features.Add(s.Name, new Feature(feature));
                return feature;
            });
        }

        private void AddLabelLayer(Dictionary<string, LastMeasurement> lastMeasurements)
        {
            labelLayer = CreateLabelLayer(lastMeasurements);
            map.Layers.Add(labelLayer);
        }


        //Creates a layer with labels
        private MemoryLayer CreateLabelLayer(Dictionary<string, LastMeasurement> lastMeasurements)
        {
            return new MemoryLayer
            {
                Name = "Labels",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(InitializeLabels(lastMeasurements)),
                Style = null
            };
        }

        //Remove label from clicked feature
        public void HideLabel(IFeature clickedFeature)
        {
            Feature feature = features[clickedFeature["Name"] as string];
            if (feature != null)
            {
                feature.Styles.Last().Enabled = false;
            }
        }

        //Show label 
        public void DisplayLabel(IFeature clickedFeature)
        {
            if (clickedFeature != null)
            {
                Feature feature = features[clickedFeature["Name"] as string];
                if (feature != null)
                {
                    feature.Styles.Last().Enabled = true;
                }
            }
        }

        // Initialize the labels.
        private IEnumerable<IFeature> InitializeLabels(Dictionary<string, LastMeasurement> lastMeasurements)
        {
            int i = 0;
            return features.Values.Select(feature =>
                {
                    LabelStyle label = new LabelStyle
                    {
                        Text = lastMeasurements[Sensors.ElementAt(i++).Name].ToString(),
                        Font = new Font { FontFamily = "Arial", Size = 13 },
                        BackColor = new Brush(Color.Black),
                        ForeColor = Color.White,
                        Opacity = 50,
                        MaxWidth = 70,
                        LineHeight = 1.2,
                        WordWrap = LabelStyle.LineBreakMode.NoWrap,
                        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
                        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
                        Offset = new Offset(20, 0)
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
