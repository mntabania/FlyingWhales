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
    
    private Camera _cameraToUse;
    private Dictionary<STRUCTURE_TYPE, GameObject> _structureVisuals;
    private bool _isShowing;
    private GameObject _goActiveStructure;
    private bool _followMouse;
    
    public void Initialize(Camera p_camera) {
        _cameraToUse = p_camera;
        List<STRUCTURE_TYPE> playerStructureTypes = new List<STRUCTURE_TYPE>() {
            STRUCTURE_TYPE.THE_PORTAL, STRUCTURE_TYPE.EYE, STRUCTURE_TYPE.BIOLAB, STRUCTURE_TYPE.DEFILER, STRUCTURE_TYPE.TORTURE_CHAMBERS, STRUCTURE_TYPE.KENNEL, STRUCTURE_TYPE.MEDDLER
        }; //CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>().Where(s => s.IsPlayerStructure()).ToList();
        _structureVisuals = new Dictionary<STRUCTURE_TYPE, GameObject>();
        for (int i = 0; i < playerStructureTypes.Count; i++) {
            STRUCTURE_TYPE structureType = playerStructureTypes[i];
            List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureType, RESOURCE.NONE);
            GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
            GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, transformInactiveStructuresParent);
            LocationStructureObject structureTemplate = structureGO.GetComponent<LocationStructureObject>();
            structureTemplate.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint, null);
            structureTemplate.OverrideDefaultSortingOrder(1000);
            _structureVisuals.Add(structureType, structureGO);
            structureGO.SetActive(false);
        }
    }
    public void Show(STRUCTURE_TYPE p_structureType) {
        _isShowing = true;
        _goActiveStructure = GetStructureVisual(p_structureType);
        _goActiveStructure.transform.SetParent(transformActiveStructuresParent);
        _goActiveStructure.SetActive(true);
        transformColorTint.gameObject.SetActive(true);
        SetFollowMouseState(true);
    }
    public void Hide() {
        _isShowing = false;
        _goActiveStructure.SetActive(false);
        _goActiveStructure.transform.SetParent(transformInactiveStructuresParent);
        transformColorTint.gameObject.SetActive(false);
        SetFollowMouseState(false);
    }
    private GameObject GetStructureVisual(STRUCTURE_TYPE p_structureType) {
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
            // Vector3 screenToWorldPoint = _cameraToUse.ScreenToWorldPoint(Input.mousePosition);
            // screenToWorldPoint.z = 0f;
            if (tile != null) {
                transformActiveStructuresParent.transform.position = tile.centeredWorldLocation;    
            }
        }
    }
}
