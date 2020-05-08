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
        BurningSource bs = null;
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, 0, true); //radius
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables((traitable) => MeteorEffect(traitable, ref bs));
        }

        //Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_TILE, targetTile, 10);
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        Messenger.Broadcast(Signals.METEOR_FELL);
        InnerMapCameraMove.Instance.MeteorShake();
        GameManager.Instance.StartCoroutine(ExpireCoroutine(gameObject));
        
    }
    private void MeteorEffect(ITraitable traitable, ref BurningSource bs) {
        if (traitable.gridTileLocation == null) { return; }
        BurningSource burningSource = bs;
        traitable.AdjustHP(-500, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //if (traitable is TileObject obj) {
        //    if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
        //        obj.AdjustHP(-500, ELEMENTAL_TYPE.Fire, 
        //            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //    } else {
        //        CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj, 
        //            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource));
        //    }
        //} else if (traitable is Character character) {
        //    character.AdjustHP(-500, ELEMENTAL_TYPE.Fire, true, 
        //        elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //} else {
        //    traitable.AdjustHP(-500, ELEMENTAL_TYPE.Fire, 
        //        elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //}
        bs = burningSource;
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
}
