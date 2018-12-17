using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    //MainModel is class that: 
    //           - we use for binding data
    //           - helps us to have access to different models at the same time
    public class MainModel
    {
        public Coordinates Coords { get; private set; }

        public MainModel()
        {
            Coords = new Coordinates();
        }
    }
}
