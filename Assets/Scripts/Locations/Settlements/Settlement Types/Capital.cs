﻿using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class Capital : SettlementType {
        public Capital() : base(SETTLEMENT_TYPE.Capital) {
            maxDwellings = 20;
            maxFacilities = 12;
        }
        public Capital(SaveDataSettlementType data) : base(data) {
            maxDwellings = 20;
            maxFacilities = 12;
        }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 50, 2);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 30, 3);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), 20, 2);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 20, 2);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.STONE), 30, 2);
        }
    }
}