using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(UIHoverHandler))]
public class FactionEmblem : MonoBehaviour, IPointerClickHandler{

    private Faction faction;

    [SerializeField] private Image emblemImage;

    public void SetFaction(Faction faction) {
        this.faction = faction;
        UpdateEmblem();
    }
    public void ShowFactionInfo() {
        if (this.faction == null) {
            return;
        }
        string text = $"{this.faction.name}\nRelationship Summary:";
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            text += $"\n{kvp.Key.name} - {kvp.Value.relationshipStatus}";
        }
        UIManager.Instance.ShowSmallInfo(text);
    }
    public void HideSmallInfo() {
        if (this.faction == null) {
            return;
        }
#if !WORLD_CREATION_TOOL
        UIManager.Instance.HideSmallInfo();
#endif
    }

    private void UpdateEmblem() {
        if (faction == null || !faction.isMajorFaction) {
            this.gameObject.SetActive(false);   
        } else {
            this.gameObject.SetActive(true);
            emblemImage.sprite = faction.emblem;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (faction != null) {
            UIManager.Instance.ShowFactionInfo(faction);
        }
    }
}
