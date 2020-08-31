using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Databases {
    public class TraitDatabase {
        public Dictionary<string, Trait> traitsByGUID { get; }

        public TraitDatabase() {
            traitsByGUID = new Dictionary<string, Trait>();
        }

        public void RegisterTrait(Trait trait) {
            Assert.IsTrue(TraitManager.Instance.IsInstancedTrait(trait.name), $"({trait.name})Un-instanced trait is being registered on trait database, it should not be registered/unregistered");
            traitsByGUID.Add(trait.persistentID, trait);
        }
        public void UnRegisterTrait(Trait trait) {
            Assert.IsTrue(TraitManager.Instance.IsInstancedTrait(trait.name), $"({trait.name})Un-instanced trait is being unregistered from trait database, it should not be registered/unregistered");
            traitsByGUID.Remove(trait.persistentID);
        }
        public Trait GetTraitByPersistentID(string id) {
            if (traitsByGUID.ContainsKey(id)) {
                return traitsByGUID[id];
            }
            throw new Exception($"There is no trait with persistent ID {id}");
        }
    }
}