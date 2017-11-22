﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PathFindingThread {
	public enum TODO{
		FIND_PATH,
	}

	private TODO _todo;

	public List<HexTile> receivedPath;
	private HexTile _startingTile;
	private HexTile _destinationTile;
	private PATHFINDING_MODE _pathfindingMode;
	private Kingdom _kingdom;
	private CitizenAvatar _citizenAvatar;

	public PathFindingThread(CitizenAvatar citizenAvatar, HexTile startingTile, HexTile destinationTile, PATHFINDING_MODE pathfindingMode, Kingdom kingdom = null){
		receivedPath = new List<HexTile> ();
		this._startingTile = startingTile;
		this._destinationTile = destinationTile;
		this._pathfindingMode = pathfindingMode;
		this._kingdom = kingdom;
		this._citizenAvatar = citizenAvatar;
	}

	public void FindPath(){
		bool isStartingTileRoad = _startingTile.isRoad;
		bool isDestinationTileRoad = _destinationTile.isRoad;

		if (_pathfindingMode == PATHFINDING_MODE.USE_ROADS || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_WITH_ALLIES || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE
			|| _pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS || _pathfindingMode == PATHFINDING_MODE.MINOR_ROADS 
			|| _pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM || _pathfindingMode == PATHFINDING_MODE.MINOR_ROADS_ONLY_KINGDOM) {
			_startingTile.isRoad = true;
			_destinationTile.isRoad = true;
		}


		Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt (Math.Pow (t.xCoordinate - _destinationTile.xCoordinate, 2) + Math.Pow (t.yCoordinate - _destinationTile.yCoordinate, 2));

		var path = PathFind.PathFind.FindPath (_startingTile, _destinationTile, distance, estimate, _pathfindingMode, _kingdom);

		if (_pathfindingMode == PATHFINDING_MODE.USE_ROADS || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_WITH_ALLIES || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE
			|| _pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS || _pathfindingMode == PATHFINDING_MODE.MINOR_ROADS 
			|| _pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM || _pathfindingMode == PATHFINDING_MODE.MINOR_ROADS_ONLY_KINGDOM) {
			_startingTile.isRoad = isStartingTileRoad;
			_destinationTile.isRoad = isDestinationTileRoad;
		}

		if (path != null) {
			if (_pathfindingMode == PATHFINDING_MODE.COMBAT || _pathfindingMode == PATHFINDING_MODE.ROAD_CREATION 
				|| _pathfindingMode == PATHFINDING_MODE.NO_MAJOR_ROADS || _pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE) {

				receivedPath = path.Reverse ().ToList ();
			} else {
				receivedPath = path.Reverse ().ToList ();
				if (receivedPath.Count > 1) {
					receivedPath.RemoveAt (0);
				}
			}
		}else{
			receivedPath = null;
		}
	}

	public void ReturnPath(){
		this._citizenAvatar.ReceivePath (receivedPath);
	}
}
