using UnityEngine;

[System.Serializable]
public struct SpecialStructureSetting {
    public Vector2Int location;
    public STRUCTURE_TYPE structureType;

    public SpecialStructureSetting(Vector2Int p_location, STRUCTURE_TYPE p_structureType) {
        location = p_location;
        structureType = p_structureType;
    }
}