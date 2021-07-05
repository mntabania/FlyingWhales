using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(UIHoverHandler))]
public class FactionEmblem : MonoBehaviour, IPointerClickHandler{

    private Faction faction;

    [SerializeField] private Image emblemImage;
    [SerializeField] private bool alwaysShowEmblem = false;
    
    public void SetFaction(Faction faction) {
        this.faction = faction;
        UpdateEmblem();
    }
    public void ShowFactionInfo() {
        if (this.faction == null) {
            return;
        }
        string text = $"{this.faction.nameWithColor}\nRelationship Summary:";
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            text += $"\n{kvp.Key.nameWithColor} - {kvp.Value.relationshipStatus}";
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        text = $"{text}\nTesting Info:";
        text = $"{text}\nPathfinding Tag: {faction.pathfindingTag.ToString()}";
        text = $"{text}\nDoor Pathfinding Tag: {faction.pathfindingDoorTag.ToString()}";
#endif
        UIManager.Instance.ShowSmallInfo(text, autoReplaceText: false);
    }
    public void HideSmallInfo() {
        if (this.faction == null) {
            return;
        }
        UIManager.Instance.HideSmallInfo();
    }

    private void UpdateEmblem() {
        if (alwaysShowEmblem) {
            //if always show emblem is set to true then do not check if faction is a major faction or not.
            if (faction == null) {
                this.gameObject.SetActive(false);   
            } else {
                this.gameObject.SetActive(true);
                emblemImage.sprite = faction.emblem;
            }    
        } else {
            if (faction == null || faction.factionType.type == FACTION_TYPE.Wild_Monsters) {
                this.gameObject.SetActive(false);   
            } else {
                this.gameObject.SetActive(true);
                emblemImage.sprite = faction.emblem;
            }
        }
        
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (faction != null) {
            UIManager.Instance.ShowFactionInfo(faction);
        }
    }
}
