using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerStructurePlacementVisual : MonoBehaviour {

    [SerializeField] private Transform transformInactiveStructuresParent;
    [SerializeField] private Transform transformColorTint;
    [SerializeField] private SpriteRenderer spriteRendererColorTint;
    [SerializeField] private Transform transformActiveStructuresParent;
    private Dictionary<STRUCTURE_TYPE, LocationStructureObject> _structureVisuals;
    private bool _isShowing;
    private LocationStructureObject _goActiveStructure;
    private bool _followMouse;

    private const float Tile_Size = 0.14f;
    
    public void Initialize(Camera p_camera) {
        List<STRUCTURE_TYPE> playerStructureTypes = new List<STRUCTURE_TYPE>() {
            STRUCTURE_TYPE.THE_PORTAL, STRUCTURE_TYPE.WATCHER, STRUCTURE_TYPE.BIOLAB, STRUCTURE_TYPE.DEFILER, STRUCTURE_TYPE.TORTURE_CHAMBERS, STRUCTURE_TYPE.KENNEL, 
            STRUCTURE_TYPE.MEDDLER, STRUCTURE_TYPE.SPIRE, STRUCTURE_TYPE.MANA_PIT, STRUCTURE_TYPE.MARAUD, STRUCTURE_TYPE.DEFENSE_POINT, STRUCTURE_TYPE.CRYPT, STRUCTURE_TYPE.IMP_HUT,

        }; //CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>().Where(s => s.IsPlayerStructure()).ToList();
        _structureVisuals = new Dictionary<STRUCTURE_TYPE, LocationStructureObject>();
        transformColorTint.gameObject.SetActive(false);
        for (int i = 0; i < playerStructureTypes.Count; i++) {
            STRUCTURE_TYPE structureType = playerStructureTypes[i];
            GameObject chosenStructurePrefab = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(new StructureSetting(structureType, RESOURCE.NONE));
            GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, transformInactiveStructuresParent);
            LocationStructureObject structureTemplate = structureGO.GetComponent<LocationStructureObject>();
            structureTemplate.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint, null);
            structureTemplate.OverrideDefaultSortingOrder(1000);
            _structureVisuals.Add(structureType, structureTemplate);
            structureGO.SetActive(false);
        }
    }
    public void Show(STRUCTURE_TYPE p_structureType) {
        _isShowing = true;
        _goActiveStructure = GetStructureVisual(p_structureType);
        _goActiveStructure.transform.SetParent(transformActiveStructuresParent);
        _goActiveStructure.transform.localPosition = Vector3.zero;
        _goActiveStructure.gameObject.SetActive(true);
        transformColorTint.localScale = new Vector3(_goActiveStructure.size.x * Tile_Size, _goActiveStructure.size.y * Tile_Size, 0f);
        Vector3 tintPos = Vector3.zero;
        if (UtilityScripts.Utilities.IsEven(_goActiveStructure.size.x)) {
            tintPos.x = -0.5f;
        }
        if (UtilityScripts.Utilities.IsEven(_goActiveStructure.size.y)) {
            tintPos.y = -0.5f;
        }
        transformColorTint.transform.localPosition = tintPos;
        transformColorTint.gameObject.SetActive(true);
        SetFollowMouseState(true);
    }
    public void Hide() {
        _isShowing = false;
        _goActiveStructure.gameObject.SetActive(false);
        _goActiveStructure.transform.SetParent(transformInactiveStructuresParent);
        transformColorTint.gameObject.SetActive(false);
        SetFollowMouseState(false);
    }
    private LocationStructureObject GetStructureVisual(STRUCTURE_TYPE p_structureType) {
        Assert.IsTrue(_structureVisuals.ContainsKey(p_structureType));
        return _structureVisuals[p_structureType];
    }
    public void SetFollowMouseState(bool p_state) {
        _followMouse = p_state;
    }
    public void SetHighlightColor(Color p_color) {
        spriteRendererColorTint.color = p_color;
    }
    private void Update() {
        if (_isShowing && _followMouse) {
            LocationGridTile tile = InnerMapManager.Instance.GetTileFromMousePosition();
            if (tile != null) {
                transformActiveStructuresParent.transform.position = tile.centeredWorldLocation;    
            }
        }
    }
}
