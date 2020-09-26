using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using EZObjectPools;
using Inner_Maps;
using Locations.Settlements;
using Ruinarch.Custom_UI;

public class FactionInfoHubUI : MonoBehaviour {
    public static FactionInfoHubUI Instance;

    [Header("General")]
    [SerializeField] private GameObject parentPanelGO;
    [SerializeField] private Button closeBtn;

    [Header("Faction Scroll Snap")]
    [SerializeField] private GameObject factionItemPrefab;
    [SerializeField] private GameObject factionItemPaginationPrefab;
    [SerializeField] private HorizontalScrollSnap factionScrollSnap;
    [SerializeField] private Transform factionScrollSnapContent;
    [SerializeField] private Transform factionScrollSnapPagination;
    [SerializeField] private ToggleGroup factionScrollSnapPaginationGroup;

    [Header("Faction Info UI V2")]
    [SerializeField] private FactionInfoUIV2 factionInfoUI;

    private List<FactionItem> factionItems;
    private List<GameObject> factionPaginationGOs;

    #region getters
    public FactionItem currentSelectedFactionItem => factionItems[factionScrollSnap.CurrentPage];
    #endregion

    #region General
    void Awake() {
        Instance = this;
    }
    void Start() {
        factionItems = new List<FactionItem>();
        factionPaginationGOs = new List<GameObject>();
    }
    void OnEnable() {
        
    }
    void OnDisable() {
        
    }
    public void InitializeAfterGameLoaded() {
        factionInfoUI.Initialize();
        PopulateInitialFactions();
        InitializeUI();
    }
    private void PopulateInitialFactions() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if(faction.isMajorNonPlayer || faction.factionType.type == FACTION_TYPE.Vagrants) {
                AddFactionItem(faction);
            }
        }
        InitialFactionItemStates();
    }
    private void InitializeUI() {
        Messenger.AddListener<Faction>(Signals.FACTION_CREATED, OnFactionCreated);
    }

    public void Open() {
        parentPanelGO.SetActive(true);
    }
    public void Close() {
        parentPanelGO.SetActive(false);
    }
    public void OnClickClose() {
        PlayerUI.Instance.SetVillagerTabIsOn(false);
    }
    public void OnSelectionChangeEnd(int index) {
        for (int i = 0; i < factionItems.Count; i++) {
            if(index != i) {
                //factionItems[i].SetSelected(false);
                SetFactionSelection(factionItems[i], false);
            }
        }
        FactionItem item = factionItems[index];
        //item.SetSelected(true);
        SetFactionSelection(item, true);
    }
    private void InitialFactionItemStates() {
        int currentPage = factionScrollSnap.CurrentPage;
        SetFactionSelection(currentSelectedFactionItem, true);
        //factionItems[currentPage].SetSelected(true);
        for (int i = 0; i < factionItems.Count; i++) {
            FactionItem item = factionItems[i];
            if(currentPage != i) {
                SetFactionSelection(item, false);
                //item.SetSelected(false);
            }
        }
    }
    public void ShowFaction(Faction faction) {
        int index = GetFactionItemIndex(faction);
        if(index != -1) {
            PlayerUI.Instance.SetVillagerTabIsOn(true);
            factionScrollSnap.CurrentPage = index;
        }
    }
    #endregion

    #region Listeners
    private void OnFactionCreated(Faction faction) {
        if(faction.isMajorNonPlayer || faction.factionType.type == FACTION_TYPE.Vagrants) {
            AddFactionItem(faction);
        }
    }
    #endregion

    #region Faction Item
    private void AddFactionItem(Faction faction) {
        if (!HasFactionItem(faction)) {
            FactionItem factionItem = CreateFactionItem(faction);
            CreateFactionItemPagination();
            factionItems.Add(factionItem);
        }
    }
    private void RemoveFactionItem(Faction faction) {
        int index = GetFactionItemIndex(faction);
        if (index != -1) {
            if(factionScrollSnap.CurrentPage == index) {
                factionScrollSnap.CurrentPage = 0;
            }
            
            FactionItem item = factionItems[index];
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            factionItems.RemoveAt(index);
            DestroyFactionItemPaginationInIndex(index);
        }
    }
    private bool HasFactionItem(Faction faction) {
        return GetFactionItem(faction) != null;
    }
    private FactionItem GetFactionItem(Faction faction) {
        for (int i = 0; i < factionItems.Count; i++) {
            FactionItem item = factionItems[i];
            if (item.faction == faction) {
                return item;
            }
        }
        return null;
    }
    private int GetFactionItemIndex(Faction faction) {
        for (int i = 0; i < factionItems.Count; i++) {
            FactionItem item = factionItems[i];
            if (item.faction == faction) {
                return i;
            }
        }
        return -1;
    }
    private FactionItem CreateFactionItem(Faction faction) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(factionItemPrefab.name, Vector3.zero, Quaternion.identity, factionScrollSnapContent);
        FactionItem item = go.GetComponent<FactionItem>();
        item.SetFaction(faction);
        return item;
    }
    private void CreateFactionItemPagination() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(factionItemPaginationPrefab.name, Vector3.zero, Quaternion.identity, factionScrollSnapPagination);
        go.GetComponent<RuinarchToggle>().group = factionScrollSnapPaginationGroup;
        factionPaginationGOs.Add(go);
    }
    private void DestroyFactionItemPaginationInIndex(int index) {
        ObjectPoolManager.Instance.DestroyObject(factionPaginationGOs[index]);
        factionPaginationGOs.RemoveAt(index);
    }
    #endregion

    #region Faction UI V2
    public void SetFactionSelection(FactionItem item, bool state) {
        item.SetSelected(state);
        if (state) {
            if (item.faction != null && item.faction != factionInfoUI.activeFaction) {
                factionInfoUI.SetFaction(item.faction);
            }
        } else {
            //Not applicable yet
        }
    }
    public bool IsShowing(Faction faction) {
        if(factionItems.Count > 0) {
            return currentSelectedFactionItem.faction == faction && factionInfoUI.activeFaction == faction;
        }
        return false;
    }
    public void FilterTrait(string traitName) {
        factionInfoUI.AddFilteredTrait(traitName);
    }
    public void UnFilterTrait(string traitName) {
        factionInfoUI.RemoveFilteredTrait(traitName);
    }
    public void FilterRegion(Region region) {
        factionInfoUI.AddFilteredRegion(region);
    }
    public void UnFilterRegion(Region region) {
        factionInfoUI.RemoveFilteredRegion(region);
    }
    #endregion
}
