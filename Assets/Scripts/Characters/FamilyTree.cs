using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class FamilyTree {

    public PreCharacterData father { get; set; }
    public PreCharacterData mother { get; set; }
    public List<PreCharacterData> children { get; set; }
    public List<PreCharacterData> allFamilyMembers { get; set; } //this is only for convenience. Contains mother, father and children

    public RACE race => father.race;

    public FamilyTree() {
        allFamilyMembers = new List<PreCharacterData>();
    }
    
    public FamilyTree(PreCharacterData _father, PreCharacterData _mother, List<PreCharacterData> _children) : this() {
        father = _father;
        mother = _mother;
        children = _children;
        allFamilyMembers.Add(father);
        allFamilyMembers.Add(mother);
        allFamilyMembers.AddRange(children);
    }
}
