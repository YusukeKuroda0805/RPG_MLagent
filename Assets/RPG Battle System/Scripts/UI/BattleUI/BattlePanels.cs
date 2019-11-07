// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : Pondomaniac
// Created          : 07-06-2016
//
// Last Modified By : Pondomaniac
// Last Modified On : 07-07-2016
// ***********************************************************************
// <copyright file="BattlePanels.cs" company="">

// </copyright>
// <summary></summary>
// ***********************************************************************
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Holoville.HOTween;
using System.Linq;
using System;

/// <summary>
/// Class BattlePanels.
/// </summary>
public class BattlePanels : MonoBehaviour {

    /// <summary>
    /// The action panels
    /// </summary>
    public PanelBattleActionMapper[] ActionPanels;
    /// <summary>
    /// The selected toggle
    /// </summary>
    public Toggle SelectedToggle;
    /// <summary>
    /// The selected character
    /// </summary>
    public static CharactersData SelectedCharacter ;
    /// <summary>
    /// The selected weapon
    /// </summary>
    public static ItemsData SelectedWeapon;
    /// <summary>
    /// The selected spell
    /// </summary>
    public static SpellsData SelectedSpell ;
    /// <summary>
    /// The selected item
    /// </summary>
    public static ItemsData SelectedItem;
    /// <summary>
    /// The fade out time
    /// </summary>
    public float FadeOutTime=2.5f;//The fadeIn animation time
                                  /// <summary>
                                  /// The log text
                                  /// </summary>
    public Text logText;
    /// <summary>
    /// The drop text
    /// </summary>
    public Text DropText;
    /// <summary>
    /// The pop up
    /// </summary>
    public Text PopUp;
    /// <summary>
    /// The drop menu
    /// </summary>
    public GameObject DropMenu;
    /// <summary>
    /// The decision menu
    /// </summary>
    public GameObject DecisionMenu;
    /// <summary>
    /// The action menu
    /// </summary>
    public GameObject ActionMenu;
    /// <summary>
    /// The logic game object
    /// </summary>
    private GameObject logicGameObject ;

    public GameObject FeedBackMenu;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    /// <summary>
    /// Starts this instance.
    /// </summary>
    void Start()
	{
		SelectedWeapon = null;
		SelectedSpell = null;
		SelectedItem = null;

	}


    /// <summary>
    /// Awakes this instance.
    /// </summary>
    void Awake ()
	{
		logicGameObject  = GameObject.FindGameObjectsWithTag(Settings.Logic).FirstOrDefault();
		ToggleFightAction (SelectedToggle); // SelectedToggleはinspector上から指定している
		SelectedCharacter = Main.CharacterList [0];
	}


    /// <summary>
    /// This procedure use the selected item
    /// <param name="toggle">The toggle that sent the action</param>
    /// </summary>
    /// <param name="toggle">The toggle.</param>
    public void ToggleFightAction(Toggle toggle)
	{
		Contract.Requires<MissingComponentException> (toggle != null);
		Contract.Requires<UnassignedReferenceException> (SelectedToggle != null);
        SoundManager.UISound();
        if (toggle.isOn) {
            SoundManager.UISound();
            SelectedToggle = toggle;
			DisplayPanel (EnumBattleAction.Weapon);
		}
	}

    /// <summary>
    /// This procedure use the selected item
    /// <param name="toggle">The toggle that sent the action</param>
    /// </summary>
    /// <param name="toggle">The toggle.</param>
    public void ToggleMagicAction(Toggle toggle)
	{
		Contract.Requires<MissingComponentException> (toggle != null);
		Contract.Requires<UnassignedReferenceException> (SelectedToggle != null);
        SoundManager.UISound();
        if (toggle.isOn) {

			SelectedToggle = toggle;
			DisplayPanel (EnumBattleAction.Magic);
		}
	}

    /// <summary>
    /// This procedure use the selected item
    /// <param name="toggle">The toggle that sent the action</param>
    /// </summary>
    /// <param name="toggle">The toggle.</param>
    public void ToggleItemAction(Toggle toggle)
	{
		Contract.Requires<MissingComponentException> (toggle != null);
		Contract.Requires<UnassignedReferenceException> (SelectedToggle != null);
        SoundManager.UISound();
        if (toggle.isOn) {

			SelectedToggle = toggle;
			DisplayPanel (EnumBattleAction.Item);
		}
	}

    /// <summary>
    /// This procedure use the selected item
    /// <param name="toggle">The toggle that sent the action</param>
    /// </summary>
    /// <param name="toggle">The toggle.</param>
    public void TogglePassAction(Toggle toggle)
	{
		Contract.Requires<MissingComponentException> (toggle != null);
		Contract.Requires<UnassignedReferenceException> (SelectedToggle != null);
        SoundManager.UISound();
        if (toggle.isOn) {

			SelectedToggle = toggle;
			DisplayPanel (EnumBattleAction.None);

			if ( logicGameObject ) 
				logicGameObject.BroadcastMessage ("PassAction");
			

		}
	}

    /// <summary>
    /// This procedure show or hide the different panels
    /// <param name="action">The action that correspond to the panel to display</param>
    /// </summary>
    /// <param name="action">The action.</param>
    void DisplayPanel(EnumBattleAction action) // この手順では、さまざまなパネルを表示または非表示にします
    {
		foreach (PanelBattleActionMapper row in ActionPanels)
		{
			if(row.Panel != null) {

				if (row.BattleAction == action){ 
					row.Panel.SetActive(true);
					row.Panel.SendMessage("Start"); 
				}
				else  row.Panel.SetActive(false); // パネルが一致しないときは子供のパネルを非アクティブにする
			}
		}

	}

    /// <summary>
    /// Fights this instance.
    /// </summary>
    void Fight ()
	{
		Debug.Log ("Fight");
		SendMessageUpwards("DisplayPanel",EnumBattleAction.Weapon);
	}

    /// <summary>
    /// Magics this instance.
    /// </summary>
    void Magic ()
	{
		Debug.Log ("Magic");
		SendMessageUpwards("DisplayPanel",EnumBattleAction.Magic);

	}

    /// <summary>
    /// Items this instance.
    /// </summary>
    void Item ()
	{
		Debug.Log ("Item");
		SendMessageUpwards("DisplayPanel",EnumBattleAction.Item);

	}

    /// <summary>
    /// Logs the text.
    /// </summary>
    /// <param name="text">The text.</param>
    void LogText (string text)
	{
		Debug.Log ("Loging"+text);
		logText.text = text;
	}


    /// <summary>
    /// Shows the drop menu.
    /// </summary>
    /// <param name="text">The text.</param>
    public void ShowDropMenu(string text)
	{
		DropMenu.SetActive (true);
		DropText.text = text;
		float time = 0.75f;

		Sequence actions = new Sequence(new SequenceParms());
		TweenParms parms = new TweenParms().Prop("localScale", DropMenu.transform.localScale*2f ).Ease(EaseType.EaseOutElastic);

		actions.Append(HOTween.To(DropMenu.transform, time, parms));

		actions.Play();

	}

    /// <summary>
    /// Hides the decision.
    /// </summary>
    public void HideDecision()
	{
		DecisionMenu.SetActive (false);
	}

    /// <summary>
    /// Shows the decision.
    /// </summary>
    public void ShowDecision()
	{
		DecisionMenu.SetActive (true);
	}


    /// <summary>
    /// Hides the action menu.
    /// </summary>
    public void HideActionMenu()
	{
		ActionMenu.SetActive (false); // 攻撃、魔法、道具から選ぶ　WeaponBattles.cs のToggleSelect○○
	}

    /// <summary>
    /// Shows the action menu.
    /// </summary>
    public void ShowActionMenu()// アクションメニューを表示する
	{
		ActionMenu.SetActive (true);
	}

    public void ShowFeedBack()// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        FeedBackMenu.SetActive(true);
    }

    public void HideFeedBack()// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        FeedBackMenu.SetActive(false);
    }

    /// <summary>
    /// Declines the decision.
    /// </summary>
    public void DeclineDecision()// キャンセル
	{
		if ( logicGameObject) 
			logicGameObject.BroadcastMessage ("DeclineDecision");


	}

    /// <summary>
    /// Accepts the decision.
    /// </summary>
    public void AcceptDecision()// 決定
	{

		if (logicGameObject ) 
			logicGameObject.BroadcastMessage ("AcceptDecision");
		

	}


    public void Good() //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        if (logicGameObject)
            logicGameObject.BroadcastMessage("FeedBackGood");

    }

    public void Bad() //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        if (logicGameObject)
            logicGameObject.BroadcastMessage("FeedBackBad");

    }


    /// <summary>
    /// Shows the popup.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public void ShowPopup(object[] parameters)
	{

		string text = (string)(parameters[0]);
		Vector3 position = (Vector3)parameters[1];
		PopUp.gameObject.SetActive (true);
		PopUp.text = text;
		PopUp.gameObject.transform.position = new Vector3(position.x, position.y, PopUp.gameObject.transform.position.z);
		float time = 0.75f;

		Sequence actions = new Sequence(new SequenceParms());
		TweenParms parms = new TweenParms().Prop("color", new Color(1.0f, 1.0f, 1.0f, 1.0f)).Ease(EaseType.EaseOutQuart);
		parms.Prop("fontSize", PopUp.fontSize * 2).Ease(EaseType.EaseOutBounce);

		TweenParms parmsReset = new TweenParms().Prop("color", new Color(1.0f, 1.0f, 1.0f, 0.0f)).Ease(EaseType.EaseOutQuart);
		parmsReset.Prop("fontSize", PopUp.fontSize ).Ease(EaseType.EaseOutQuart);

		actions.Append(HOTween.To(PopUp, time, parms));
		actions.Append(HOTween.To(PopUp, time, parmsReset));

		actions.Play();

	}

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public void HidePopup()
	{
		PopUp.gameObject.SetActive (false);
		PopUp.text = string.Empty;


	}


    /// <summary>
    /// Ends the battle.
    /// </summary>
    public void EndBattle()
	{

		if ( logicGameObject ) 
			logicGameObject.BroadcastMessage ("EndBattle");
		
	}

}
