﻿namespace Inner_Maps.Location_Structures {
    public class Granary : ManMadeStructure {
        public Granary(Region location) : base(STRUCTURE_TYPE.GRANARY, location) {
            SetMaxHPAndReset(8000);
        }
        public Granary(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}