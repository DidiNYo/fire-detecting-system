using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fire_detecting_system.Models
{
    public class ZoomLevel : ObservableObject
    {
        private string level;

        public string Level
        {
            get => level;
            set
            {
                level = value;
                OnPrоpertyChanged();
            }
        }

    }
}
