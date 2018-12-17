using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    public class Coordinates : ObservableObject
    {
        private string xCoordinate;
        private string yCoordinate;

        public string XCoordinate
        {
            get
            {
                return xCoordinate;
            }
            set
            {
                xCoordinate = value;
                OnPrоpertyChanged();
            }
        }

        public string YCoordinate
        {
            get
            {
                return yCoordinate;
            }
            set
            {
                yCoordinate = value;
                OnPrоpertyChanged();
            }
        }
    }
}
