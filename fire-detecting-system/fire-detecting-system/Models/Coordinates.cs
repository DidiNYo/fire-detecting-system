using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    public class Coordinates : ObservableObject
    {
        private string x_coordinate;
        private string y_coordinate;

        public string X_coordinate
        {
            get
            {
                return x_coordinate;
            }
            set
            {
                x_coordinate = value;
                OnPrepertyChanged("X_coordinate");
            }
        }

        public string Y_coordinate
        {
            get
            {
                return y_coordinate;
            }
            set
            {
                y_coordinate = value;
                OnPrepertyChanged("Y_coordinate");
            }
        }
    }
}
