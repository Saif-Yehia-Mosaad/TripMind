namespace TripMind.Application.DTOs.Ai
{
    public static class AiAllowedValues
    {
        public static readonly HashSet<string> Cities = new(StringComparer.OrdinalIgnoreCase)
        {
            "Cairo","Giza","Alexandria","Luxor","Aswan",
            "Sharm El Sheikh","Hurghada","Port Said",
            "Ismailia","Marsa Matrouh","Fayoum","Dahab"
        };

        public static readonly HashSet<string> DisplayInterests = new(StringComparer.Ordinal)
        {
            "Arts & Crafts","Bakery","Beaches & Water","Cafe","Entertainment",
            "History & Antiquities","Hotel","Mosques & Churches","Music","Nature",
            "Nightlife","Outdoor","Park","Restaurants","Seafood",
            "Shopping","Street Food","Tourism","Waterfront"
        };

        // food_cafes kept as a backward-compatible alias (older docs used it)
        public static readonly HashSet<string> PlaceCategorySlugs = new(StringComparer.OrdinalIgnoreCase)
        {
            "historical_sites","arts_culture","cafe","food","food_cafes",
            "beaches","shopping","nature","religious_sites","entertainment","hotel"
        };

        public static readonly HashSet<string> SortByValues = new(StringComparer.OrdinalIgnoreCase)
        {
            "rating","reviews","price","name"
        };

        public static readonly HashSet<string> OrderValues = new(StringComparer.OrdinalIgnoreCase)
        {
            "asc","desc"
        };
    }
}