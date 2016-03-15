using System;

namespace WordChainGame
{
    class City
    { // Структура данных для хранения города
        public string Name { get; private set; }
        public string Region { get; private set; }
        public string Country { get; private set; }
        public string Id { get; private set; }
        public string Disable { get; private set; }
        public string First { get; private set; }
        public override string ToString()
        {
            return String.Format("City: {0}, Region: {1}, Country: {2}", Name, Region, Country);
        }
        public City(string name, string region, string country, string id, string disable, string first)
        {
            Name = name;
            Region = region;
            Country = country;
            Id = id;
            Disable = disable;
            First = first;
        }
    }
}
