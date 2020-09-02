﻿public class IceNymph : Nymph {
    
    public IceNymph() : base(SUMMON_TYPE.Ice_Nymph, "Ice Nymph") { }
    public IceNymph(string className) : base(SUMMON_TYPE.Ice_Nymph, className) { }
    public IceNymph(SaveDataSummon data) : base(data) { }
}
