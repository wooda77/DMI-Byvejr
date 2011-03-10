// 
// AllCities.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Collections.Generic;

namespace DMI.Models
{
    public class AllCities : List<CityGroup>
    {
        private static readonly string Groups = "#abcdefghijklmnoprstuvæøå";

        public AllCities(List<City> cities)
        {
            cities.Sort();

            var groups = new Dictionary<string, CityGroup>();

            foreach (char c in Groups)
            {
                var group = new CityGroup(c.ToString());                
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (City city in cities)
            {
                groups[char.ToLower(city.Name[0]).ToString()].Add(city);
            }
        }
    }
}
