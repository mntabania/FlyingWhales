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
    [SerializeField] private RuinarchToggle membersToggle;

    [Header("Faction Scroll Snap")]
    [SerializeField] private GameObject factionItemPrefab;
    //[SerializeField] private GameObject factionItemPaginationPrefab;
    [SerializeField] private HorizontalScrollSnap factionScrollSnap;
    [SerializeField] private Transform factionScrollSnapContent;
    //[SerializeField] private Transform factionScrollSnapPagination;
    //[SerializeField] private ToggleGroup factionScrollSnapPaginationGroup;

    [Header("Faction Info UI V2")]
    [SerializeField] private FactionInfoUIV2 factionInfoUI;

    public List<FactionItem> factionItems;
    //private List<GameObject> factionPaginationGOs;

    //private int lastIndex = 0;

    #region getters
    public FactionItem currentSelectedFactionItem => GetCurrentFactionItem();
    public bool isShowing => parentPanelGO.gameObject.activeInHierarchy;
    #endregion

    #region General
    void Awake() {
        Instance = this;
    }
    void Start() {
        factionItems = new List<FactionItem>();
        //factionPaginationGOs = new List<GameObject>();
    }
    private void InitializeUI() {
        //Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
        Messenger.AddListener(FactionSignals.FORCE_FACTION_UI_RELOAD, ForceFactionReload);
        Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
    }
    private void OnFactionCreated(Faction p_createdFaction) {
        if (p_createdFaction.factionType.type == FACTION_TYPE.Ratmen) {
            AddFactionItem(p_createdFaction);
        }
    }

    public void InitializeAfterGameLoaded() {
        factionInfoUI.Initialize();
        PopulateInitialFactions();
        InitializeUI();
    }
    private void PopulateInitialFactions() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if(faction.isMajorNonPlayer && !faction.isDisbanded) {
                AddFactionItem(faction);
            }
        }
        if (FactionManager.Instance.vagrantFaction != null) {
            AddFactionItem(FactionManager.Instance.vagrantFaction);
        }
        if(FactionManager.Instance.undeadFaction != null) {
            AddFactionItem(FactionManager.Instance.undeadFaction);
        }
        if (FactionManager.Instance.ratmenFaction != null) {
            AddFactionItem(FactionManager.Instance.ratmenFaction);
        }
        InitialFactionItemStates();
    }
    private IEnumerator RepopulateFactions() { //IEnumerator
        //int currentPage = factionScrollSnap.CurrentPage;
        UtilityScripts.Utilities.DestroyChildren(factionScrollSnapContent);
        //UtilityScripts.Utilities.DestroyChildren(factionScrollSnapPagination);
        //factionPaginationGOs.Clear();
        factionItems.Clear();
        //yield return null;
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.isMajorNonPlayer && !faction.isDisbanded) {
                FactionItem item = AddFactionItem(faction);
                SetFactionSelection(item, false);
            }
        }
        if (FactionManager.Instance.vagrantFaction != null) {
            FactionItem item = AddFactionItem(FactionManager.Instance.vagrantFaction);
            SetFactionSelection(item, false);
        }
        if (FactionManager.Instance.undeadFaction != null) {
            FactionItem item = AddFactionItem(FactionManager.Instance.undeadFaction);
            SetFactionSelection(item, false);
        }
        if (FactionManager.Instance.ratmenFaction != null) {
            FactionItem item = AddFactionItem(FactionManager.Instance.ratmenFaction);
            SetFactionSelection(item, false);
        }
        //if(currentPage >= factionItems.Count || currentPage < 0) {
        //    currentPage = 0;
        //}
        if (isShowing) {
            factionScrollSnap.UpdateChildrenAndPagination();
            yield return null;
            factionScrollSnap.GoToScreen(0);
        }
    }

    public void Open() {
        parentPanelGO.SetActive(true);
    }
    public void Close() {
        parentPanelGO.SetActive(false);
    }
    public void ShowMembers() {
        membersToggle.isOn = true;
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
            StartCoroutine(GoToScreen(index));
        }
    }
    private IEnumerator GoToScreen(int index) {
        yield return null;
        factionScrollSnap.GoToScreen(index);
    }
    private FactionItem GetCurrentFactionItem() {
        if(factionScrollSnap.CurrentPage >= 0 && factionScrollSnap.CurrentPage < factionItems.Count && factionItems.Count > 0) {
            return factionItems[factionScrollSnap.CurrentPage];
        }
        return currentSelectedFactionItem;
    }
    #endregion

    #region Listeners
    private void OnFactionCreated(Faction faction, Character creator) {
        if (GameManager.Instance.gameHasStarted) {
            if (faction.isMajorNonPlayer || faction.factionType.type == FACTION_TYPE.Vagrants || faction.factionType.type == FACTION_TYPE.Undead || faction.factionType.type == FACTION_TYPE.Ratmen) {
                //FactionItem item = AddFactionItem(faction);
                //SetFactionSelection(item, false);
                //RepopulateFactions();
                StartCoroutine(RepopulateFactions());
            }
        }
    }
    private void OnFactionDisbanded(Faction faction) {
        if (GameManager.Instance.gameHasStarted) {
            if (faction.isMajorNonPlayer) {
                StartCoroutine(RepopulateFactions());
            }
        }
    }
    private void ForceFactionReload() {
        StartCoroutine(RepopulateFactions());
    }
    private void OnLoadoutSelected() {
        if (factionInfoUI.activeFaction != null) {
            factionInfoUI.UpdateAllRelationships();    
        }
    }
    #endregion

    #region Faction Item
    private FactionItem AddFactionItem(Faction faction) {
        if (!HasFactionItem(faction)) {
            //if(faction?.factionType.type == FACTION_TYPE.Vagrants || faction?.factionType.type == FACTION_TYPE.Undead) {
            //    lastIndex++;
            //}
            FactionItem factionItem = CreateFactionItem(faction);
            CreateFactionItemPagination();
            factionItems.Add(factionItem);

            //if (faction?.factionType.type != FACTION_TYPE.Vagrants && faction?.factionType.type != FACTION_TYPE.Undead) {
            //    int index = factionItems.Count - lastIndex;
            //    factionItems.Insert(index, factionItem);
            //} else {
            //    factionItems.Add(factionItem);
            //}
            return factionItem;
        }
        return null;
    }
    private void RemoveFactionItem(Faction faction) {
        int index = GetFactionItemIndex(faction);
        if (index != -1) {
            if(factionScrollSnap.CurrentPage == index) {
                factionScrollSnap.GoToScreen(0);
            }
            
            FactionItem item = factionItems[index];
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            factionItems.RemoveAt(index);
            //DestroyFactionItemPaginationInIndex(index);
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
    public void UpdateFactionItem(Faction faction) {
        FactionItem item = GetFactionItem(faction);
        if(item != null) {
            item.UpdateFaction();
        }
    }
    private void CreateFactionItemPagination() {
        //GameObject go = Instantiate(factionItemPaginationPrefab, Vector3.zero, Quaternion.identity, factionScrollSnapPagination);
        //go.GetComponent<Toggle>().group = factionScrollSnapPaginationGroup;
        //factionPaginationGOs.Add(go);
    }
    private void DestroyFactionItemPaginationInIndex(int index) {
        //Destroy(factionPaginationGOs[index]);
        //factionPaginationGOs.RemoveAt(index);
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
            return /*currentSelectedFactionItem.faction == faction && */factionInfoUI.activeFaction == faction;
        }
        return false;
    }
    public void FilterTrait(string traitName) {
        factionInfoUI.AddFilteredTrait(traitName);
    }
    public void UnFilterTrait(string traitName) {
        factionInfoUI.RemoveFilteredTrait(traitName);
    }
    public void FilterRegion(BaseSettlement village) {
        factionInfoUI.AddFilteredRegion(village);
    }
    public void UnFilterRegion(BaseSettlement village) {
        factionInfoUI.RemoveFilteredRegion(village);
    }
    #endregion
}
