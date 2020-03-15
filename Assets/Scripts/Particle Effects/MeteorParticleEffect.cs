using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;

public class MeteorParticleEffect : BaseParticleEffect {
    public ParticleSystem meteorParticle;
    private bool hasMeteorFell;
    //public ParticleSystem[] meteorExplosionParticles;

    //private LocationGridTile targetTile;
    //private int radius;
    //public void MeteorStrike(LocationGridTile targetTile) { //, int radius
    //    this.targetTile = targetTile;
    //    //this.radius = radius;
    //    meteorParticle.Play();
    //}
    //protected override void PlayParticle() {
    //    base.PlayParticle();
    //    OnMeteorFell();
    //}
    //protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
    //    OnMeteorFell();
    //}
    protected override IEnumerator PlayParticleCoroutine() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        PlayParticle();
        yield return new WaitForSeconds(0.1f);
        if (pauseOnGamePaused && GameManager.Instance.isPaused) {
            PauseParticle();
        }
    }
    protected override void ResetParticle() {
        base.ResetParticle();
        hasMeteorFell = false;
    }
    private void OnMeteorFell() {
        hasMeteorFell = true;
        //for (int i = 0; i < meteorExplosionParticles.Length; i++) {
        //    meteorExplosionParticles[i].Play();
        //}
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, 0, true); //radius
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            if (traitable.gridTileLocation == null) {
                continue; //skip
            }
            if (traitable is TileObject obj) {
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Fire, 
                        elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                } else {
                    CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj, 
                        elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
                    //obj.AdjustHP(0, ELEMENTAL_TYPE.Fire);
                }
            } else if (traitable is Character character) {
                character.AdjustHP(-(int)(character.maxHP * 0.4f), ELEMENTAL_TYPE.Fire, true, 
                    elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
            } else {
                traitable.AdjustHP(-traitable.currentHP, ELEMENTAL_TYPE.Fire, 
                    elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
            }
            // Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
            // if(burningTrait != null && burningTrait.sourceOfBurning == null) {
            //     if(bs == null) {
            //         bs = new BurningSource(traitable.gridTileLocation.parentMap.location);
            //     }
            //     burningTrait.SetSourceOfBurning(bs, traitable);
            // }
            //if (traitable.currentHP > 0 && Random.Range(0, 100) < 60) {
            //    if (traitable.traitContainer.HasTrait("Flammable") &&
            //        !traitable.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")) {
            //        Burning burning = new Burning();
            //        burning.SetSourceOfBurning(bs, traitable);
            //        traitable.traitContainer.AddTrait(traitable, burning);
            //    }
            //}
        }
        //if (!DOTween.IsTweening(InnerMapCameraMove.Instance.innerMapsCamera)) {
        //    Tweener tween = InnerMapCameraMove.Instance.innerMapsCamera.DOShakeRotation(0.8f, new Vector3(8f, 8f, 0f), 25);
        //    tween.OnComplete(OnTweenComplete);
        //}
        //else {

        //}
        InnerMapCameraMove.Instance.MeteorShake();
        GameManager.Instance.StartCoroutine(ExpireCoroutine(gameObject));
    }
    private void OnTweenComplete() {
        //InnerMapCameraMove.Instance.innerMapsCamera.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));
        InnerMapCameraMove.Instance.innerMapsCamera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
    }
    private IEnumerator ExpireCoroutine(GameObject go) {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.DestroyObject(go);
    }

    private void Update() {
        if (!hasMeteorFell) {
            if (meteorParticle.isStopped) {
                OnMeteorFell();
            }
        }
    }

    #region Elemental Traits
    private void ProcessBurningTrait(ITraitable traitable, Trait trait, ref BurningSource burningSource) {
        if (trait is Burning burning) {
            if (burningSource == null) {
                burningSource = new BurningSource(traitable.gridTileLocation.parentMap.region);
            }
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
    #endregion
}
