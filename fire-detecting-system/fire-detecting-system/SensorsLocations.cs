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
using System.Threading.Tasks;


namespace fire_detecting_system
{
    public class SensorsLocations : ObservableObject
    {
        private Map map;

        public List<OrganizationItem> Sensors { get; private set; }

        public Dictionary<string, LastMeasurement> LastMeasurements { get; private set; }

        private Dictionary<string, Feature> features;

        private ILayer labelLayer;

        private int indexOfLabelStyle;

        private int indexOfSymbolStyle;

        public SensorsLocations()
        {
        }

        public async Task InitializeAsync(IMapControl mapControl, APIService APIConnection)
        {
            features = new Dictionary<string, Feature>();
            Sensors = await APIConnection.GetOrganizationItemsAsync();
            LastMeasurements = await APIConnection.GetLastMeasurementsAsync();

            map = new Map();
            mapControl.Map = CreateMap();

            indexOfLabelStyle = 0;
            indexOfSymbolStyle = 0;

            AddSensorsLayer();
            AddLabelLayer(LastMeasurements);
            InitializeImagesToCameraFeatures();
            CallGetLastMeasurements(APIConnection);
            await APIConnection.GetImagesAsync();
        }

        private void CallGetLastMeasurements(APIService APIConnection)
        {
            Task.Run(async () =>
              {
                  Dictionary<string, LastMeasurement> newMeasurements = new Dictionary<string, LastMeasurement>();
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
                feature.Styles.ElementAt(indexOfLabelStyle).Enabled = false;
                if(feature.Styles.Count > indexOfSymbolStyle)
                {
                    feature.Styles.ElementAt(indexOfSymbolStyle).Enabled = false;
                }               
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
                    feature.Styles.ElementAt(indexOfLabelStyle).Enabled = true;
                    if(feature.Styles.Count > indexOfSymbolStyle)
                    {
                        feature.Styles.ElementAt(indexOfSymbolStyle).Enabled = true;
                    }                   
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
                    indexOfLabelStyle = feature.Styles.Count - 1;
                    feature.Styles.ElementAt(indexOfLabelStyle).Enabled = false;
                    return feature;
                });
        }

        //Initialize images to the cameras.
        private void InitializeImagesToCameraFeatures()
        {
            List<OrganizationItem> cameras = Sensors.FindAll(s => s.TypeId == (int)APIService.Type.Camera);

            foreach (OrganizationItem item in cameras)
            {
                foreach (TagInfo tag in item.Tags)
                {
                    if (tag.Type == null)
                    {
                        features[item.Name].Styles.Add(AddImage(tag.TagId));
                        indexOfSymbolStyle = features[item.Name].Styles.Count - 1;
                        features[item.Name].Styles.ElementAt(indexOfSymbolStyle).Enabled = false;
                    }
                }
            }
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

        // don't use an embedded resource (don't use assembly)
        private static IStyle AddImage(int id)
        {
            string path = "..\\..\\Assets\\" + id + ".jpg";
            int bitmapId = GetBitmapIdForEmbeddedResource(path);

            return new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolScale = 0.345f,
                SymbolOffset = new Offset(400, -524),
                SymbolType = SymbolType.Bitmap
            };
        }

        // get image
        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {         
            if (File.Exists(imagePath))
            {
                MemoryStream imageStream = new MemoryStream();
                using (FileStream image = File.Open(imagePath, FileMode.Open))
                {
                    image.CopyTo(imageStream);
                }
                return BitmapRegistry.Instance.Register(imageStream);
            }

            return 0;
        }
    }
}
