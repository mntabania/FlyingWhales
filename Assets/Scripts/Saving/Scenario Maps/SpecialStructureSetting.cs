using UnityEngine;

[System.Serializable]
public struct SpecialStructureSetting {
    public Point location;
    public STRUCTURE_TYPE structureType;

    public SpecialStructureSetting(Point p_location, STRUCTURE_TYPE p_structureType) {
        location = p_location;
        structureType = p_structureType;
    }
}