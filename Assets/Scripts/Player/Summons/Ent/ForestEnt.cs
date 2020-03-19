﻿public class ForestEnt : Ent {
    
    public ForestEnt() : base(SUMMON_TYPE.Forest_Ent, "Forest Ent") { }
    public ForestEnt(string className) : base(SUMMON_TYPE.Forest_Ent, className) { }
    public ForestEnt(SaveDataCharacter data) : base(data) { }
}