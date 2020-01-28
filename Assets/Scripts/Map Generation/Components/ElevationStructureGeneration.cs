﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class ElevationStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			List<ElevationIsland> islandsInRegion = GetElevationIslandsInRegion(region);
			for (int j = 0; j < islandsInRegion.Count; j++) {
				ElevationIsland currIsland = islandsInRegion[j];
				STRUCTURE_TYPE structureType = GetStructureTypeFor(currIsland.elevation);
				LocationStructure elevationStructure = LandmarkManager.Instance.CreateNewStructureAt(region, structureType);
				
				yield return MapGenerator.Instance.StartCoroutine(
					GenerateElevationMap(currIsland, elevationStructure));
				yield return MapGenerator.Instance.StartCoroutine(
					RefreshTilemapCollider(region.innerMap.structureTilemapCollider));

			}
		}
		yield return null;
	}
	private IEnumerator RefreshTilemapCollider(TilemapCollider2D tilemapCollider2D) {
		tilemapCollider2D.enabled = false;
		yield return new WaitForSeconds(0.5f);
		// ReSharper disable once Unity.InefficientPropertyAccess
		tilemapCollider2D.enabled = true;
	}
	private STRUCTURE_TYPE GetStructureTypeFor(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.MOUNTAIN:
				return STRUCTURE_TYPE.CAVE;
			case ELEVATION.WATER:
				return STRUCTURE_TYPE.OCEAN;
		}
		throw new Exception($"There is no corresponding structure type for {elevation.ToString()}");
	}
	private List<ElevationIsland> GetElevationIslandsInRegion(Region region) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		ELEVATION[] elevationsToCheck = new[] {ELEVATION.WATER, ELEVATION.MOUNTAIN};
		for (int i = 0; i < elevationsToCheck.Length; i++) {
			ELEVATION elevation = elevationsToCheck[i];
			List<HexTile> tilesOfThatElevation = GetTilesWithElevationInRegion(region, elevation);
			List<ElevationIsland> initialIslands = CreateInitialIslands(tilesOfThatElevation, elevation);
			List<ElevationIsland> mergedIslands = MergeIslands(initialIslands);
			islands.AddRange(mergedIslands);
		}
		return islands;
	}
	private List<HexTile> GetTilesWithElevationInRegion(Region region, ELEVATION elevation) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < region.tiles.Count; i++) {
			HexTile tile = region.tiles[i];
			if (tile.elevationType == elevation) {
				tiles.Add(tile);
			}
		}
		return tiles;
	}
	private List<ElevationIsland> CreateInitialIslands(List<HexTile> tiles, ELEVATION elevation) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		for (int i = 0; i < tiles.Count; i++) {
			HexTile tile = tiles[i];
			ElevationIsland island = new ElevationIsland(elevation);
			island.AddTile(tile);
			islands.Add(island);
		}
		return islands;
	}
	private List<ElevationIsland> MergeIslands(List<ElevationIsland> islands) {
		for (int i = 0; i < islands.Count; i++) {
			ElevationIsland currIsland = islands[i];
			for (int j = 0; j < islands.Count; j++) {
				ElevationIsland otherIsland = islands[j];
				if (currIsland != otherIsland) {
					if (currIsland.IsAdjacentToIsland(otherIsland)) {
						currIsland.MergeWithIsland(otherIsland);
					}
				}
			}
		}
		List<ElevationIsland> mergedIslands = new List<ElevationIsland>();
		for (int i = 0; i < islands.Count; i++) {
			ElevationIsland island = islands[i];
			if (island.tilesInIsland.Count > 0) {
				mergedIslands.Add(island);
			}
		}
		return mergedIslands;
	}
	private TileBase GetWallTileAssetForElevation(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.WATER:
				return InnerMapManager.Instance.assetManager.shoreTle;
			case ELEVATION.MOUNTAIN:
				return InnerMapManager.Instance.assetManager.caveWallTile;
		}
		return null;
	}

	#region Cellular Automata
	private IEnumerator GenerateElevationMap(ElevationIsland island, LocationStructure elevationStructure) {
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>();
		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile tileInIsland = island.tilesInIsland[i];
			locationGridTiles.AddRange(tileInIsland.locationGridTiles);
		}
		
		// int minX = locationGridTiles.Min(t => t.localPlace.x);
		// int maxX = locationGridTiles.Max(t => t.localPlace.x);
		// int minY = locationGridTiles.Min(t => t.localPlace.y);
		// int maxY = locationGridTiles.Max(t => t.localPlace.y);
		//
		// int xSize = (maxX - minX) + 1;
		// int ySize = (maxY - minY) + 1;

		
		// LocationGridTile[,] arrangedMap = new LocationGridTile[xSize, ySize];
		// for (int i = 0; i < locationGridTiles.Count; i++) {
		// 	LocationGridTile tile = locationGridTiles[i];
		// 	int localX = tile.localPlace.x - minX;
		// 	int localY = tile.localPlace.y - minY;
		// 	arrangedMap[localX, localY] = tile;
		// }

		int[,] cellMap = null;
		if (island.elevation == ELEVATION.WATER) {
			WaterCellAutomata(locationGridTiles, elevationStructure);
		} else if (island.elevation == ELEVATION.MOUNTAIN) {
			MountainCellAutomata(locationGridTiles, elevationStructure);
		}

		
		
		// for (int x = 0; x < xSize; x++) {
		// 	for (int y = 0; y < ySize; y++) {
		// 		int cellMapValue = cellMap[x, y];
		// 		LocationGridTile tile = arrangedMap[x, y];
		// 		if (tile != null) {
		// 			if (island.elevation == ELEVATION.MOUNTAIN) {
		// 				//set as ground of mountiain (hollowed inside of cave)
		// 				tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
		// 				if (cellMapValue == 0) {
		// 					tile.SetStructureTilemapVisual(null);
		// 				} else {
		// 					//set as mountain
		// 					tile.SetStructureTilemapVisual(GetWallTileAssetForElevation(island.elevation));
		// 					tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		// 				}
		// 				tile.SetStructure(elevationStructure); //set whole cave area as cave structure
		// 			} else if (island.elevation == ELEVATION.WATER) {
		// 				if (cellMapValue == 1) {
		// 					//not part of water
		// 					tile.SetStructureTilemapVisual(null);
		// 				} else {
		// 					//set as water
		// 					tile.SetStructureTilemapVisual(GetWallTileAssetForElevation(island.elevation));
		// 					tile.SetStructure(elevationStructure); //set as part of ocean structure
		// 				}
		// 			}
		// 		}
		// 	}
		// }
		
		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile hexTile = island.tilesInIsland[i];
			for (int j = 0; j < hexTile.ownedBuildSpots.Length; j++) {
				BuildingSpot spot = hexTile.ownedBuildSpots[j];
				if (spot.isOccupied == false) {
					spot.SetIsOccupied(true);
					spot.UpdateAdjacentSpotsOccupancy(hexTile.region.innerMap);	
				}
			}
		}
		yield return null;
	}
	private void WaterCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
		int[,] cellMap = Cellular_Automata.CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, 2, 20);
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		
		CellularAutomataGenerator.DrawMap(tileMap, cellMap, null, 
			InnerMapManager.Instance.assetManager.shoreTle, 
			null, (locationGridTile) => SetAsWater(locationGridTile, elevationStructure));
	}
	private void SetAsWater(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
	}
	private void MountainCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
		List<LocationGridTile> refinedTiles =
			locationGridTiles.Where(t => t.HasNeighbourNotInList(locationGridTiles) == false && t.IsAtEdgeOfMap() == false).ToList();
		
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(refinedTiles);
		int[,] cellMap = Cellular_Automata.CellularAutomataGenerator.GenerateMap(tileMap, refinedTiles, 1, 35);
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		
		CellularAutomataGenerator.DrawMap(tileMap, cellMap, InnerMapManager.Instance.assetManager.caveWallTile, 
			null, 
			(locationGridTile) => SetAsMountainWall(locationGridTile, elevationStructure),
			(locationGridTile) => SetAsMountainGround(locationGridTile, elevationStructure));
	}
	private void SetAsMountainWall(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
	}
	private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
	}
	#endregion
}

public class ElevationIsland {
	public readonly ELEVATION elevation;
	public readonly List<HexTile> tilesInIsland;

	public ElevationIsland(ELEVATION elevation) {
		this.elevation = elevation;
		tilesInIsland = new List<HexTile>();
	}

	public void AddTile(HexTile tile) {
		if (tilesInIsland.Contains(tile) == false) {
			tilesInIsland.Add(tile);	
		}
	}
	private void RemoveTile(HexTile tile) {
		tilesInIsland.Remove(tile);
	}
	private void RemoveAllTiles() {
		tilesInIsland.Clear();
	}
	
	public void MergeWithIsland(ElevationIsland otherIsland) {
		for (int i = 0; i < otherIsland.tilesInIsland.Count; i++) {
			HexTile tileInOtherIsland = otherIsland.tilesInIsland[i];
			AddTile(tileInOtherIsland);
		}
		otherIsland.RemoveAllTiles();
	}

	public bool IsAdjacentToIsland(ElevationIsland otherIsland) {
		for (int i = 0; i < tilesInIsland.Count; i++) {
			HexTile tile = tilesInIsland[i];
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (otherIsland.tilesInIsland.Contains(neighbour)) {
					//this island has a tile that has a neighbour that is part of the given island.
					return true;
				}
			}
		}
		return false;
	}
}
