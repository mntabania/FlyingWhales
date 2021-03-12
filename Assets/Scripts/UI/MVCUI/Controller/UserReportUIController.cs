using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class UserReportUIController : MVCUIController, UserReportUIView.IListener {

	[SerializeField]
	private UserReportUIModel m_userReportUIModel;
	private UserReportUIView m_userReportSkillUIView;

	private void Start() {
		InstantiateUI();
	}

	public override void HideUI() {
		base.HideUI();
	}
	public override void ShowUI() {
		m_mvcUIView.ShowUI();
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_userReportSkillUIView == null) {
			UserReportUIView.Create(_canvas, m_userReportUIModel, (p_ui) => {
				m_userReportSkillUIView = p_ui;
				m_userReportSkillUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				HideUI();
			});
		} else {
			ShowUI();
			int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 4;
			m_userReportSkillUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);
			m_userReportSkillUIView.GetUserReportingScript().CreateUserReport();
		}
	}

	#region Listeners
	public void OnSubmitClicked() {
		m_userReportSkillUIView.GetUserReportingScript().SubmitUserReport();
	}
	public void OnCloseClicked() {
		HideUI();
	}
	#endregion
}