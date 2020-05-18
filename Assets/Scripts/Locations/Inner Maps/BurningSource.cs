using System.Collections;
using System.Collections.Generic;
using Traits;

public class BurningSource {

    public int id { get; }
    public List<ITraitable> objectsOnFire { get; }
    public Region location { get; }
    
    public BurningSource(Region location) {
        id = UtilityScripts.Utilities.SetID(this);
        objectsOnFire = new List<ITraitable>();
        this.location = location;
        location.innerMap.AddActiveBurningSource(this);
        Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    public void AddObjectOnFire(ITraitable poi) {
        if (objectsOnFire.Contains(poi) == false) {
            objectsOnFire.Add(poi);    
        }
    }
    private void RemoveObjectOnFire(ITraitable poi) {
        if (objectsOnFire.Remove(poi)) {
            if (objectsOnFire.Count == 0) {
                location.innerMap.RemoveActiveBurningSources(this);
                SetAsInactive();
            }
        }
    }
    public bool HasFireInSettlement(NPCSettlement npcSettlement) {
        for (int i = 0; i < objectsOnFire.Count; i++) {
            ITraitable traitable = objectsOnFire[i];
            if (traitable.gridTileLocation != null && traitable.gridTileLocation.IsPartOfSettlement(npcSettlement)) {
                return true;
            }
        }
        return false;
    }

    private void SetAsInactive() {
        Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        Messenger.Broadcast(Signals.BURNING_SOURCE_INACTIVE, this);
    }
    
    #region Listeners
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Burning) {
            RemoveObjectOnFire(traitable);
        }
    }
    #endregion
    
    public override string ToString() {
        return $"Burning Source {id.ToString()}. Objects: {objectsOnFire.Count.ToString()}";
    }
}

[System.Serializable]
public class SaveDataBurningSource {
    public int id;

    public void Save(BurningSource bs) {
        id = bs.id;
    }
}