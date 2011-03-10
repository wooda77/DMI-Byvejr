// 
// BingLocationResponse.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Runtime.Serialization;

namespace DMI.Models
{
    public class BingLocationResponse
    {
        public ResourceSet[] resourceSets
        {
            get;
            set;
        }

        public string statusCode
        {
            get;
            set;
        }

        public string statusDescription
        {
            get;
            set;
        }

        public class ResourceSet
        {
            public Resource[] resources
            {
                get;
                set;
            }

            public class Resource
            {
                public Address address
                {
                    get;
                    set;
                }

                public class Address
                {
                    public string addressLine
                    {
                        get;
                        set;
                    }

                    public string adminDistrict
                    {
                        get;
                        set;
                    }

                    public string adminDistrict2
                    {
                        get;
                        set;
                    }

                    public string countryRegion
                    {
                        get;
                        set;
                    }

                    public string formattedAddress
                    {
                        get;
                        set;
                    }

                    public string locality
                    {
                        get;
                        set;
                    }

                    public string postalCode
                    {
                        get;
                        set;
                    }
                }
            }
        }
    }
}