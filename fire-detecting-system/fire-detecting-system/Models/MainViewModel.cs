using ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    //MainViewModel is class that: 
    //           - we use for binding data
    //           - helps us to have access to different models at the same time
    public class MainViewModel
    {
        public APIService APIConnection { get; private set; }

        public Coordinates Coords { get; private set; }

        public ZoomLevel Zoom { get; private set; }

        public SensorsLocations Sensors { get; set; }

        public MainViewModel()
        {
            APIConnection = new APIService();
            Coords = new Coordinates();
            Zoom = new ZoomLevel();
        }
    }
}
