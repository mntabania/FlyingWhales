using System;
using System.Collections;
using System.Collections.Generic;
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
	
	public void Initialize() {
		GameDate expiry = GameManager.Instance.Today();
		expiry = expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(ExpiryInHours));
		expiryKey = SchedulingManager.Instance.AddEntry(expiry, Expire, this);
		
		randomPos = transform.position;
		randomPos.x += Random.Range(-1.5f, 1.5f);
		randomPos.y += Random.Range(-1.5f, 1.5f);
		positionCoroutine = StartCoroutine(GoTo(randomPos, 0.5f));
	}
	private IEnumerator GoTo(Vector3 targetPos, float smoothTime, System.Action onReachAction = null) {
		while (Mathf.Approximately(transform.position.x, targetPos.x) == false && 
		       Mathf.Approximately(transform.position.y, targetPos.y) == false) {
			transform.position = Vector3.SmoothDamp(transform.position, targetPos,  ref velocity, smoothTime);
			yield return null;
		}
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
	}
	public void OnPointerClick(BaseEventData data) {
		// if (positionCoroutine != null) {
		// 	StopCoroutine(positionCoroutine);	
		// }
		// _collider.enabled = false;
		// Vector3 manaContainerPos = PlayerUI.Instance.manaContainer.TransformPoint(PlayerUI.Instance.manaContainer.rect.center);
		// StartCoroutine(GoTo(manaContainerPos, 0.7f, GainMana));
		GainMana();
	}
	private void GainMana() {
		int randomMana = Random.Range(5, 11);
		PlayerManager.Instance.player.AdjustMana(randomMana);
		Destroy();
	}
	public override void Reset() {
		base.Reset();
		_collider.enabled = true;
		positionCoroutine = null;
	}
}
