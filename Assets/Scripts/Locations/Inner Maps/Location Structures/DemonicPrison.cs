﻿using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class DemonicPrison : DemonicStructure {
        
        public override Vector2 selectableSize { get; }
        public DemonicPrison(Region location) : base(STRUCTURE_TYPE.DEMONIC_PRISON, location){
            selectableSize = new Vector2(10f, 10f);
        }
        public DemonicPrison(Region location, SaveDataLocationStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
        }
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        #endregion
        
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                character.trapStructure.SetForcedStructure(this);
                character.DecreaseCanTakeJobs();
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                character.trapStructure.SetForcedStructure(null);
                character.IncreaseCanTakeJobs();
            }
        }
    }
}