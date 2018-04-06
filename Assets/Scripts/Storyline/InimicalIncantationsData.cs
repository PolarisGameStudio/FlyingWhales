﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class InimicalIncantationsData : StorylineData {

	private TheDarkRitual _darkRitualQuest;
	private List<Item> _booksOfInimicalIncantations;

	#region getters/setters
	public TheDarkRitual darkRitualQuest{
		get { return _darkRitualQuest; }
	}
	#endregion

	public InimicalIncantationsData() : base(STORYLINE.INIMICAL_INCANTATIONS) {

	}

	#region overrides
	public override bool InitialStorylineSetup() {
		_booksOfInimicalIncantations = new List<Item> ();

		//Check if there is a Ritual Stone in the World
		bool hasRitualStone = false;
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region region = GridMap.Instance.allRegions [i];
			for (int j = 0; j < region.landmarks.Count; j++) {
				BaseLandmark landmark = region.landmarks [j];
				if(landmark.specificLandmarkType == LANDMARK_TYPE.RITUAL_STONES){
					hasRitualStone = true;
				}
			}
		}
		if(!hasRitualStone){
			return false;
		}

		//Get All Ancient Ruins and Caves
		List<DungeonLandmark> ancientRuins = new List<DungeonLandmark>();
		List<DungeonLandmark> caves = new List<DungeonLandmark>();

		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region region = GridMap.Instance.allRegions [i];
			for (int j = 0; j < region.landmarks.Count; j++) {
				BaseLandmark landmark = region.landmarks [j];
				if(landmark is DungeonLandmark){
					if(landmark.specificLandmarkType == LANDMARK_TYPE.CAVE){
						caves.Add ((DungeonLandmark)landmark);
					}else if(landmark.specificLandmarkType == LANDMARK_TYPE.ANCIENT_RUIN){
						ancientRuins.Add ((DungeonLandmark)landmark);
					}
				}
			}
		}

		//Spawn 3 Books of Inimical Incantations in 3 Random Ancient Ruins and Spawn Neuroctus Plant in Caves
		if(ancientRuins.Count > 0){

			Messenger.AddListener<ECS.Item, BaseLandmark>(Signals.ITEM_PLACED_LANDMARK, OnBookPlacedInLandmark);
			Messenger.AddListener<ECS.Item, ECS.Character>(Signals.ITEM_PLACED_INVENTORY, OnBookPlacedInInventory);

			for (int i = 0; i < 3; i++) {
				int index = UnityEngine.Random.Range (0, ancientRuins.Count);
				DungeonLandmark chosenAncientRuin = ancientRuins [index];
				Item bookOfInimicalIncantations = ItemManager.Instance.CreateNewItemInstance("Book of Inimical Incantations");
				_booksOfInimicalIncantations.Add (bookOfInimicalIncantations);
				AddRelevantItem(bookOfInimicalIncantations, CreateLogForStoryline("book_description"));
				chosenAncientRuin.AddItemInLandmark (bookOfInimicalIncantations);
				ancientRuins.RemoveAt (index);
				if(ancientRuins.Count <= 0){
					break;
				}
			}

			for (int i = 0; i < caves.Count; i++) {
				caves [i].SpawnItemInLandmark ("Neuroctus");
			}
		}else{
			return false;
		}

		AddRelevantItem("Ritual Stones", CreateLogForStoryline("ritual_stones"));
		AddRelevantItem("Meteor Strike", CreateLogForStoryline("meteor_strike"));

		_storylineDescription = CreateLogForStoryline("description");
		//Create Dark Ritual Quest
		_darkRitualQuest = new TheDarkRitual(QuestManager.Instance);
		QuestManager.Instance.AddQuestToAvailableQuests(_darkRitualQuest);
		AddRelevantQuest(_darkRitualQuest);
		return true;
	}
	#endregion

	#region Logs
	protected override Log CreateLogForStoryline(string key) {
		return new Log(GameManager.Instance.Today(), "Storylines", "InimicalIncantations", key);
	}
	private void OnBookPlacedInLandmark(ECS.Item item, BaseLandmark landmark) {
		if (_booksOfInimicalIncantations.Contains(item)) {
			Log changeLocation = CreateLogForStoryline("landmark_possession");
			changeLocation.AddToFillers(landmark, landmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
			ReplaceItemLog(item, changeLocation, 1);
		}
	}
	private void OnBookPlacedInInventory(ECS.Item item, ECS.Character character) {
		if (_booksOfInimicalIncantations.Contains(item)) {
			Log changeLocation = CreateLogForStoryline("character_possession");
			changeLocation.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			ReplaceItemLog(item, changeLocation, 1);
		}
	}
	#endregion
}