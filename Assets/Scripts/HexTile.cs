﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>{
	public int xCoordinate;
	public int yCoordinate;

	public string tileName;

	public float elevationNoise;
	public float moistureNoise;
	public float temperature;

	public RESOURCE defaultResource;
	public RESOURCE specialResource;

	public BIOMES biomeType;
	public ELEVATION elevationType;

	public int movementDays;

	public City city = null;
//	public Citizen occupant = null;
//	public ROLE roleIntendedForTile = ROLE.UNTRAINED;
//	public STRUCTURE structureOnTile = STRUCTURE.NONE;

	public bool isHabitable = false;
	public bool isRoad = false;
	public bool isOccupied = false;
	public bool isOwned = false;
	internal bool isBorder = false;
	internal int isBorderOfCityID = 0;
//	public GameObject topLeft, topRight, right, bottomRight, bottomLeft, left;

//	public GameObject leftGround;
//	public GameObject bottomLeftGround;
//	public GameObject rightGround;
//	public GameObject bottomRightGround;
	public GameObject centerPiece;
//	public GameObject topLeftGround;
//	public GameObject topRightGround;

	public GameObject leftBorder;
	public GameObject rightBorder;
	public GameObject topLeftBorder;
	public GameObject topRightBorder;
	public GameObject bottomLeftBorder;
	public GameObject bottomRightBorder;

	public GameObject resourceVisualGO;
	public GameObject structureGO;
	public Transform eventsParent;
	public GameObject cityNameGO;
	public TextMesh cityNameLbl;
	public SpriteRenderer kingdomColorSprite;
	public GameObject highlightGO;

	public List<HexTile> connectedTiles = new List<HexTile>();

	public IEnumerable<HexTile> AllNeighbours { get; set; }
	public IEnumerable<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN); } }
	public IEnumerable<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad); } }
	public IEnumerable<HexTile> PurchasableTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
	public IEnumerable<HexTile> CombatTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}

	public List<HexTile> elligibleNeighbourTilesForPurchase { get { return PurchasableTiles.Where(o => !o.isOwned && !o.isHabitable).ToList(); } } 

	private List<WorldEventItem> eventsOnTile = new List<WorldEventItem>();

	public int range = 0;
	List<HexTile> tiles = new List<HexTile> ();
//	public List<HexTile> allFoodNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => (o.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(o.defaultResource) == BASE_RESOURCE_TYPE.FOOD) || 
//				(Utilities.GetBaseResourceType(o.specialResource) == BASE_RESOURCE_TYPE.FOOD)).ToList(); 
//		}
//	}
//
//	public List<HexTile> allBaseResourceNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => (o.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(o.defaultResource) == city.kingdom.basicResource) || 
//				(Utilities.GetBaseResourceType(o.specialResource) == city.kingdom.basicResource)).ToList(); 
//		}
//	}
//
//	public List<HexTile> allNormalNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => o.specialResource == RESOURCE.NONE).ToList(); 
//		}
//	}

	[ContextMenu("Show Tiles In Range")]
	public void ShowTilesInRange(){
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].GetComponent<SpriteRenderer> ().color = Color.white;
		}
		tiles.Clear ();
		tiles.AddRange(this.GetTilesInRange (range));
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].GetComponent<SpriteRenderer> ().color = Color.magenta;
		}
	}

	[ContextMenu("Show Border Tiles")]
	public void ShowBorderTiles(){
		for (int i = 0; i < this.city.borderTiles.Count; i++) {
			this.city.borderTiles[i].GetComponent<SpriteRenderer> ().color = Color.magenta;
		}
	}
		
	[ContextMenu("Increase General HP")]
	public void IncreaseGeneralHP(){
		List<Citizen> generals = this.city.GetCitizensWithRole (ROLE.GENERAL);
		for (int i = 0; i < generals.Count; i++) {
			((General)generals[i].assignedRole).army.hp += 100;
			Debug.Log (((General)generals [i].assignedRole).citizen.name + " hp is " + ((General)generals [i].assignedRole).army.hp.ToString ());
		}
	}

	[ContextMenu("Show Citizen Creation Table")]
	public void ShowCitizenCreationTable(){
		Dictionary<ROLE, int> citizenCreationTable = this.city.citizenCreationTable;
		for (int i = 0; i < citizenCreationTable.Keys.Count; i++) {
			ROLE key = citizenCreationTable.Keys.ElementAt(i);
			Debug.Log (key.ToString () + " - " + citizenCreationTable [key].ToString ());
		}
	}

	void Start(){
//		EventManager.Instance.onGameEventEnded.AddListener(RemoveEvent);
//		EventManager.Instance.onShowEventsOfType.AddListener(ShowEventOnTile);
//		EventManager.Instance.onHideEvents.AddListener(HideEventsOnTile);
	}

	#region Resource
	internal void AssignDefaultResource(){
		if(elevationType == ELEVATION.MOUNTAIN){
			this.defaultResource = RESOURCE.GRANITE;
		}else{
			if (this.elevationType != ELEVATION.WATER) {
				switch (biomeType) {
				case BIOMES.BARE:
					this.defaultResource = RESOURCE.NONE;
					break;
				case BIOMES.DESERT:
					this.defaultResource = RESOURCE.GRANITE;
					break;
				case BIOMES.FOREST:
					this.defaultResource = RESOURCE.OAK;
					break;
				case BIOMES.GRASSLAND:
					this.defaultResource = RESOURCE.CORN;
					break;
				case BIOMES.SNOW:
					this.defaultResource = RESOURCE.NONE;
					break;
				case BIOMES.TUNDRA:
					this.defaultResource = RESOURCE.CORN;
					break;
				case BIOMES.WOODLAND:
					this.defaultResource = RESOURCE.CEDAR;
					break;

				}
			}
		}
	}
	internal void AssignSpecialResource(){
		int specialChance = UnityEngine.Random.Range (0, 100);

        int specialChanceForBiome = 0;

        if (this.biomeType == BIOMES.GRASSLAND || this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST) {
            specialChanceForBiome = 15;
        } else if (this.biomeType == BIOMES.DESERT) {
            specialChanceForBiome = 18;
        } else if (this.biomeType == BIOMES.TUNDRA || this.biomeType == BIOMES.SNOW) {
            specialChanceForBiome = 12;
        }

        if(specialChance < specialChanceForBiome) {
            if (this.elevationType != ELEVATION.WATER && this.elevationType != ELEVATION.MOUNTAIN) {
                this.specialResource = ComputeSpecialResource(Utilities.specialResourcesLookup[this.biomeType]);
                if (this.specialResource != RESOURCE.NONE) {
                    this.resourceVisualGO.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("Resources Icons")
                    .Where(x => x.name == this.specialResource.ToString()).ToList()[0];
                    this.resourceVisualGO.SetActive(true);
                }
            }
            }

        //		if(specialChance < 15){
        ////		if(specialChance < 50){
        //			//			Utilities.specialResourceCount += 1;
        //			if(this.elevationType == ELEVATION.MOUNTAIN){
        //				SpecialResourceChance specialResources = new SpecialResourceChance (
        //					new RESOURCE[] {
        //						RESOURCE.BEHEMOTH,
        //						RESOURCE.SLATE,
        //						RESOURCE.MARBLE,
        //						RESOURCE.MANA_STONE,
        //						RESOURCE.MITHRIL,
        //						RESOURCE.COBALT,
        //						RESOURCE.GOLD
        //					}, 
        ////					new int[] { 5, 60, 40, 15, 15, 15, 5 });
        //					new int[] { 0, 0, 0, 0, 0, 0, 0 });
        //				this.specialResource = ComputeSpecialResource (specialResources);
        //				if (this.specialResource != RESOURCE.NONE) {
        //					this.resourceVisualGO.GetComponent<SpriteRenderer> ().sprite = Resources.LoadAll<Sprite> ("Resources Icons")
        //					.Where (x => x.name == this.specialResource.ToString ()).ToList () [0];
        //					this.resourceVisualGO.SetActive (true);
        //				}
        //			}else{
        //				if (this.elevationType != ELEVATION.WATER) {
        //					this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
        //					if (this.specialResource != RESOURCE.NONE) {
        //						this.resourceVisualGO.GetComponent<SpriteRenderer> ().sprite = Resources.LoadAll<Sprite> ("Resources Icons")
        //						.Where (x => x.name == this.specialResource.ToString ()).ToList () [0];
        //						this.resourceVisualGO.SetActive (true);
        //					}
        //				}
        //			}
        //		}
    }
	public PandaBehaviour GetBehaviourTree(){
		return this.GetComponent<PandaBehaviour>();
	}
	private RESOURCE ComputeSpecialResource(SpecialResourceChance specialResources){
		int totalChance = 0;
		int lowerLimit = 0;
		int upperLimit = specialResources.chance [0];
		for(int i = 0; i < specialResources.resource.Length; i++){
			totalChance += specialResources.chance[i];
		}

		int chance = UnityEngine.Random.Range (0, totalChance);
		for(int i = 0; i < specialResources.resource.Length; i++){
			if(chance >= lowerLimit && chance < upperLimit){
				return specialResources.resource[i];
			}else{
				lowerLimit = upperLimit;
				if (i + 1 < specialResources.resource.Length) {
					upperLimit += specialResources.chance [i + 1];
				}
			}
		}
		return RESOURCE.NONE;
	}
	#endregion
		
	/*
	 * Returns all Hex tiles gameobjects within a radius
	 * 3 - 1 tile radius
	 * 6 - 2 tile radius
	 * 10 - 3 tile radius
	 * */
	public List<HexTile> GetTilesInRange(int range){
//		var layerMask = 1 << LayerMask.NameToLayer ("Hextiles");
//		Collider2D[] nearHexes = Physics2D.OverlapCircleAll (new Vector2(transform.position.x, transform.position.y), radius, layerMask);
//		List<HexTile> nearTiles = new List<HexTile>();
//		for (int i = 0; i < nearHexes.Length; i++) {
//			if (nearHexes[i].name != this.name) {
//				nearTiles.Add(nearHexes[i].gameObject.GetComponent<HexTile>());
////				nearHexes[i].gameObject.GetComponent<SpriteRenderer>().color = Color.black;
////				Debug.Log (nearHexes [i].name);
//			}
//		}
//		return nearTiles;
		List<HexTile> tilesInRange = new List<HexTile>();
		List<HexTile> checkedTiles = new List<HexTile> ();

		for (int i = 0; i < range; i++) {
			if (tilesInRange.Count <= 0) {
				tilesInRange.AddRange (this.AllNeighbours);
				checkedTiles.Add (this);
			}else{
				List<HexTile> tilesToAdd = new List<HexTile> ();
				for (int j = 0; j < tilesInRange.Count; j++) {
					if (!checkedTiles.Contains (tilesInRange [j])) {
						checkedTiles.Add (tilesInRange [j]);
						tilesToAdd.AddRange (tilesInRange[j].AllNeighbours);
					}
				}
				tilesInRange.AddRange (tilesToAdd);
			}
		}
		return tilesInRange.Distinct().ToList();
	}

//	public List<HexTile> GetTilesWithinRange(int range){
//		List<HexTile> tilesInRange = new List<HexTile>();
//		List<HexTile> checkedTiles = new List<HexTile> ();
//
//		for (int i = 0; i < range; i++) {
//			if (tilesInRange.Count <= 0) {
//				tilesInRange.AddRange (this.AllNeighbours);
//				checkedTiles.Add (this);
//			}else{
//				List<HexTile> tilesToAdd = new List<HexTile> ();
//				for (int j = 0; j < tilesInRange.Count; j++) {
//					if (!checkedTiles.Contains (tilesInRange [j])) {
//						checkedTiles.Add (tilesInRange [j]);
//						tilesToAdd.AddRange (tilesInRange[j].AllNeighbours);
//					}
//				}
//				tilesInRange.AddRange (tilesToAdd);
//			}
//		}
//		return tilesInRange.Distinct().ToList();
//	}

	#region Pathfinding
	public void FindNeighbours(HexTile[,] gameBoard) {
		var neighbours = new List<HexTile>();

		List<Point> possibleExits;

		if ((yCoordinate % 2) == 0) {
			possibleExits = Utilities.EvenNeighbours;
		} else {
			possibleExits = Utilities.OddNeighbours;
		}

		for (int i = 0; i < possibleExits.Count; i++) {
			int neighbourCoordinateX = xCoordinate + possibleExits [i].X;
			int neighbourCoordinateY = yCoordinate + possibleExits [i].Y;
			if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)){
				neighbours.Add (gameBoard [neighbourCoordinateX, neighbourCoordinateY]);
			}

		}
		this.AllNeighbours = neighbours;
	}
	#endregion
	
	#region Tile Visuals
//	public void GenerateTileDetails(){
//		List<HexTile> neighbours = this.AllNeighbours.ToList ();
//		for (int i = 0; i < neighbours.Count; i++) {
//			
//			int neighbourX = neighbours [i].xCoordinate;
//			int neighbourY = neighbours [i].yCoordinate;
//
//			Point difference = new Point((neighbourX - this.xCoordinate), (neighbourY - this.yCoordinate));
//			if (this.yCoordinate % 2 == 0) {
//				if (difference.X == -1 && difference.Y == 1) {
//					//top left
//					if (neighbours[i].elevationType == ELEVATION.WATER) {
//						this.topLeftGround.SetActive (true);
//					}else if (neighbours [i].biomeType != this.biomeType) {
////						this.topLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == 1) {
//					//top right
//					if (neighbours[i].elevationType == ELEVATION.WATER) {
//						this.topRightGround.SetActive (true);
//					}else if (neighbours [i].biomeType != this.biomeType) {
////						this.topRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 0) {
//					//right
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.rightGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.rightBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == -1){
//					//bottom right
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.bottomRightGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.bottomRightBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == -1){
//					//bottom left
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.bottomLeftGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.bottomLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == 0){
//					//left
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.leftGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.leftBorder.SetActive (true);
//					}
//				}
//			} else {
//				if (difference.X == 0 && difference.Y == 1) {
//					//top left
//					if (neighbours[i].elevationType == ELEVATION.WATER) {
//						this.topLeftGround.SetActive (true);
//					}else if (neighbours [i].biomeType != this.biomeType) {
////						this.topLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 1) {
//					//top right
//					if (neighbours[i].elevationType == ELEVATION.WATER) {
//						this.topRightGround.SetActive (true);
//					}else if (neighbours [i].biomeType != this.biomeType) {
////						this.topRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == 0) {
//					//right
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.rightGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.rightBorder.SetActive (true);
//					}
//				} else if (difference.X == 1 && difference.Y == -1){
//					//bottom right
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.bottomRightGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.bottomRightBorder.SetActive (true);
//					}
//				} else if (difference.X == 0 && difference.Y == -1){
//					//bottom left
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.bottomLeftGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.bottomLeftBorder.SetActive (true);
//					}
//				} else if (difference.X == -1 && difference.Y == 0){
//					//left
//					if (neighbours [i].elevationType == ELEVATION.WATER) {
//						this.leftGround.SetActive (true);
//					} else if (neighbours [i].biomeType != this.biomeType) {
////						this.leftBorder.SetActive (true);
//					}
//				}
//			}
//		}
//	}
	public void SetTileSprites(Sprite baseSprite, Sprite leftSprite, Sprite rightSprite, Sprite topLeftCornerSprite, Sprite topRightCornerSprite, Sprite leftCornerSprite, 
		Sprite rightCornerSprite, Sprite[] centerSprite){

		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
//		this.leftGround.GetComponent<SpriteRenderer>().sprite = leftSprite;
//		this.rightGround.GetComponent<SpriteRenderer>().sprite = rightSprite;
//		this.topLeftGround.GetComponent<SpriteRenderer> ().sprite = topLeftCornerSprite;
//		this.topRightGround.GetComponent<SpriteRenderer> ().sprite = topRightCornerSprite;
//		this.bottomLeftGround.GetComponent<SpriteRenderer>().sprite = leftCornerSprite;
//		this.bottomRightGround.GetComponent<SpriteRenderer>().sprite = rightCornerSprite;
		if (this.elevationType == ELEVATION.MOUNTAIN) {
			this.centerPiece.SetActive(true);
		} else {
			if (this.biomeType == BIOMES.GRASSLAND) {
				return;
			} else if (this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST || this.biomeType == BIOMES.TUNDRA) {
//				this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
//				this.centerPiece.SetActive(true);
//				if (this.biomeType != BIOMES.TUNDRA) {
//					this.centerPiece.transform.localPosition = new Vector3 (0f, 0.37f, 0f);
//				}
			} else {
//				int chanceForDetail = Random.Range (0, 100);
//				if (chanceForDetail < 25) {
//					this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
//					this.centerPiece.SetActive(true);
//					float randomXPosition = Random.Range(-1.30f, 1.30f);
//					float randomYPosition = Random.Range(-0.40f, 0.70f);
//					if (randomXPosition <= 0.45f && randomXPosition >= -0.45f) {
//						int chanceToModify = Random.Range(0, 100);
//						if (chanceToModify < 25) {
//							if (Mathf.Sign (randomYPosition) == 0) {
//								//negative
//								randomYPosition -= Random.Range(0.20f,0.40f);
//							} else {
//								//positive
//								randomYPosition += Random.Range(0.20f,0.40f);
//							}
//						}
//					}
//					this.centerPiece.transform.localPosition = new Vector3(randomXPosition, randomYPosition, 0f);
//				}
			}

		}
	}


	#endregion

	public void ShowCitySprite(){
		this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfCitySprite;
		this.structureGO.SetActive(true);
//		this.structureOnTile = STRUCTURE.CITY;
		Color color = this.city.kingdom.kingdomColor;
		color.a = 76.5f/255f;
		this.kingdomColorSprite.color = color;
		this.GetComponent<SpriteRenderer>().color = Color.white;
		this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.tundraTiles [Random.Range (0, Biomes.Instance.tundraTiles.Length)];
	}

	public void ShowNamePlate(){
		this.cityNameGO.SetActive(true);
		this.cityNameLbl.GetComponent<Renderer>().sortingLayerName = "CityNames";
		this.cityNameLbl.text = this.city.name + "\n" + this.city.kingdom.name;
	}

	public void ShowOccupiedSprite(){
		this.GetComponent<SpriteRenderer> ().sprite = Biomes.Instance.tundraTiles [Random.Range (0, Biomes.Instance.tundraTiles.Length)];
		this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfTraderSprite;
		this.structureGO.SetActive(true);
		this.centerPiece.SetActive(false);
//		this.kingdomColorSprite.gameObject.SetActive(true);
	}

//	public void OccupyTile(Citizen citizen){
//		this.isOccupied = true;
////		this.occupant = citizen;
//		this.GetComponent<SpriteRenderer> ().color = Color.white;
//		this.GetComponent<SpriteRenderer> ().sprite = Biomes.Instance.tundraTiles [Random.Range (0, Biomes.Instance.tundraTiles.Length)];
//		switch (citizen.role) {
//		case ROLE.FOODIE:
////			this.GetComponent<SpriteRenderer> ().color = Color.green;
//			if (this.specialResource == RESOURCE.NONE) {
//				if (this.defaultResource == RESOURCE.DEER || this.defaultResource == RESOURCE.PIG || this.defaultResource == RESOURCE.BEHEMOTH) {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfHuntingLodgeSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.HUNTING_LODGE;
//				} else {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfFarmSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.FARM;
//				}
//			} else {
//				if (this.specialResource == RESOURCE.DEER || this.specialResource == RESOURCE.PIG || this.specialResource == RESOURCE.BEHEMOTH) {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfHuntingLodgeSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.HUNTING_LODGE;
//				} else {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfFarmSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.FARM;
//				}
//			}
//			break;
//		case ROLE.GATHERER:
//			if (this.specialResource == RESOURCE.NONE) {
//				if (Utilities.GetBaseResourceType(this.defaultResource) == BASE_RESOURCE_TYPE.STONE) {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfQuarrySprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.QUARRY;
//				} else {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfLumberyardSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.LUMBERYARD;
//				}
//			} else {
//				if (Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.STONE) {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfQuarrySprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.QUARRY;
//				} else {
//					structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfLumberyardSprite;
//					structureGO.SetActive(true);
////					this.structureOnTile = STRUCTURE.LUMBERYARD;
//				}
//			}
//			break;
//		case ROLE.GENERAL:
//			structureGO.GetComponent<SpriteRenderer> ().sprite = CityGenerator.Instance.elfBarracks;
//			structureGO.SetActive (true);
////			this.structureOnTile = STRUCTURE.BARRACKS;
//			break;
//		case ROLE.MINER:
////			this.GetComponent<SpriteRenderer> ().color = Color.grey;
//			structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfMiningSprite;
//			structureGO.SetActive(true);
////			this.structureOnTile = STRUCTURE.MINES;
//			break;
//		case ROLE.TRADER:
//			structureGO.GetComponent<SpriteRenderer> ().sprite = CityGenerator.Instance.elfTraderSprite;
//			structureGO.SetActive (true);
////			this.structureOnTile = STRUCTURE.TRADING_POST;
//			break;
//		case ROLE.SPY:
//			structureGO.GetComponent<SpriteRenderer> ().sprite = CityGenerator.Instance.elfSpyGuild;
//			structureGO.SetActive (true);
////			this.structureOnTile = STRUCTURE.SPY_GUILD;
//			break;
//		case ROLE.GUARDIAN:
//			structureGO.GetComponent<SpriteRenderer> ().sprite = CityGenerator.Instance.elfKeep;
//			structureGO.SetActive (true);
////			this.structureOnTile = STRUCTURE.KEEP;
//			break;
//		case ROLE.ENVOY:
//			structureGO.GetComponent<SpriteRenderer> ().sprite = CityGenerator.Instance.elfMinistry;
//			structureGO.SetActive (true);
////			this.structureOnTile = STRUCTURE.MINISTRY;
//			break;
////		default:
////			this.GetComponent<SpriteRenderer> ().color = Color.blue;
////			break;
//		}
//	}

//	public void UnoccupyTile(){
////		if(this.occupant != null){
////			this.occupant.workLocation = null;
////			this.occupant.currentLocation = null;
////			this.occupant.isBusy = false;
////			this.occupant = null;
////		}
//
//		if (!this.isHabitable) {
//			this.isOccupied = false;
//			this.structureGO.SetActive (false);
////			this.structureOnTile = STRUCTURE.NONE;
//			this.GetComponent<SpriteRenderer> ().color = Color.clear;
//		}
//	}

	public void ResetTile(){
		this.isOwned = false;
		this.isOccupied = false;
//		this.occupant = null;
		this.structureGO.SetActive(false);
//		this.structureOnTile = STRUCTURE.NONE;
//		this.GetComponent<SpriteRenderer> ().color = Color.white;
		this.kingdomColorSprite.color = Color.white;
		this.kingdomColorSprite.gameObject.SetActive(false);
	}

	public void AddEventOnTile(GameEvent gameEvent){
		GameObject eventGO = GameObject.Instantiate (Resources.Load ("GameObjects/WorldEventItem") as GameObject, this.eventsParent) as GameObject;
		eventGO.transform.localPosition = Vector3.zero;
		eventGO.GetComponent<WorldEventItem> ().SetGameEvent(gameEvent);
//		eventGO.SetActive(false);
//		this.eventsOnTile.Add(eventGO.GetComponent<WorldEventItem>());
	}

//	public void ShowEventOnTile(EVENT_TYPES eventType){
//		if (this.eventsOnTile.Count <= 0) {
//			return;
//		}
//		if (eventType == EVENT_TYPES.ALL) {
//			for (int i = 0; i < this.eventsOnTile.Count; i++) {
//				this.eventsOnTile [i].gameObject.SetActive (true);
//			}
//		} else {
//			for (int i = 0; i < this.eventsOnTile.Count; i++) {
//				if (this.eventsOnTile [i].gameEvent.eventType == eventType) {
//					this.eventsOnTile [i].gameObject.SetActive (true);
//				}
//			}
//		}
//	}

//	public void HideEventsOnTile(){
//		if (this.eventsOnTile.Count <= 0) {
//			return;
//		}
//		for (int i = 0; i < this.eventsOnTile.Count; i++) {
//			this.eventsOnTile [i].gameObject.SetActive(false);
//		}
//	}

//	public void RemoveEvent(GameEvent gameEvent){
//		if (this.eventsOnTile.Count <= 0) {
//			return;
//		}
//		for (int i = 0; i < this.eventsOnTile.Count; i++) {
//			if (this.eventsOnTile[i].gameEvent.eventID == gameEvent.eventID) {
//				Destroy(this.eventsOnTile[i].gameObject);
//				this.eventsOnTile.RemoveAt(i);
//				break;
//			}
//		}
//	}

	void OnMouseDown(){
		if (UIManager.Instance.IsMouseOnUI ()) {
			return;
		}
		if (this.isHabitable && this.isOccupied && this.city != null) {
			CameraMove.Instance.CenterCameraOn(this.gameObject);
			UIManager.Instance.ShowCityInfo (this.city, true);
		}
	}

	void OnMouseOver(){
		if (UIManager.Instance.IsMouseOnUI ()) {
			return;
		}
		if (this.isHabitable && this.isOccupied) {
			this.city.HighlightAllOwnedTiles(204f/255f);
		}
//		if (!this.isHabitable && this.isOccupied && this.structureOnTile != STRUCTURE.NONE) {
//			UIManager.Instance.ShowSmallInfo("Occupant: [b]" + this.occupant.name + "[/b] \nStructure: [b]" + this.structureOnTile.ToString().Replace("_", " ") + "[/b]", this.transform);
//		}
		
	}

	void OnMouseExit(){
//		UIManager.Instance.HideSmallInfo();
		if (this.isHabitable && this.isOccupied) {
			if (!UIManager.Instance.kingdomInfoGO.activeSelf) {
				this.city.UnHighlightAllOwnedTiles ();
			} else {
				if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id != this.city.kingdom.id) {
					this.city.UnHighlightAllOwnedTiles ();
				} else {
					this.city.kingdom.HighlightAllOwnedTilesInKingdom();
				}
			}
		}
	}
}
