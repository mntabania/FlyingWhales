using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;

namespace Traits {
    public class ChainedElectric : Trait {

        public int damage { get; private set; }
        public bool hasInflictedDamage { get; private set; }
        public ITraitable traitable { get; private set; }

        public override System.Type serializedData => typeof(SaveDataChainedElectric);

        public ChainedElectric() {
            name = "Chained Electric";
            description = "Affected by electric chain damage.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            traitable = sourcePOI;
            GameManager.Instance.StartCoroutine(InflictDamageEnumerator(traitable));
            //GameDate dueDate = GameManager.Instance.Today().AddTicks(1);
            //SchedulingManager.Instance.AddEntry(dueDate, () => InflictDamage(sourcePOI), sourcePOI);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            traitable = null;
        }
        #endregion

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitable = addTo;
        }
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataChainedElectric data = saveDataTrait as SaveDataChainedElectric;
            damage = data.damage;
            hasInflictedDamage = data.hasInflictedDamage;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            if (!hasInflictedDamage) {
                GameManager.Instance.StartCoroutine(InflictDamageEnumerator(traitable));
                //GameDate dueDate = GameManager.Instance.Today().AddTicks(1);
                //SchedulingManager.Instance.AddEntry(dueDate, () => InflictDamage(traitable), traitable);
            }
        }
        #endregion

        private IEnumerator InflictDamageEnumerator (ITraitable traitable) {
            while (GameManager.Instance.isPaused) {
                //Pause coroutine while game is paused
                //Might be performance heavy, needs testing
                yield return null;
            }
            yield return new WaitForSeconds(0.5f * GameManager.Instance.progressionSpeed); // * GameManager.Instance.progressionSpeed);
            InflictDamage(traitable);
        }
        public void SetDamage(int amount) {
            damage = amount;
        }
        private void InflictDamage(ITraitable traitable) {
            if(traitable == null) {
                return;
            }
            if (!hasInflictedDamage) {
                hasInflictedDamage = true;
                int chainDamage = Mathf.RoundToInt(damage * 0.8f);
                if (chainDamage >= 0) {
                    chainDamage = -1;
                }
                List<LocationGridTile> neighbours = traitable.gridTileLocation.neighbourList;
                for (int i = 0; i < neighbours.Count; i++) {
                    LocationGridTile tile = neighbours[i];
                    if (tile.genericTileObject.traitContainer.HasTrait("Wet") && !tile.genericTileObject.traitContainer.HasTrait("Zapped", "Chained Electric")) {
                        tile.PerformActionOnTraitables((t) => ChainElectricEffect(t, chainDamage, responsibleCharacter));
                    }
                }
            }
        }
        private void ChainElectricEffect(ITraitable traitable, int damage, Character responsibleCharacter) {
            traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, true, source: responsibleCharacter, showHPBar: true);
        }
    }
}


#region Save Data
[System.Serializable]
public class SaveDataChainedElectric : SaveDataTrait {
    public int damage;
    public bool hasInflictedDamage;
    public override void Save(Trait trait) {
        base.Save(trait);
        ChainedElectric chainedElectric = trait as ChainedElectric;
        damage = chainedElectric.damage;
        hasInflictedDamage = chainedElectric.hasInflictedDamage;
    }
}
#endregion