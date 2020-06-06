using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class ChaosOrb : PooledObject {

	private const int ExpiryInHours = 2;

	private string expiryKey;
	private Coroutine positionCoroutine;
	private Vector3 randomPos;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Collider2D _collider;
	[SerializeField] private TrailRenderer _trail;
	
	public void Initialize() {
		GameDate expiry = GameManager.Instance.Today();
		expiry = expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(ExpiryInHours));
		expiryKey = SchedulingManager.Instance.AddEntry(expiry, Expire, this);
		
		randomPos = transform.position;
		randomPos.x += Random.Range(-1.5f, 1.5f);
		randomPos.y += Random.Range(-1.5f, 1.5f);
		positionCoroutine = StartCoroutine(GoTo(randomPos, 0.5f));
		_collider.enabled = true;
		_trail.enabled = false;
	}
	private IEnumerator GoTo(Vector3 targetPos, float smoothTime, System.Action onReachAction = null) {
		while (Mathf.Approximately(transform.position.x, targetPos.x) == false && 
		       Mathf.Approximately(transform.position.y, targetPos.y) == false) {
			transform.position = Vector3.SmoothDamp(transform.position, targetPos,  ref velocity, smoothTime);
			yield return null;
		}
		Vector3 finalPos = transform.position;
		transform.position = new Vector3(finalPos.x, finalPos.y, -1f);
		onReachAction?.Invoke();
		positionCoroutine = null;
	}
	
	private void Expire() {
		Destroy();
	}
	private void Destroy() {
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);	
		}
		ObjectPoolManager.Instance.DestroyObject(this);
		PlayerManager.Instance.RemoveChaosOrbFromAvailability(this);
	}
	public void OnPointerClick(BaseEventData data) {
		if (positionCoroutine != null) {
			StopCoroutine(positionCoroutine);	
		}
		_collider.enabled = false;
		_trail.enabled = true;
		Vector3 manaContainerPos = InnerMapCameraMove.Instance.innerMapsCamera.ScreenToWorldPoint(PlayerUI.Instance.manaLbl.transform.position);

		Vector3 controlPointA = transform.position;
		controlPointA.x += 5f;
		
		Vector3 controlPointB = manaContainerPos;
		controlPointB.y -= 5f;
		
		transform.DOPath(new[] {manaContainerPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier)
			.SetEase(Ease.InSine)
			.OnComplete(GainMana);
		Messenger.Broadcast(Signals.CHAOS_ORB_CLICKED);
	}
	private void GainMana() {
		int randomMana = Random.Range(5, 11);
		PlayerManager.Instance.player.AdjustMana(randomMana);
		PlayerUI.Instance.DoManaPunchEffect();
		AudioManager.Instance.PlayParticleMagnet();
		Destroy();
	}
	public override void Reset() {
		base.Reset();
		_trail.Clear();
		_collider.enabled = true;
		positionCoroutine = null;
	}
}
