﻿using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;
    [Space(10)]
    [Header("Map Settings")]
    public float width;
	public float height;
    [SerializeField] private Transform _borderParent;
    [SerializeField] private int _borderThickness;

    public float xOffset;
    public float yOffset;

    public int tileSize;

    public float elevationFrequency;
    public float moistureFrequency;

    [Space(10)]
    [Header("Region Settings")]
    public int numOfRegions;
    public int refinementLevel;
    public List<InitialMapResource> resourceSetup;
	internal Dictionary<RESOURCE, int> resources = new Dictionary<RESOURCE, int>();

    [Space(10)]
	public List<GameObject> listHexes;
    public List<HexTile> hexTiles;
    public List<Region> allRegions;
	public HexTile[,] map;
    public HexTile[,] outerGrid;

	internal float mapWidth;
	internal float mapHeight;

	void Awake(){
		Instance = this;
		ConvertInitialResourceSetupToDictionary ();
	}

	internal void GenerateGrid () {
        float newX = xOffset * (width / 2);
        float newY = yOffset * (height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        //CameraMove.Instance.minimapCamera.transform.position
		map = new HexTile[(int)width, (int)height];
		listHexes = new List<GameObject>();
        hexTiles = new List<HexTile>();
        int id = 1;
		for (int x = 0;  x < width; x++){
			for(int y = 0; y < height; y++){
				float xPosition = x * xOffset;

				float yPosition = y * yOffset;
				if (y % 2 == 1) {
					xPosition += xOffset / 2;
				}

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.parent = this.transform;
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                hex.name = x + "," + y;
                HexTile currHex = hex.GetComponent<HexTile>();
                hexTiles.Add(currHex);
                currHex.id = id;
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
                currHex.SetPathfindingTag(0);
                listHexes.Add(hex);
                map[x, y] = hex.GetComponent<HexTile>();
                id++;
			}
		}
		listHexes.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(map));
		mapWidth = listHexes [listHexes.Count - 1].transform.position.x;
		mapHeight = listHexes [listHexes.Count - 1].transform.position.y;	
	}
    
    internal void GenerateOuterGrid() {
        int newWidth = (int)width + (_borderThickness * 2);
        int newHeight = (int)height + (_borderThickness * 2);

        float newX = xOffset * (newWidth / 2);
        float newY = yOffset * (newHeight / 2);

        List<HexTile> outerGridList = new List<HexTile>();
        outerGrid = new HexTile[newWidth, newHeight];

        _borderParent.transform.localPosition = new Vector2(-newX, -newY);
        for (int x = 0; x < newWidth; x++) {
            for (int y = 0; y < newHeight; y++) {
                if((x > _borderThickness && x < newWidth - _borderThickness) && (y > _borderThickness && y < newHeight - _borderThickness)) {
                    continue;
                }
                float xPosition = x * xOffset;

                float yPosition = y * yOffset;
                if (y % 2 == 1) {
                    xPosition += xOffset / 2;
                }

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.parent = _borderParent.transform;
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                HexTile currHex = hex.GetComponent<HexTile>();
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
                outerGrid[x, y] = currHex;
                outerGridList.Add(currHex);

                //int xToCopy = Mathf.Clamp(x, 0, (int)width - 1);
                //int yToCopy = Mathf.Clamp(y, 0, (int)height - 1);

                int xToCopy = Mathf.Max(x - (_borderThickness * 2), 0);
                int yToCopy = Mathf.Max(y - (_borderThickness * 2), 0);
                HexTile hexToCopy = map[xToCopy, yToCopy];

                hex.name = x + "," + y + "(Border) Copied from " + hexToCopy.name;

                currHex.SetElevation(hexToCopy.elevationType);
                Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
                Biomes.Instance.AddBiomeDetailToTile(currHex);
                //currHex.CopyEdgesFromOtherTile(hexToCopy);

                currHex.DisableColliders();
                currHex.HideFogOfWarObjects();
            }
        }

        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(outerGrid));


        //List<HexTile> outerTiles = listHexes.Select(x => x.GetComponent<HexTile>()).ToList();
        //outerTiles = outerTiles.Where(x => x.xCoordinate == 0 || x.xCoordinate == width - 1 || x.yCoordinate == 0 || x.yCoordinate == height - 1).ToList();
        //for (int i = 0; i < outerTiles.Count; i++) {
        //    HexTile currOuterTile = outerTiles[i];

        //    for (int k = 0; k < _borderThickness; k++) {
        //        int xCoordinateModifier = 1;
        //        int yCoordinateModifier = 1;
        //        float xPosition = currOuterTile.transform.localPosition.x;
        //        float yPosition = currOuterTile.transform.localPosition.y;

        //        if (currOuterTile.xCoordinate == 0) {
        //            xCoordinateModifier = -1;
        //            yCoordinateModifier = 0;
        //        } else if (currOuterTile.yCoordinate == 0) {
        //            xCoordinateModifier = 0;
        //            yCoordinateModifier = -1;
        //        } else if (currOuterTile.xCoordinate == width - 1) {
        //            xCoordinateModifier = 1;
        //            yCoordinateModifier = 0;
        //        } else if (currOuterTile.yCoordinate == height - 1) {
        //            xCoordinateModifier = 0;
        //            yCoordinateModifier = 1;
        //        }

        //        if (k % 2 == 0 && yCoordinateModifier != 0) {
        //            xPosition += xOffset / 2;
        //        }

        //        xPosition = xPosition + (((k + 1) * xOffset) * xCoordinateModifier);
        //        yPosition = yPosition + (((k + 1) * yOffset) * yCoordinateModifier);

        //        GameObject hex = GameObject.Instantiate(goHex) as GameObject;
        //        HexTile currHex = hex.GetComponent<HexTile>();
        //        hex.transform.parent = this.transform;
        //        hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
        //        hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
        //        hex.name = currOuterTile.name + " (Border)";
        //        currHex.SetElevation(currOuterTile.elevationType);
        //        Biomes.Instance.SetBiomeForTile(currOuterTile.biomeType, currHex);
        //        Biomes.Instance.AddBiomeDetailToTile(currHex);
        //        currHex.DisableColliders();
        //        currHex.HideFogOfWarObjects();
        //    }
        //}
    }

	internal GameObject GetHex(string hexName){
		for(int i = 0; i < listHexes.Count; i++){
			if(hexName == listHexes[i].name){
				return listHexes[i];
			}
		}
		return null;
	}

    public void GenerateNeighboursWithSameTag() {
        for (int i = 0; i < listHexes.Count; i++) {
            HexTile currHex = listHexes[i].GetComponent<HexTile>();
            currHex.sameTagNeighbours = currHex.AllNeighbours.Where(x => x.tileTag == currHex.tileTag).ToList();
        }
    }

    public List<HexTile> GetTilesInRange(HexTile center, int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        CubeCoordinate cube = OddRToCube(new HexCoordinate(center.xCoordinate, center.yCoordinate));
        Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx-range); dy <= Mathf.Min(range, -dx+range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if(hex.col >= 0 && hex.row >= 0 && !(hex.col == center.xCoordinate && hex.row == center.yCoordinate)) {
                    tilesInRange.Add(map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }

    public HexCoordinate CubeToOddR(CubeCoordinate cube) {
        int modifier = 0;
        if(cube.z % 2 == 1) {
            modifier = 1;
        }
        int col = cube.x + (cube.z - (modifier)) / 2;
        int row = cube.z;
        return new HexCoordinate(col, row);
    }

    public CubeCoordinate OddRToCube(HexCoordinate hex) {
        int modifier = 0;
        if (hex.row % 2 == 1) {
            modifier = 1;
        }

        int x = hex.col - (hex.row - (modifier)) / 2;
        int z = hex.row;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }

    public bool GenerateRegions(int numOfRegions, int refinementLevel) {
        List<HexTile> allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
        List<HexTile> possibleCenterTiles = new List<HexTile>(allHexTiles.Where(x => (x.xCoordinate > 1 && x.xCoordinate < width - 1) && (x.yCoordinate < height - 2 && x.yCoordinate > 2)));
        HexTile[] initialCenters = new HexTile[numOfRegions];
        allRegions = new List<Region>();
        for (int i = 0; i < numOfRegions; i++) {
            if(possibleCenterTiles.Count <= 0) {
                //throw new System.Exception("All tiles have been used up!");
                return false;
            }
            HexTile chosenHexTile = possibleCenterTiles[Random.Range(0, possibleCenterTiles.Count)];
            possibleCenterTiles.Remove(chosenHexTile);
            allHexTiles.Remove(chosenHexTile);
            initialCenters[i] = chosenHexTile;
            Region newRegion = new Region(chosenHexTile);
            allRegions.Add(newRegion);
            //Color centerOfMassColor = newRegion.regionColor;
            //centerOfMassColor.a = 75.6f / 255f;
            //chosenHexTile.SetTileHighlightColor(centerOfMassColor);
            //chosenHexTile.ShowTileHighlight();
            foreach (HexTile hex in chosenHexTile.GetTilesInRange(5)) {
                possibleCenterTiles.Remove(hex);
            }
        }
        Debug.Log("Successfully got " + initialCenters.Length.ToString() + " center of masses!");

        for (int i = 0; i < refinementLevel; i++) {
            if(i != 0) {
                allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
                for (int j = 0; j < allRegions.Count; j++) {
                    allRegions[j].ReComputeCenterOfMass();
                    allRegions[j].ResetTilesInRegion();
                    allHexTiles.Remove(allRegions[j].centerOfMass);
                }
            }
            for (int j = 0; j < allHexTiles.Count; j++) {
                HexTile currHexTile = allHexTiles[j];
                Region regionClosestTo = null;
                float closestDistance = 999999f;
                for (int k = 0; k < allRegions.Count; k++) {
                    Region currRegion = allRegions[k];
                    HexTile currCenter = currRegion.centerOfMass;
                    float distance = Vector2.Distance(currHexTile.transform.position, currCenter.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        regionClosestTo = currRegion;
                    }
                }
                if (regionClosestTo != null) {
                    regionClosestTo.AddTile(currHexTile);
                    //currHexTile.SetTileHighlightColor(regionClosestTo.regionColor);
                    //currHexTile.ShowTileHighlight();
                } else {
                    throw new System.Exception("Could not find closest distance for tile " + currHexTile.name);
                }

            }
        }

        for (int i = 0; i < allRegions.Count; i++) {
            allRegions[i].RevalidateCenterOfMass();
            allRegions[i].CheckForAdjacency();
        }
        return true;
    }

    public void GenerateResourcesPerRegion() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
			if(currRegion.tileWithSpecialResource != null && currRegion.tileWithSpecialResource.specialResource != RESOURCE.NONE){
				continue;
			}
			RESOURCE resourceForRegion = resources.Keys.ElementAt(Random.Range(0, resources.Count));
			ReduceResourceCount (resourceForRegion);
            currRegion.SetSpecialResource(resourceForRegion);
            currRegion.ComputeNaturalResourceLevel(); //Compute For Natural Resource Level of current region
        }
    }
	public void GenerateResourceTiles(){
		for (int i = 0; i < allRegions.Count; i++) {
			Region currRegion = allRegions[i];
			currRegion.AssignATileAsResourceTile();
		}
	}
    private void ConvertInitialResourceSetupToDictionary() {
        for (int i = 0; i < resourceSetup.Count; i++) {
            InitialMapResource r = resourceSetup[i];
            resources.Add(r.resourceType, r.resourceAmount);
        }
    }
	internal bool ReduceResourceCount(RESOURCE resourceForRegion){
		this.resources[resourceForRegion] -= 1;
		if(this.resources[resourceForRegion] <= 0) {
			this.resources.Remove(resourceForRegion);
			return true;
		}else{
			return false;
		}
	}

    /*
     * <summary>
     * Generate landmarks for all regions
     * other landmarks include: shrine, habitat
     * </summary>
     * */
    public void GenerateOtherLandmarksPerRegion() {
        List<RESOURCE> allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
        allSpecialResources.Remove(RESOURCE.NONE);
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if(currRegion.landmarkCount < 2) {
                if(Random.Range(0, 2) == 0) {
                    currRegion.SetSummoningShrine();
                } else {
                    currRegion.SetHabitat();
                }
            }
        }
    }

    public void GenerateLandmarkExternalConnections() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            List<Landmark> landmarksInRegion = new List<Landmark>(currRegion.landmarks);
            for (int j = 0; j < landmarksInRegion.Count; j++) {
                Landmark currLandmark = landmarksInRegion[j];
                if(currLandmark.connections.Count >= RoadManager.Instance.maxLandmarkConnections) {
                    continue;
                }

                List<Region> adjacentRegions = new List<Region>(currRegion.adjacentRegions);
                List<HexTile> tilesToChooseFrom = new List<HexTile>(adjacentRegions.Where(x => x.connections.Count < RoadManager.Instance.maxConnections 
                    && !x.connections.Contains(currLandmark.location)).Select(x => x.centerOfMass));

                for (int k = 0; k < adjacentRegions.Count; k++) {
                    tilesToChooseFrom.AddRange(adjacentRegions[k].landmarks.Where(x => x.connections.Count < RoadManager.Instance.maxLandmarkConnections &&
                        !x.connections.Contains(currLandmark.location)).Select(x => x.location));
                }

                //When connecting landmarks to nearby landmarks, exclude the other landmark on the same region if they are already connected
                for (int k = 0; k < landmarksInRegion.Count; k++) {
                    Landmark otherLandmark = landmarksInRegion[k];
                    if(otherLandmark != currLandmark) {
                        if(PathGenerator.Instance.GetPath(currLandmark.location, otherLandmark.location, PATHFINDING_MODE.USE_ROADS) != null) {
                            RoadManager.Instance.ConnectLandmarkToLandmark(currLandmark.location, otherLandmark.location);
                        } else {
                            if(otherLandmark.connections.Count < RoadManager.Instance.maxLandmarkConnections) {
                                tilesToChooseFrom.Add(otherLandmark.location);
                            }
                        }
                    }
                }

                tilesToChooseFrom = tilesToChooseFrom.OrderBy(x => Vector2.Distance(currLandmark.location.transform.position, x.transform.position)).ToList();
                //if it can, it should connect to the nearest landmark or city within its region or adjacent region without intersecting a 
                //major road and without creating a path in a third different region
                for (int k = 0; k < tilesToChooseFrom.Count; k++) {
                    HexTile currTile = tilesToChooseFrom[k];
                    List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.location, currTile, PATHFINDING_MODE.LANDMARK_EXTERNAL_CONNECTION);
                    if(path != null) {
                        if (currTile.isHabitable) {
                            RoadManager.Instance.ConnectLandmarkToRegion(currLandmark.location, currTile.region);
                        } else if (currTile.hasLandmark) {
                            RoadManager.Instance.ConnectLandmarkToLandmark(currLandmark.location, currTile);
                        }
                        RoadManager.Instance.CreateRoad(path, ROAD_TYPE.MINOR);
                        break;
                    }
                }

            }
        }
    }

   // public void GenerateLandmarksPerRegion() {
   //     List<RESOURCE> allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
   //     allSpecialResources.Remove(RESOURCE.NONE);
   //     for (int i = 0; i < allRegions.Count; i++) {
   //         Region currRegion = allRegions[i];
			//int chanceResource = UnityEngine.Random.Range (0, 2);
			//int chanceShrine = UnityEngine.Random.Range (0, 2);
			//int chanceHabitat = UnityEngine.Random.Range (0, 2);
			//if(chanceResource == 0 && currRegion.landmarkCount < 2) {
   //             //Region has a special resource
   //             if(allSpecialResources.Count <= 0) {
   //                 allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
   //                 allSpecialResources.Remove(RESOURCE.MANA_STONE);
   //                 allSpecialResources.Remove(RESOURCE.COBALT);
   //                 allSpecialResources.Remove(RESOURCE.MITHRIL);
   //                 allSpecialResources.Remove(RESOURCE.NONE);
   //             }
   //             RESOURCE specialResource = allSpecialResources[Random.Range(0, allSpecialResources.Count)];
   //             allSpecialResources.Remove(specialResource);
   //             currRegion.SetSpecialResource(specialResource);
   //         }
   //         currRegion.ComputeNaturalResourceLevel(); //Compute For Natural Resource Level of current region

			//if(chanceShrine == 0 && currRegion.landmarkCount < 2){
			//	currRegion.SetSummoningShrine();
			//}

			//if(chanceHabitat == 0 && currRegion.landmarkCount < 2){
			//	currRegion.SetHabitat ();
			//}
   //     }
   //     //Debug.Log("All Special Resources Per Region:");
   //     //for (int i = 0; i < allRegions.Count; i++) {
   //     //    Debug.Log("Region " + i.ToString() + ": " + allRegions[i].specialResource.ToString());
   //     //}
   // }
//	public void GenerateRoadConnectionLandmarkToCity(){
//		for (int i = 0; i < allRegions.Count; i++) {
//			Region currRegion = allRegions [i];
//			if(currRegion.tileWithSpecialResource != null){
//				RoadManager.Instance.DrawConnection (currRegion.tileWithSpecialResource, currRegion.centerOfMass, ROAD_TYPE.MINOR);
//			}
//			if(currRegion.tileWithSummoningShrine != null){
//				RoadManager.Instance.DrawConnection (currRegion.tileWithSummoningShrine, currRegion.centerOfMass, ROAD_TYPE.MINOR);
//			}
//			if(currRegion.tileWithHabitat != null){
//				RoadManager.Instance.DrawConnection (currRegion.tileWithHabitat, currRegion.centerOfMass, ROAD_TYPE.MINOR);
//			}
//		}
//	}
//	public void GenerateCityConnections(){
//		for (int i = 0; i < allRegions.Count; i++) {
//			Region currRegion = allRegions [i];
//			if(currRegion.centerOfMass.GetNumOfConnectedCenterOfMass() >= RoadManager.Instance.maxCityConnections){
//				continue;
//			}
//			int maxConnection = RoadManager.Instance.maxConnections - currRegion.centerOfMass.connectedTiles.Count;
//			if(maxConnection > 4){
//				maxConnection = 4;
//			}
//			int numOfConnections = UnityEngine.Random.Range (1, maxConnection);
////			List<Region> adjacentRegions = currRegion.adjacentRegions.Where(x => !currRegion.centerOfMass.connectedTiles.ContainsKey(x.centerOfMass) && x.centerOfMass.GetNumOfConnectedCenterOfMass() < RoadManager.Instance.maxCityConnections
////				&& !RoadManager.Instance.IsIntersectingWith(currRegion.centerOfMass, x.centerOfMass, ROAD_TYPE.MINOR)).ToList();
//			List<Region> adjacentRegions = currRegion.adjacentRegions.Where(x => !currRegion.centerOfMass.connectedTiles.ContainsKey(x.centerOfMass) && x.centerOfMass.GetNumOfConnectedCenterOfMass() < RoadManager.Instance.maxCityConnections).ToList();
//			if(adjacentRegions.Count > 0){
//				if(numOfConnections > adjacentRegions.Count){
//					numOfConnections = adjacentRegions.Count;
//				}
//				Region chosenRegion = null;
//				for (int j = 0; j < numOfConnections; j++) {
//					List<Region> priorityAdjacentRegions = adjacentRegions.Where (x => x.centerOfMass.GetNumOfConnectedCenterOfMass () > 0).ToList ();
//					if(priorityAdjacentRegions.Count > 0){
//						chosenRegion = priorityAdjacentRegions [UnityEngine.Random.Range (0, priorityAdjacentRegions.Count)];
//					}else{
//						chosenRegion = adjacentRegions [UnityEngine.Random.Range (0, adjacentRegions.Count)];
//					}
//					RoadManager.Instance.DrawConnection (currRegion.centerOfMass, chosenRegion.centerOfMass, ROAD_TYPE.MAJOR);
//					adjacentRegions.Remove (chosenRegion);
//					if(currRegion.centerOfMass.GetNumOfConnectedCenterOfMass() >= RoadManager.Instance.maxCityConnections){
//						break;
//					}
//				}
//			}
//		}
//	}
//	public void GenerateExtraLandmarkConnections(){
//		for (int i = 0; i < allRegions.Count; i++) {
//			Region currRegion = allRegions [i];
//			if(currRegion.tileWithSpecialResource != null){
//				CreateExtraLandmarkConnections (currRegion.tileWithSpecialResource);
//			}
//			if(currRegion.tileWithSummoningShrine != null){
//				CreateExtraLandmarkConnections (currRegion.tileWithSummoningShrine);
//			}
//			if(currRegion.tileWithHabitat != null){
//				CreateExtraLandmarkConnections (currRegion.tileWithHabitat);
//			}
//		}
//	}
//	private void CreateExtraLandmarkConnections(HexTile landmark){
//		if(landmark.connectedTiles.Count < RoadManager.Instance.maxLandmarkConnections){
//			int chanceAdjCity = UnityEngine.Random.Range (0, 2);
//			if(chanceAdjCity == 0){
//				for (int i = 0; i < landmark.region.adjacentRegions.Count; i++) {
//					if(!landmark.connectedTiles.ContainsKey(landmark.region.adjacentRegions[i].centerOfMass) && landmark.region.adjacentRegions[i].centerOfMass.connectedTiles.Count < RoadManager.Instance.maxConnections){
//						RoadManager.Instance.DrawConnection (landmark, landmark.region.adjacentRegions[i].centerOfMass, ROAD_TYPE.MINOR);
//						break;	
//					}
//				}
//			}
//		}
//		if(landmark.connectedTiles.Count < RoadManager.Instance.maxLandmarkConnections){
//			int chanceAdjLandmark = UnityEngine.Random.Range (0, 2);
//			if(chanceAdjLandmark == 0){
//				int insideChance = UnityEngine.Random.Range (0, 2);
//				if(insideChance == 0){
//					List<HexTile> adjLandmarks = landmark.region.tilesInRegion.Where(x => x.hasLandmark && x.id != landmark.id && !landmark.connectedTiles.ContainsKey(x)
//						&& x.connectedTiles.Count < RoadManager.Instance.maxLandmarkConnections).ToList();
//					if(adjLandmarks.Count > 0){
//						for (int i = 0; i < adjLandmarks.Count; i++) {
//							RoadManager.Instance.DrawConnection (landmark, adjLandmarks[i], ROAD_TYPE.MINOR);
//							return;
//						}
//					}
//				}

//				List<Region> adjRegions = Utilities.Shuffle (landmark.region.adjacentRegions);
//				for (int i = 0; i < adjRegions.Count; i++) {
//					List<HexTile> adjLandmarks = adjRegions[i].tilesInRegion.Where(x => x.hasLandmark && !landmark.connectedTiles.ContainsKey(x)
//						&& x.connectedTiles.Count < RoadManager.Instance.maxLandmarkConnections).ToList();
//					if(adjLandmarks.Count > 0){
//						for (int j = 0; j < adjLandmarks.Count; j++) {
//							RoadManager.Instance.DrawConnection (landmark, adjLandmarks[j], ROAD_TYPE.MINOR);
//							return;
//						}
//					}
//				}
//			}
//		}
//		if (landmark.connectedTiles.Count < RoadManager.Instance.maxLandmarkConnections) {
//			//connect to major road
//		}
//	}

    public void UpdateAllRegionsDiscoveredKingdoms() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if(currRegion.occupant != null) {
                currRegion.CheckForDiscoveredKingdoms();
            }
        }
    }
}
