﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Camp {

	internal HexTile hexTile;
	internal City targetCity;
	internal int hp;
	internal int maxHp;

	public Camp(HexTile hexTile){
		this.hexTile = hexTile;
		this.hp = 100;
		this.maxHp = 500;
		this.targetCity = null;
		Messenger.AddListener("CityEverydayActions", CampEverydayActions);
	}
	internal virtual void CampEverydayActions(){
		
	}

}
