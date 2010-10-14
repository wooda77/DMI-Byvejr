//
// Resources.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//

namespace DMI.Models
{
    using Properties;

    public class Resources
    {
        private static AppResources resources = new AppResources();

        public AppResources AppResources
        {
            get
            {
                return resources;
            }
        }
    }
}
