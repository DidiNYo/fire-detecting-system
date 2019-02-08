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

        public string[] Levels { get; }

        public string Level
        {
            get => level;
            set
            {
                level = value;
                OnPrоpertyChanged();
            }
        }

        public ZoomLevel()
        {
            Levels = new string[19];

            for (int i = 0; i < 19; i++)
            {
                Levels[i] = (i + 1).ToString();
            }
        }

    }
}
