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
