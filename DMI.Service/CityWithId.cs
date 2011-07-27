using System;

namespace DMI.Service
{
    public class CityWithId
    {
        public CityWithId()
        {            
        }

        public CityWithId(int postalCode, string name, int id)
        {
            this.PostalCode = postalCode;
            this.Name = name;
            this.Id = id;            
        }

        public int PostalCode
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }
}
