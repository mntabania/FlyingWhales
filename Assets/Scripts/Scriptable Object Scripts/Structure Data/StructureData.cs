using UnityEngine;
[CreateAssetMenu(fileName = "New Structure Data", menuName = "Scriptable Objects/Structure Data")]
public class StructureData : ScriptableObject {
    [SerializeField] private Sprite structureSprite;
    [SerializeField] private ItemGenerationSetting itemGenerationSetting;
    [SerializeField] private LocationStructurePrefabDictionary structurePrefabs;
    [SerializeField] private LocationStructurePrefabDictionary corruptedStructurePrefabs;
}
