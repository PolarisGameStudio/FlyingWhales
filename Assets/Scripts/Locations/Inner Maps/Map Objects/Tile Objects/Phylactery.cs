﻿public class Phylactery : TileObject {
    
    public Phylactery() {
        Initialize(TILE_OBJECT_TYPE.PHYLACTERY, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
    }
    public Phylactery(SaveDataTileObject data) { }
    protected override string GenerateName() { return "Phylactery"; }
}