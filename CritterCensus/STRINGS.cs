namespace CritterCensus;

// All user-visible text. Translations are loaded from translations/<language code>.po (see CritterCensusMod).
public static class STRINGS
{
    public static class UI
    {
        public static LocString TOOL_NAME = "Critter Census";
        public static LocString TOOL_TOOLTIP = "Drag to list the critters and eggs in an area";
        public static LocString TITLE = "Critter Census";
        public static LocString EMPTY = "No critters or eggs in the selected area.";

        public static class FILTERS
        {
            public static LocString STAGE = "Stage";
            public static LocString GENUS = "Genus";
            public static LocString ALL = "All";
        }

        public static class STAGES
        {
            public static LocString ADULT = "Adult";
            public static LocString BABY = "Baby";
            public static LocString EGG = "Egg";
        }

        public static class ORIGINS
        {
            public static LocString WILD = "Wild";
            public static LocString TAME = "Tame";
        }

        public static class COLUMNS
        {
            public static LocString STAGE = "Stage";
            public static LocString CRITTER = "Critter";
            public static LocString ORIGIN = "Origin";
            public static LocString AGE = "Age / lifespan";
            public static LocString HAPPINESS = "Happiness";
            public static LocString FERTILITY = "Fertility";
            public static LocString FULLNESS = "Fullness";
            public static LocString FUR = "Fur growth";
            public static LocString TEMPERATURE = "Temperature";
            public static LocString AILMENTS = "Ailments";
        }

        public static class FORMATS
        {
            // {0}: number of cycles
            public static LocString CYCLES = "{0}cyc";

            // {0}: a cycles value
            public static LocString LEFT = "({0} left)";
        }

        public static class TOOLTIPS
        {
            public static LocString EGG_CHANCES = "Egg chances";
            public static LocString HAPPINESS = "Happiness: {0}";
            public static LocString CALORIES = "Calories: {0} / {1}";
            public static LocString WILDNESS = "Wildness: {0} / {1} ({2}%)";
            public static LocString WILDNESS_CHANGE = "Change: {0} per cycle";
        }
    }
}
