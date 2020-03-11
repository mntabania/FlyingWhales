using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Elemental Damage Data", menuName = "Scriptable Objects/Elemental Data")]
public class ElementalDamageData : ScriptableObject {
    [SerializeField] private ELEMENTAL_TYPE _type;
    [SerializeField] private string _addedTraitName;
    [SerializeField] private GameObject _hitEffectPrefab;

    #region getters
    public ELEMENTAL_TYPE type => _type;
    public string addedTraitName => _addedTraitName;
    public GameObject hitEffectPrefab => _hitEffectPrefab;
    #endregion
}