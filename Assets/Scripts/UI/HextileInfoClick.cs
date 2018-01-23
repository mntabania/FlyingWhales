﻿using UnityEngine;
using System.Collections;

public class HextileInfoClick : MonoBehaviour {
	void OnClick(){
		UILabel lbl = GetComponent<UILabel> ();
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_landmark")){
				HexTile hextile = UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile;
				if(hextile != null && hextile.landmarkOnTile != null && hextile.landmarkOnTile.id == idToUse){
					UIManager.Instance.ShowSettlementInfo (hextile.landmarkOnTile);
				}
			} else if(url.Contains("_character")){
				HexTile hextile = UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile;
				ECS.Character character = hextile.GetCharacterByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}
            } else if (url.Contains("_quest")) {
                Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
                if (quest != null) {
                    UIManager.Instance.ShowQuestInfo(quest);
                }
            }
        }
	}
}
