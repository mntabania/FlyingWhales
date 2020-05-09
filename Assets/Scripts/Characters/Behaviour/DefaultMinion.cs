﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;

public class DefaultMinion : CharacterBehaviourComponent {
	public DefaultMinion() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will roam around assigned area!";
        if(character.behaviourComponent.assignedTargetSettlement != null) {
            NPCSettlement assignedSettlement = character.behaviourComponent.assignedTargetSettlement;
            HexTile chosenHex = CollectionUtilities.GetRandomElement(assignedSettlement.tiles);
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
            character.jobComponent.TriggerRoamAroundTile(chosenTile);
            return true;
        } else if (character.behaviourComponent.assignedTargetHex != null) {
            HexTile chosenHex = character.behaviourComponent.assignedTargetHex;
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
            character.jobComponent.TriggerRoamAroundTile(chosenTile);
            return true;
        }
        //      if (character.minion != null) {
        //	log += $"\n-{character.name} is minion";
        //	if (character.gridTileLocation != null) {
        //		HexTile portal = PlayerManager.Instance.player.portalTile;
        //		List<HexTile> playerTiles = PlayerManager.Instance.player.playerSettlement.tiles;
        //		if (playerTiles.Contains(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
        //			//bool hasAddedJob = false;
        //			log += "\n-Inside corruption";
        //			if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner != portal) {
        //				log += "\n-Inside portal hex tile";
        //				log += "\n-Roam Around Portal";
        //				character.jobComponent.TriggerRoamAroundPortal();
        //			} else {
        //				log += "\n-50% chance Roam Around Corruption";
        //				int roll = UnityEngine.Random.Range(0, 100);
        //				log += $"\n-Roll: {roll}";
        //				if (roll < 50) {
        //					log += "\n-Roam Around Corruption";
        //					character.jobComponent.TriggerRoamAroundCorruption();
        //				} else {
        //					log += "\n-Return To Portal";
        //					character.jobComponent.TriggerReturnPortal();
        //				}
        //			}
        //		} else {
        //			log += "\n-Outside corruption";
        //			int fiftyPercentOfMaxHP = Mathf.RoundToInt(character.maxHP * 0.5f);
        //			if (character.currentHP < fiftyPercentOfMaxHP) {
        //				log += "\n-Less than 50% of Max HP, Return To Portal";
        //				character.jobComponent.TriggerReturnPortal();
        //			} else {
        //				log += "\n-50% chance to Roam Around Tile";
        //				int roll = UnityEngine.Random.Range(0, 100);
        //				log += $"\n-Roll: {roll}";
        //				if (roll < 50) {
        //					log += "\n-Roam Around Tile";
        //					character.jobComponent.TriggerRoamAroundTile();
        //				} else {
        //					log += "\n-Return To Portal";
        //					character.jobComponent.TriggerReturnPortal();
        //				}
        //			}
        //		}
        //	}
        //	return true;
        //}
        return false;
	}
}
