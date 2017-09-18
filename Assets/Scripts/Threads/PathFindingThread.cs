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
		Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt (Math.Pow (t.xCoordinate - _destinationTile.xCoordinate, 2) + Math.Pow (t.yCoordinate - _destinationTile.yCoordinate, 2));

		var path = PathFind.PathFind.FindPath (_startingTile, _destinationTile, distance, estimate, _pathfindingMode, _kingdom);
		if (path != null) {
			if (_pathfindingMode == PATHFINDING_MODE.COMBAT) {
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
