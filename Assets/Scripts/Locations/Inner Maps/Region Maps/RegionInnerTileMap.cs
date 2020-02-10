﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {
        public virtual bool isSettlementMap => false;
        private Region region { get; set; }
        
        public override void Initialize(ILocation location) {
            base.Initialize(location);
            this.region = location as Region;
        }
        public IEnumerator GenerateMap(MapGenerationComponent mapGenerationComponent) {
            this.name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTilemaps();
            Vector2Int buildSpotGridSize = CreateBuildSpotGrid(mapGenerationComponent);
            int tileMapWidth = buildSpotGridSize.x * InnerMapManager.BuildingSpotSize.x;
            int tileMapHeight = buildSpotGridSize.y * InnerMapManager.BuildingSpotSize.y;
            yield return StartCoroutine(GenerateGrid(tileMapWidth, tileMapHeight, mapGenerationComponent));
            InitializeBuildingSpots(mapGenerationComponent);
            ConnectHexTilesToBuildSpots(mapGenerationComponent);
            AssignWilderness(mapGenerationComponent);
            yield return StartCoroutine(GenerateDetails(mapGenerationComponent));
        }
        private void AssignWilderness(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                bool isAtEdges = UtilityScripts.Utilities.IsInRange(tile.localPlace.x, 0, WestEdge) ||
                                 UtilityScripts.Utilities.IsInRange(tile.localPlace.x, width - EastEdge, width) ||
                                 UtilityScripts.Utilities.IsInRange(tile.localPlace.y, 0, SouthEdge) ||
                                 UtilityScripts.Utilities.IsInRange(tile.localPlace.y, height - NorthEdge, width);
                if (isAtEdges == false) { //&& tile.buildSpotOwner.isPartOfParentRegionMap
                    tile.CreateGenericTileObject();
                    tile.SetStructure(structure);
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} AssignWilderness took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        #region Build Spots
        private Vector2Int CreateBuildSpotGrid(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int buildSpotGridWidth;
            
            int maxX = region.tiles.Max(t => t.data.xCoordinate);
            int minX = region.tiles.Min(t => t.data.xCoordinate);

            int difference = ((maxX - minX) + 1) * 2;

            if (region.AreLeftAndRightMostTilesInSameRowType()) {
                buildSpotGridWidth = difference;
            } else {
                buildSpotGridWidth = difference + 1;
            }
            
            int maxY = region.tiles.Max(t => t.data.yCoordinate);
            int minY = region.tiles.Min(t => t.data.yCoordinate);
            int buildSpotGridHeight = ((maxY - minY) + 1) * 2;
            
            buildingSpots = new BuildingSpot[buildSpotGridWidth, buildSpotGridHeight];
            for (int x = 0; x < buildSpotGridWidth; x++) {
                for (int y = 0; y < buildSpotGridHeight; y++) {
                    GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
                    float xPos = (x + 1) * (InnerMapManager.BuildingSpotSize.x) - (InnerMapManager.BuildingSpotSize.x / 2f);
                    float yPos = (y + 1) * (InnerMapManager.BuildingSpotSize.y) - (InnerMapManager.BuildingSpotSize.y / 2f);
                    buildSpotGo.transform.localPosition = new Vector2(xPos, yPos);
                    BuildingSpot newSpot = new BuildingSpot(
                        new Vector3Int((int)xPos, (int)yPos, 0), new Vector2Int(x, y));
                    buildingSpots[x, y] = newSpot;
                    
                    BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
                    spotItem.SetBuildingSpot(newSpot);
                    newSpot.SetBuildSpotItem(spotItem);
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} CreateBuildSpotGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            return new Vector2Int(buildSpotGridWidth, buildSpotGridHeight);
        }
        private void InitializeBuildingSpots(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int upperBoundX = buildingSpots.GetUpperBound(0);
            int upperBoundY = buildingSpots.GetUpperBound(1);
            for (int x = 0; x <= upperBoundX; x++) {
                for (int y = 0; y <= upperBoundY; y++) {
                    BuildingSpot spot = buildingSpots[x, y];
                    spot.Initialize(this);
                    spot.FindNeighbours(this);
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} initialize building spots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private void ConnectHexTilesToBuildSpots(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            HexTile leftMostTile = region.GetLeftMostTile();
            List<int> leftMostRows = region.GetLeftMostRows();
            for (int localX = 0; localX <= region.hexTileMap.GetUpperBound(0); localX++) {
                for (int localY = 0; localY <= region.hexTileMap.GetUpperBound(1); localY++) {
                    HexTile firstTileInRow = region.hexTileMap[0, localY];
                    HexTile tile = region.hexTileMap[localX, localY];
                    if (tile.region == this.region) {
                        //the row will be indented if its row type (odd/even) is not the same as the row type of the left most tile.
                        //and the first tile in it's row is not null.
                        bool isIndented = UtilityScripts.Utilities.IsEven(tile.yCoordinate) !=
                                          UtilityScripts.Utilities.IsEven(leftMostTile.yCoordinate);

                        int buildSpotColumn1 = localX * 2;
                        int buildSpotColumn2 = buildSpotColumn1 + 1;
                        
                        if (isIndented) {
                            buildSpotColumn1 += 1;
                            buildSpotColumn2 += 1;
                            if (firstTileInRow.region != this.region) {
                                buildSpotColumn1 -= 2;
                                buildSpotColumn2 -= 2;
                            }
                        }


                        int buildSpotRow1 = localY * 2;
                        int buildSpotRow2 = buildSpotRow1 + 1;
                        AssignBuildSpotsToHexTile(tile, buildSpotColumn1, buildSpotColumn2,
                            buildSpotRow1, buildSpotRow2);    
                    }
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} ConnectHexTilesToBuildSpots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private void AssignBuildSpotsToHexTile(HexTile tile, int column1, int column2, int row1, int row2) {
            int width = (column2 - column1) + 1;
            int height = (row2 - row1) + 1;
            BuildingSpot[] spots = new BuildingSpot[width * height];
            int index = 0;
            for (int column = column1; column <= column2; column++) {
                for (int row = row1; row <= row2; row++) {
                    BuildingSpot spot = buildingSpots[column, row];
                    spot.SetHexTileOwner(tile);
                    spots[index] = spot;
                    index++;
                }
            }
            tile.SetOwnedBuildSpot(spots);
        }
        #endregion
    }
}