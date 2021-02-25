using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Linq;
using System.Collections.Generic;

public class DefensePointUIController : MVCUIController, DefensePointUIView.IListener {

	[SerializeField]
	private DefensePointUIModel m_defensePointUIModel;
	private DefensePointUIView m_defensePointUIView;

	[SerializeField]
	private AvailableMonsterItemUI m_availableMonsterItemUI; //item to instantiate
	private List<AvailableMonsterItemUI> m_availableMonsters = new List<AvailableMonsterItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	private List<DeployedMonsterItemUI> m_deployedMonsters = new List<DeployedMonsterItemUI>();

	public FakePlayer fakePlayer;

	public bool isTestScene;

	private int m_defaultUnlockedCount = 3;

	private void Start() {
		
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onSpireClicked -= OnSpireClicked;
		}
	}

	private void OnSpireClicked() {
		if (GameManager.Instance.gameHasStarted) {
			Init();
		}
	}

	public void Init() {
		
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_defensePointUIView == null) {
			DefensePointUIView.Create(_canvas, m_defensePointUIModel, (p_ui) => {
				m_defensePointUIView = p_ui;
				m_defensePointUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				ShowUI();
			});
		}
	}

	void OnAvailableMonsterClicked() {
		
	}

	void OnDeployedMonsterClicked() {

	}

	void OnUnlockClicked() { 
	
	}

	#region BiolabUIView.IListener implementation
	public void OnDeployClicked() {
		
	}

	public void OnCloseClicked() {
		HideUI();
	}
	#endregion
}