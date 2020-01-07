﻿// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : Pondomaniac
// Created          : 07-06-2016
//
// Last Modified By : Pondomaniac
// Last Modified On : 07-20-2016
// ***********************************************************************
// <copyright file="BattleController.cs" company="">

// </copyright>
// <summary></summary>
// ***********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;




/// <summary>
/// Class BattleController.
/// </summary>
public class BattleController : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the state of the character.
    /// </summary>
    /// <value>The state of the character.</value>
    EnumCharacterState CharacterState { get; set; }
    /// <summary>
    /// Gets or sets the character side.
    /// </summary>
    /// <value>The character side.</value>
    EnumSide CharacterSide { get; set; }
    /// <summary>
    /// The possible ennemies
    /// </summary>
    public List<GameObject> PossibleEnnemies = new List<GameObject>();
    /// <summary>
    /// The number of enemy to generate
    /// </summary>
    public int NumberOfEnemyToGenerate = 1;
    /// <summary>
    /// The space between characters
    /// </summary>
    public int SpaceBetweenCharacters = 5;
    /// <summary>
    /// The space between character and enemy
    /// </summary>
    public int SpaceBetweenCharacterAndEnemy = 3;
    /// <summary>
    /// The players x position
    /// </summary>
    public int PlayersXPosition = -17;
    /// <summary>
    /// The ennemy x position
    /// </summary>
    public int EnnemyXPosition = 17;
    /// <summary>
    /// The players y position
    /// </summary>
    public int PlayersYPosition = 16;
    /// <summary>
    /// The ennemy y position
    /// </summary>
    public int EnnemyYPosition = 16;
    /// <summary>
    /// The selector
    /// </summary>
    public GameObject Selector;
    /// <summary>
    /// The target selector
    /// </summary>
    public GameObject TargetSelector;
    /// <summary>
    /// The weapon particle effect
    /// </summary>
    public GameObject WeaponParticleEffect;
    /// <summary>
    /// The magic particle effect
    /// </summary>
    public GameObject MagicParticleEffect;
    /// <summary>
    /// The current state
    /// </summary>
    private EnumBattleState currentState = EnumBattleState.Beginning;
    /// <summary>
    /// The battl action
    /// </summary>
    private EnumBattleAction battlAction = EnumBattleAction.None;
    /// <summary>
    /// The generated enemy list
    /// </summary>
    private List<GameObject> generatedEnemyList = new List<GameObject>();
    /// <summary>
    /// The instantiated character list
    /// </summary>
    private List<GameObject> instantiatedCharacterList = new List<GameObject>();
    /// <summary>
    /// The turn by turn sequence list
    /// </summary>
    private List<Tuple<EnumPlayerOrEnemy, GameObject>> turnByTurnSequenceList = new List<Tuple<EnumPlayerOrEnemy, GameObject>>(); //tuple 2つの要素を一度に返せる
                                                                                                                                  //つまりこの場合：turnByTurnSequenceList=<敵か味方か、実際の見た目>
                                                                                                                                  /// <summary>
                                                                                                                                  /// The sequence enumerator
                                                                                                                                  /// </summary>
    List<Tuple<EnumPlayerOrEnemy, GameObject>>.Enumerator sequenceEnumerator;// List<T>の要素を列挙する

    /// <summary>
    /// The instantiated selector
    /// </summary>
    private GameObject instantiatedSelector;
    /// <summary>
    /// The instantiated target selector
    /// </summary>
    private GameObject instantiatedTargetSelector;
    /// <summary>
    /// The selected enemy
    /// </summary>
    private GameObject selectedEnemy;
    /// <summary>
    /// The selected player
    /// </summary>
    private GameObject selectedPlayer;
    /// <summary>
    /// The selected player datas
    /// </summary>
    private CharactersData selectedPlayerDatas;
    /// <summary>
    /// The UI game object
    /// </summary>
    private GameObject uiGameObject;
    /// <summary>
    /// Awakes this instance.main
    /// </summary>
    /// 

    public enum OperationType
    {
        Attack,
        balance,
        support,
    }
    public OperationType AIType;
    public string AItypeText;
    private int count = 0;//これで行動順が主人公か仲間かを識別している
    public List<int> feedbackCountList = new List<int>();//各ターンのフィードバック回数を記録
    public List<string> FelloActions = new List<string>();//各ターンの仲間の行動を記録
    public List<string> EnemyActions = new List<string>();//各ターンの敵の行動を記録
    public int nowFeedbackCount = 0; //フィードバックの回数を計測
    public int battleTurn = 1;
    public int indexSelectAction = 0;
    public int indexSelectEnemyAction = 0;
    public bool receptionFB = false; //フィードバックを受け付けるかどうか
    public GameObject fadeout;
    public GameObject goodPanel;
    public GameObject gp;

    // プレイヤーの攻撃力と防御力の補正値
    public float playerAttackCorrection = 1;
    public float playerDefenseCorrection = 1;
    public float enemyAttackCorrection = 1;
    public float enemyDefenseCorrection = 1;





    void Awake()
    {
        //OperationType AIType;
        //キャラ、バトルUI等を呼び出す
        CharacterState = EnumCharacterState.Idle;
        CharacterSide = EnumSide.Down;
        instantiatedSelector = GameObject.Instantiate(Selector);
        instantiatedTargetSelector = GameObject.Instantiate(TargetSelector);
        var t = Resources.Load<GameObject>(Settings.PrefabsPath + Settings.UIBattle);
        var o = Instantiate(t);
        var canvas = o.GetComponentsInChildren<Canvas>();
        foreach (Canvas canva in canvas)
        {
            canva.worldCamera = Camera.main;
        }
        //敵を生成
        GenerateEnnemies();
        //プレイヤーの位置を決める
        PositionPlayers();
        //
        GenerateTurnByTurnSequence();//ターンごとのシーケンスを生成
        sequenceEnumerator = turnByTurnSequenceList.GetEnumerator();// GetEnumerator コレクション内全ての要素を一回ずつ呼び出す
        NextBattleSequence();
        uiGameObject = GameObject.FindGameObjectsWithTag(Settings.UI).FirstOrDefault();//FitstOrDefault シーケンスの最初を返す
        HideDecision();

    }

    /// <summary>
    /// This is the main loop and where the system detect the presed keys and send them to the controller.
    /// </summary>
    void Update()
    {
        if (HOTween.GetAllPlayingTweens().Any())
            return;

        if (currentState == EnumBattleState.SelectingTarget)
        {
            //// 狙う敵を選択するフェーズ
            //case EnumBattleState.SelectingTarget:
            //Detecting if the player clicked on the left mouse button and also if there is no animation playing
            // プレーヤーがマウスの左ボタンをクリックしたかどうか、およびアニメーションが再生されていないかどうかを検出する
            //if (Input.GetButtonDown("Fire1"))
            //{
            //    //The 3 following lines is to get the clicked GameObject and getting the RaycastHit2D that will help us know the clicked object
            //    //次の3行は、クリックされたGameObjectを取得し、クリックされたオブジェクトを知るのに役立つRaycastHit2Dを取得することです。
            //    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            //    if (hit.transform != null)
            //    {


            //        bool foundEnemy = false;
            //        foreach (var x in generatedEnemyList)
            //        {
            //            // マウスで選択した敵へターゲットを定める
            //            if (x.GetInstanceID() == hit.transform.gameObject.GetInstanceID())
            //            {
            //                foundEnemy = true;
            //                selectedEnemy = hit.transform.gameObject;
            //                PositionTargetSelector(selectedEnemy);
            //                //　決定を押したときの処理を直接ここに書いてしまう!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //                Log("プレイヤーの攻撃！！");
            //                AcceptDecision();

            //                break;
            //            }
            //        }
            //        if (!foundEnemy)
            //            return;
            //    }
            //}

        }
        else if (currentState == EnumBattleState.EnemyTurn)
        {
            Log(GameTexts.EnemyTurn);
            var z = turnByTurnSequenceList.Where(w => w.First == EnumPlayerOrEnemy.Player);
            // ElementAt 指定したインデックスのデータを返す
            var playerTargetedByEnemy = z.ElementAt(UnityEngine.Random.Range(0, z.Count() - 1));
            // 攻撃するターゲットを決める
            var playerTargetedByEnemyDatas = GetCharacterDatas(playerTargetedByEnemy.Second.name);
            // 矢印をターゲットの向きに変更
            PositionTargetSelector(playerTargetedByEnemy.Second);
            EnemyAttack(playerTargetedByEnemy.Second, playerTargetedByEnemyDatas);
            //PositionTargetSelector(playerTargetedByEnemy.Second);
            //Invoke("NextBattleSequence", 2.0f);
        }
        else if (currentState == EnumBattleState.PlayerTurn)
        {
            Log(GameTexts.PlayerTurn);
            HideTargetSelector();
            // 主人公の行動
            if (count == 0)
            {
                Debug.Log("主人公のターン");
                receptionFB = false;
                //Debug.Log(selectedPlayerDatas);
                count++;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                ShowMenu();//メニューを表示する
                currentState = EnumBattleState.None;
            }

            // 仲間の行動
            else if (count == 1)
            {
                Debug.Log("仲間のターン");
                DrawAction();//仲間の行動パターンを抽選
                //BattlePanels.SelectedSpell = BattlePanels.SelectedCharacter.SpellsList[indexSelectAction];
                //MagicAction();
                AcceptDecision();
                count = 0;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
        }
        else if (currentState == EnumBattleState.PlayerWon)
        {
            Log(GameTexts.PlayerWon);
            HideTargetSelector();
            HideMenu();
            //int totalXP = 0;
            ////経験値の処理
            //foreach (var x in generatedEnemyList)
            //{
            //    totalXP += x.GetComponent<EnemyCharacterDatas>().XP;
            //}

            //foreach (var x in turnByTurnSequenceList)
            //{
            //    var characterdatas = GetCharacterDatas(x.Second.name);
            //    characterdatas.XP += totalXP;
            //    var calculatedXP = Math.Floor(Math.Sqrt(625 + 100 * characterdatas.XP) - 25) / 50;
            //    characterdatas.Level = (int)calculatedXP;
            //}
            var textTodisplay = GameTexts.EndOfTheBattle + "\n\n" + "次の戦闘を行う";
            ShowDropMenu(textTodisplay);
            currentState = EnumBattleState.EndBattle;
            var go = GameObject.FindGameObjectsWithTag(Settings.Music).FirstOrDefault();
            if (go) go.GetComponent<AudioSource>().Stop();
            SoundManager.WinningMusic();

            if (battleTurn > FelloActions.Count) FelloActions.Add("主人公のアクションで終了");
            EnemyActions.Add("死亡");//敵の行動を記録しておく（後で変更!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!）
            feedbackCountList.Add(nowFeedbackCount);
            GetComponent<ExportCSV>().OutputCSV(battleTurn, FelloActions, feedbackCountList, EnemyActions,AItypeText);
        }
        else if (currentState == EnumBattleState.EnemyWon)
        {
            Log(GameTexts.EnemyWon);
            HideTargetSelector();
            HideMenu();

            var textTodisplay = GameTexts.EndOfTheBattle + "\n\n" + GameTexts.YouLost;
            ShowDropMenu(textTodisplay);

            currentState = EnumBattleState.None;
            var go = GameObject.FindGameObjectsWithTag(Settings.Music).FirstOrDefault();
            if (go) go.GetComponent<AudioSource>().Stop();
            SoundManager.GameOverMusic();
        }

    }

    public void PushGoodKey() //Gキーを押すことによってフィードバック
    {
        if (receptionFB)
        {
            nowFeedbackCount++;
            if (gp == null)
            {
                GameObject goodprefab = (GameObject)Instantiate(goodPanel);
                goodprefab.transform.SetParent(fadeout.transform, false);
                SoundManager.StaticPlayOneShot("Good", new Vector3(0, 0, 0));
                gp = goodprefab;
            }
            else
            {
                Destroy(gp);
                GameObject goodprefab = (GameObject)Instantiate(goodPanel);
                goodprefab.transform.SetParent(fadeout.transform, false);
                SoundManager.StaticPlayOneShot("Good", new Vector3(0, 0, 0));
                gp = goodprefab;
            }

            Debug.Log("Good!");
        }
    }

    //そのターンのフィードバック回数を記録 ＆ そのターンのフィードバック数を記録
    public void AggregateFB()
    {
        feedbackCountList.Add(nowFeedbackCount);
        Debug.Log(battleTurn + "ターン目のフィードバックは" + feedbackCountList[battleTurn - 1] + "回");
        nowFeedbackCount = 0;
        battleTurn++;
    }

    public void defaultAI(int index)
    {
        switch (index)
        {
            case 1:
                Debug.Log("攻撃重視");
                AIType = OperationType.Attack;
                AItypeText = "攻撃重視";
                break;

            case 2:
                Debug.Log("バランス重視");
                AIType = OperationType.balance;
                AItypeText = "バランス重視";
                break;

            case 3:
                Debug.Log("サポート重視");
                AIType = OperationType.support;
                AItypeText = "サポート重視";
                break;

            default:
                break;
        }
    }

    //プレイヤーの行動パターンを抽選（ついでに敵の行動パターンも抽選）
    public void DrawAction()
    {
        int value = 0;

        switch (AIType)
        {
            case OperationType.Attack:
                value = UnityEngine.Random.Range(0, 3);//UnityEngine.Random.Range(0, 8);//
                break;

            case OperationType.balance:
                value = UnityEngine.Random.Range(0, 8);//
                break;

            case OperationType.support:
                value = UnityEngine.Random.Range(3, 8);//
                break;

            default:
                break;
        }
        //int value = UnityEngine.Random.Range(0, 8);//
        indexSelectAction = value;
        // 敵の行動パターンを抽選
        indexSelectEnemyAction = UnityEngine.Random.Range(0, 100);
        //if(indexSelectEnemyAction > 89 && )
        //{

        //}
        BattlePanels.SelectedSpell = BattlePanels.SelectedCharacter.SpellsList[indexSelectAction];
        MagicAction();
    }


    /// <summary>
    /// Gets the character datas.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <returns>CharactersData.</returns>
    public CharactersData GetCharacterDatas(string s)
    {

        return Main.CharacterList.Where(w => w.Name == s.Replace(Settings.CloneName, "")).FirstOrDefault();
        // Replace 文字列を置換する。
        // 空白を埋める処理をしてる？

    }


    /// <summary>
    /// Changes the state of the enum character.
    /// </summary>
    /// <param name="state">The state.</param>
    public void ChangeEnumCharacterState(EnumCharacterState state)
    {
        CharacterState = state;
        SendMessage("Animate", string.Format("{0}{1}", CharacterState, CharacterSide));
    }

    /// <summary>
    /// Changes the enum character side.
    /// </summary>
    /// <param name="state">The state.</param>
    public void ChangeEnumCharacterSide(EnumSide state)
    {
        CharacterSide = state;
        SendMessage("Animate", string.Format("{0}{1}", CharacterState, CharacterSide));
    }

    /// <summary>
    /// Flips the specified to the left.
    /// </summary>
    /// <param name="toTheLeft">if set to <c>true</c> [to the left].</param>
    void Flip(bool toTheLeft)
    {
        Vector3 theScale = transform.localScale;
        if (toTheLeft)
            theScale.x = -Mathf.Abs(theScale.x);
        else
            theScale.x = Mathf.Abs(theScale.x);
        transform.localScale = theScale;
    }

    /// <summary>
    /// Generates the ennemies.
    /// </summary>
    void GenerateEnnemies() //敵を生成する
    {
        int y = EnnemyYPosition;
        int calculatedPosition = y;
        for (int i = 0; i <= NumberOfEnemyToGenerate - 1; i++)
        {
            y--;
            calculatedPosition = calculatedPosition - SpaceBetweenCharacters;
            GameObject go = GameObject.Instantiate(PossibleEnnemies[UnityEngine.Random.Range((int)0, (int)PossibleEnnemies.Count)],
                         new Vector3(EnnemyXPosition, calculatedPosition, 1), Quaternion.identity) as GameObject;

            //Debug.Log("sldfk;gsl;gl;:sdklfg:;lskdfg;:lskdf;lgks:;lkfg:;lsdkg;:lsk");
            generatedEnemyList.Add(go);
            Debug.Log("ボスキャラ" + generatedEnemyList[0]);

        }

    }

    /// <summary>
    /// Positions the players.
    /// </summary>
    void PositionPlayers()
    {
        try
        {


            int y = PlayersYPosition;
            int calculatedPosition = y;
            foreach (var character in Main.CharacterList)
            {
                y--;
                calculatedPosition = calculatedPosition - SpaceBetweenCharacters;

                GameObject go = GameObject.Instantiate(Resources.Load(Settings.PrefabsPath + character.Name)
                                        , new Vector3(PlayersXPosition, calculatedPosition, 1), Quaternion.identity) as GameObject;
                var datas = GetCharacterDatas(go.name);

                instantiatedCharacterList.Add(go);

                go.BroadcastMessage("SetHPValue", datas.MaxHP <= 0 ? 0 : datas.HP * 100 / datas.MaxHP);
                go.BroadcastMessage("SetMPValue", datas.MaxMP <= 0 ? 0 : datas.MP * 100 / datas.MaxMP);

            }



        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }

    /// <summary>
    /// Generates the turn by turn sequence.
    /// </summary>
    void GenerateTurnByTurnSequence()//ターンごとのシーケンスを生成!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        var x = 0;
        var y = 0;
        var z = 0;

        var indexInRange = true;
        while (indexInRange)
        {
            if (instantiatedCharacterList.Count - 1 < y && generatedEnemyList.Count - 1 < z)
            {
                indexInRange = false;
                break;
            }

            if (x < 2 && instantiatedCharacterList.Count - 1 >= y)//!!!!!!!!!!!!!!プレイヤー陣営が先制
            {
                turnByTurnSequenceList.Add(new Tuple<EnumPlayerOrEnemy, GameObject>(EnumPlayerOrEnemy.Player, instantiatedCharacterList[y]));
                y++;
            }

            else if (x >= 2 && generatedEnemyList.Count - 1 >= z)//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            {
                turnByTurnSequenceList.Add(new Tuple<EnumPlayerOrEnemy, GameObject>(EnumPlayerOrEnemy.Enemy, generatedEnemyList[z]));
                z++;
            }

            x++;
        }


    }

    /// <summary>
    /// Nexts the battle sequence.
    /// </summary>
    public void NextBattleSequence()//次のバトルシーケンス
    {
        //GetComponent<ExportCSV>().OutputCSV(battleTurn, FelloActions, feedbackCountList, EnemyActions);
        receptionFB = true;
        var x = turnByTurnSequenceList.Where(w => w.First == EnumPlayerOrEnemy.Enemy).Count();
        var y = turnByTurnSequenceList.Where(w => w.First == EnumPlayerOrEnemy.Player).Count();
        if (x <= 0)
        {
            currentState = EnumBattleState.PlayerWon;
            return;
        }
        else if (y <= 0)
        {
            currentState = EnumBattleState.EnemyWon;
            return;
        }


        if (sequenceEnumerator.MoveNext())
        {
            PositionSelector(sequenceEnumerator.Current.Second);
            if (sequenceEnumerator.Current.First == EnumPlayerOrEnemy.Player)
            {

                currentState = EnumBattleState.PlayerTurn;

                selectedPlayer = sequenceEnumerator.Current.Second;
                selectedPlayerDatas = GetCharacterDatas(selectedPlayer.name);
                BattlePanels.SelectedCharacter = selectedPlayerDatas;
                //selectedPlayer = sequenceEnumerator.Current.Second;
                //selectedPlayerDatas = GetCharacterDatas(selectedPlayer.name);
                //PositionSelector(sequenceEnumerator.Current.Second);

            }
            else if (sequenceEnumerator.Current.First == EnumPlayerOrEnemy.Enemy)
            {
                currentState = EnumBattleState.EnemyTurn;
                //PositionSelector(sequenceEnumerator.Current.Second);
            }

        }
        else
        {
            sequenceEnumerator = turnByTurnSequenceList.GetEnumerator();
            NextBattleSequence();

        }
    }



    /// <summary>
    /// Positions the selector.
    /// </summary>
    /// <param name="go">The go.</param>
    public void PositionSelector(GameObject go)
    {
        instantiatedSelector.transform.position = go.transform.position + new Vector3(-4, 0, 0);
    }

    /// <summary>
    /// Weapons the action.
    /// </summary>
    public void WeaponAction()//「こうげき」パネルを選択したら呼び出される

    {
        selectedEnemy = GameObject.FindGameObjectWithTag("Enemy");
        PositionTargetSelector(selectedEnemy);
        currentState = EnumBattleState.PlayerTurn;
        battlAction = EnumBattleAction.Weapon;
        SelectTheFirstEnemy();
        HideMenu();
        AcceptDecision();
        //ShowDecision();

    }


    /// <summary>
    /// Magics the action.
    /// </summary>
    public void MagicAction()
    {
        currentState = EnumBattleState.SelectingTarget;
        battlAction = EnumBattleAction.Magic;
        SelectTheFirstEnemy();
        //HideMenu();
        //ShowDecision();

    }


    /// <summary>
    /// Items the action.
    /// </summary>
    public void ItemAction()
    {
        currentState = EnumBattleState.SelectingTarget;
        battlAction = EnumBattleAction.Item;
        SelectTheFirstEnemy();
        PassAction();

    }

    /// <summary>
    /// Passes the action.
    /// </summary>
    public void PassAction()
    {
        battlAction = EnumBattleAction.Pass;
        selectedPlayer.BroadcastMessage("SetHPValue", selectedPlayerDatas.MaxHP <= 0 ? 0 : selectedPlayerDatas.HP * 100 / selectedPlayerDatas.MaxHP);
        selectedPlayer.BroadcastMessage("SetMPValue", selectedPlayerDatas.MaxMP <= 0 ? 0 : selectedPlayerDatas.MP * 100 / selectedPlayerDatas.MaxMP);
        NextBattleSequence();
        HideMenu();


    }

    /// <summary>
    /// Selects the first enemy.
    /// </summary>
    public void SelectTheFirstEnemy()
    {
        selectedEnemy = generatedEnemyList.Where(w => w.activeSelf).FirstOrDefault();
        if (selectedEnemy != null)
            PositionTargetSelector(selectedEnemy);

    }
    /// <summary>
    /// Positions the target selector.
    /// </summary>
    /// <param name="target">The target.</param>
    // ターゲットを指し示す矢印のポジション指定
    public void PositionTargetSelector(GameObject target)
    {
        instantiatedTargetSelector.SetActive(true);
        instantiatedTargetSelector.transform.position = target.transform.position + new Vector3(-6, -2, 0);
    }

    /// <summary>
    /// Hides the target selector.
    /// </summary>
    // 矢印の表示、非表示の切り替え
    public void HideTargetSelector()
    {

        instantiatedTargetSelector.SetActive(false);
    }

    /// <summary>
    /// Hides the menu.
    /// </summary>
    public void HideMenu()
    {
        if (battlAction != EnumBattleAction.Pass && currentState != EnumBattleState.EnemyWon && currentState != EnumBattleState.PlayerWon)
            DeselectMenusToggles();

        if (uiGameObject)
            uiGameObject.BroadcastMessage("HideActionMenu");

    }

    /// <summary>
    /// Deselects the menus toggles.
    /// </summary>
    public void DeselectMenusToggles()
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("DeselectMenusToggles");
    }


    /// <summary>
    /// Shows the menu.
    /// </summary>
    public void ShowMenu()
    {
        if (uiGameObject)
        {
            uiGameObject.BroadcastMessage("ShowActionMenu");//BroadcastMessage ゲームオブジェクトまたは子オブジェクトにあるすべての 
                                                            //MonoBehaviour を継承したクラスにある methodName 名のメソッドを呼び出します。
                                                            //ShowActionMenuはBattlePanels.csのメソッド
            uiGameObject.BroadcastMessage("Start");
        }


    }

    public void ShowFeedBackMenu()//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        if (uiGameObject)
        {
            uiGameObject.BroadcastMessage("ShowFeedBack"); //ShowActionMenuはBattlePanels.csのメソッド
            uiGameObject.BroadcastMessage("Start");
        }


    }

    /// <summary>
    /// Hides the decision.
    /// </summary>
    public void HideDecision()
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("HideDecision");
    }

    /// <summary>
    /// Shows the decision.
    /// </summary>
    public void ShowDecision()
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("ShowDecision");
    }

    /// <summary>
    /// Declines the decision.
    /// </summary>
    public void DeclineDecision() // キャンセル
    {
        SoundManager.UISound();
        currentState = EnumBattleState.PlayerTurn;
        ShowMenu();
        HideTargetSelector();
        HideDecision();
    }

    /// <summary>
    /// Accepts the decision.
    /// </summary>
    public void AcceptDecision() // 決定
    {
        SoundManager.UISound();
        //currentState = EnumBattleState.SelectedTarget;
        //HideTargetSelector();
        //HideDecision();
        PlayerAction();
    }

    /// <summary>
    /// Ends the battle.
    /// </summary>
    public void EndBattle()
    {
        Main.ControlsBlocked = false;
        SceneManager.LoadScene(Settings.MainMenuScene);
    }

    /// <summary>
    /// Players the action.
    /// </summary>
    public void PlayerAction() // プレイヤーの攻撃のためのメソッド!!!!!!!!!!!!!!!!!
    {
        var enemyCharacterdatas = selectedEnemy.GetComponent<EnemyCharacterDatas>();
        int calculatedDamage = 0;
        if (enemyCharacterdatas != null && selectedPlayerDatas != null)
        {
            switch (battlAction)
            {
                //通常攻撃のとき
                case EnumBattleAction.Weapon:

                    Holoville.HOTween.Sequence actions = new Holoville.HOTween.Sequence(new SequenceParms());
                    TweenParms parms = new TweenParms().Prop("position", selectedEnemy.transform.position - new Vector3(SpaceBetweenCharacterAndEnemy, 0, 0)).Ease(EaseType.EaseOutQuart);
                    TweenParms parmsResetPlayerPosition = new TweenParms().Prop("position", selectedPlayer.transform.position).Ease(EaseType.EaseOutQuart);
                    actions.Append(HOTween.To(selectedPlayer.transform, 1.0f, parms));
                    actions.Append(HOTween.To(selectedPlayer.transform, 1.0f, parmsResetPlayerPosition));
                    actions.Play();

                    //主人公のダメージ計算
                    calculatedDamage = Mathf.CeilToInt((BattlePanels.SelectedWeapon.Attack + selectedPlayerDatas.GetAttack()) * playerAttackCorrection - (enemyCharacterdatas.Defense * enemyDefenseCorrection)); //小数点切り捨て
                    calculatedDamage = Mathf.Clamp(calculatedDamage, 0, calculatedDamage); //与えられた最小 float 値と最大 float 値の範囲に値を制限します　public static float Clamp (float value, float min, float max);
                    enemyCharacterdatas.HP = Mathf.Clamp(enemyCharacterdatas.HP - calculatedDamage, 0, enemyCharacterdatas.HP - calculatedDamage);
                    Log("主人公の攻撃!!");
                    ShowPopup("-" + calculatedDamage.ToString(), selectedEnemy.transform.position);
                    selectedEnemy.BroadcastMessage("SetHPValue", enemyCharacterdatas.MaxHP <= 0 ? 0 : enemyCharacterdatas.HP * 100 / enemyCharacterdatas.MaxHP);
                    Destroy(Instantiate(WeaponParticleEffect, selectedEnemy.transform.localPosition, Quaternion.identity), 1.5f);
                    SoundManager.WeaponSound();
                    selectedPlayer.SendMessage("Animate", EnumBattleState.Attack.ToString());
                    selectedEnemy.SendMessage("Animate", EnumBattleState.Hit.ToString());

                    break;
                // まほうを使うとき
                case EnumBattleAction.Magic:

                    if (indexSelectAction <= 2) //使用された魔法がファイヤ、アイス、サンダー
                    {
                        //ダメージ計算
                        calculatedDamage = BattlePanels.SelectedSpell.Attack + selectedPlayerDatas.GetMagic() - enemyCharacterdatas.MagicDefense;
                        calculatedDamage = Mathf.Clamp(calculatedDamage, 0, calculatedDamage);
                        enemyCharacterdatas.HP = Mathf.Clamp(enemyCharacterdatas.HP - calculatedDamage, 0, enemyCharacterdatas.HP - calculatedDamage);
                        selectedPlayerDatas.MP = Mathf.Clamp(selectedPlayerDatas.MP - BattlePanels.SelectedSpell.ManaAmount, 0, selectedPlayerDatas.MP - BattlePanels.SelectedSpell.ManaAmount);
                        Log("魔導士の" + BattlePanels.SelectedSpell.Name);
                        FelloActions.Add(BattlePanels.SelectedSpell.Name); //仲間の行った特技を記録しておく

                        //ダメージを表示する処理
                        ShowPopup(calculatedDamage.ToString(), selectedEnemy.transform.localPosition);
                        ShowPopup("-" + calculatedDamage.ToString(), selectedEnemy.transform.position);
                        selectedEnemy.BroadcastMessage("SetHPValue", enemyCharacterdatas.MaxHP <= 0 ? 0 : enemyCharacterdatas.HP * 100 / enemyCharacterdatas.MaxHP);
                        selectedPlayer.BroadcastMessage("SetMPValue", selectedPlayerDatas.MaxMP <= 0 ? 0 : selectedPlayerDatas.MP * 100 / selectedPlayerDatas.MaxMP);

                        //エフェクト関連の処理
                        var ennemyEffect = Resources.Load<GameObject>(Settings.PrefabsPath + BattlePanels.SelectedSpell.ParticleEffect);
                        Destroy(Instantiate(ennemyEffect, selectedEnemy.transform.localPosition, Quaternion.identity), 0.5f);
                        var playerEffect = Resources.Load<GameObject>(Settings.PrefabsPath + Settings.MagicAuraEffect);
                        Destroy(Instantiate(playerEffect, selectedPlayer.transform.localPosition, Quaternion.identity), 0.4f);
                        SoundManager.StaticPlayOneShot(BattlePanels.SelectedSpell.SoundEffect, Vector3.zero);

                        //アニメーション関連の処理
                        selectedPlayer.SendMessage("Animate", EnumBattleState.Magic.ToString());
                        selectedEnemy.SendMessage("Animate", EnumBattleState.Hit.ToString());
                    }
                    else if (3 <= indexSelectAction)//使用魔法がバフ、デバフ,回復
                    {
                        switch (indexSelectAction)
                        {
                            // 攻撃アップ
                            case 3:
                                playerAttackCorrection *= 1.5f;
                                break;
                            // 防御アップ
                            case 4:
                                playerDefenseCorrection *= 1.5f;
                                break;
                            // 攻撃デバフ
                            case 5:
                                enemyAttackCorrection *= 0.8f;
                                selectedEnemy.SendMessage("Animate", EnumBattleState.Hit.ToString());
                                break;
                            // 防御デバフ
                            case 6:
                                enemyDefenseCorrection *= 0.8f;
                                selectedEnemy.SendMessage("Animate", EnumBattleState.Hit.ToString());
                                break;
                            // 回復
                            case 7:
                                //instantiatedCharacterList[0].
                                //BattlePanels.SelectedCharacter.HP -= 100;
                                var PlayerDatas = GetCharacterDatas(turnByTurnSequenceList[0].Second.name);

                                if(PlayerDatas.MaxHP < PlayerDatas.HP + 50)
                                {
                                    PlayerDatas.HP = PlayerDatas.MaxHP;
                                }
                                else
                                {
                                    PlayerDatas.HP += 50;
                                }
                                
                                turnByTurnSequenceList[0].Second.BroadcastMessage("SetHPValue", PlayerDatas.MaxHP <= 0 ? 0 : PlayerDatas.HP * 100 / PlayerDatas.MaxHP);
                                turnByTurnSequenceList[0].Second.BroadcastMessage("SetHPValue", PlayerDatas.MaxHP <= 0 ? 0 : PlayerDatas.HP * 100 /PlayerDatas.MaxHP);
                                break;
                            default:
                                break;
                        }

                        Log("魔導士の" + BattlePanels.SelectedSpell.Name);
                        FelloActions.Add(BattlePanels.SelectedSpell.Name); //仲間の行った特技を記録しておく

                        //エフェクト関連の処理
                        var ennemyEffect = Resources.Load<GameObject>(Settings.PrefabsPath + BattlePanels.SelectedSpell.ParticleEffect);
                        Destroy(Instantiate(ennemyEffect, selectedEnemy.transform.localPosition, Quaternion.identity), 0.5f);
                        var playerEffect = Resources.Load<GameObject>(Settings.PrefabsPath + Settings.MagicAuraEffect);
                        Destroy(Instantiate(playerEffect, selectedPlayer.transform.localPosition, Quaternion.identity), 0.4f);
                        SoundManager.StaticPlayOneShot(BattlePanels.SelectedSpell.SoundEffect, Vector3.zero);
                        //アニメーション関連の処理
                        selectedPlayer.SendMessage("Animate", EnumBattleState.Magic.ToString());
                    }

                    break;
                // アイテムを使うとき
                case EnumBattleAction.Item:
                    calculatedDamage = BattlePanels.SelectedItem.Attack - enemyCharacterdatas.MagicDefense;
                    calculatedDamage = Mathf.Clamp(calculatedDamage, 0, calculatedDamage);
                    enemyCharacterdatas.HP = Mathf.Clamp(enemyCharacterdatas.HP - calculatedDamage, 0, enemyCharacterdatas.HP - calculatedDamage);
                    ShowPopup("-" + calculatedDamage.ToString(), selectedEnemy.transform.position);
                    selectedEnemy.BroadcastMessage("SetHPValue", enemyCharacterdatas.HP * 100 / enemyCharacterdatas.MaxHP);
                    Destroy(Instantiate(MagicParticleEffect, selectedEnemy.transform.localPosition, Quaternion.identity), 1.7f);
                    SoundManager.ItemSound();
                    selectedPlayer.SendMessage("Animate", EnumBattleState.Magic.ToString());
                    selectedEnemy.SendMessage("Animate", EnumBattleState.Hit.ToString());

                    break;
                default:
                    break;
            }
        }

        if (enemyCharacterdatas.HP <= 0)
            KillCharacter(selectedEnemy);
        //selectedPlayer.SendMessage ("ChangeEnumCharacterState", battlection);
        selectedEnemy = null;
        //NextBattleSequence();
        Invoke("NextBattleSequence", 2.0f);

        //NextBattleSequence();
    }

    /// <summary>
    /// Enemies the attack.
    /// </summary>
    /// <param name="playerToAttack">The player to attack.</param>
    /// <param name="playerToAttackDatas">The player to attack datas.</param>
    public void EnemyAttack(GameObject playerToAttack, CharactersData playerToAttackDatas)
    {
        var go = sequenceEnumerator.Current.Second;
        Holoville.HOTween.Sequence actions = new Holoville.HOTween.Sequence(new SequenceParms());
        TweenParms parms = new TweenParms().Prop("position", playerToAttack.transform.position + new Vector3(SpaceBetweenCharacterAndEnemy, 0, 0)).Ease(EaseType.EaseOutQuart);
        TweenParms parmsResetPlayerPosition = new TweenParms().Prop("position", go.transform.position).Ease(EaseType.EaseOutQuart);
        //　通常攻撃の時だけ移動を行うように！！
        if(indexSelectEnemyAction < 50)
        {
            actions.Append(HOTween.To(go.transform, 0.5f, parms));
            actions.Append(HOTween.To(go.transform, 0.5f, parmsResetPlayerPosition));
        }
        actions.Play();

        var enemyCharacterdatas = go.GetComponent<EnemyCharacterDatas>();
        int calculatedDamage = 0;
        if (enemyCharacterdatas != null && selectedPlayerDatas != null)
        {
            switch (battlAction)
            {
                case EnumBattleAction.Weapon:
                    //            calculatedDamage = Mathf.CeilToInt((enemyCharacterdatas.Attack * enemyAttackCorrection) - (playerToAttackDatas.Defense * playerDefenseCorrection)); 
                    //calculatedDamage = Mathf.Clamp (calculatedDamage, 0, calculatedDamage);
                    //playerToAttackDatas.HP = Mathf.Clamp (playerToAttackDatas.HP - calculatedDamage , 0 ,playerToAttackDatas.HP - calculatedDamage);
                    //            Log("味方に"+ calculatedDamage.ToString() + "ダメージ");
                    //            //EnemyActions.Add("通常攻撃");//敵の行動を記録しておく（後で変更!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!）
                    //            ShowPopup ("-"+calculatedDamage.ToString (), playerToAttack.transform.position);
                    //	playerToAttack.BroadcastMessage ("SetHPValue",playerToAttackDatas.MaxHP<=0 ?0 : playerToAttackDatas.HP*100/playerToAttackDatas.MaxHP);
                    //Destroy( Instantiate (WeaponParticleEffect, playerToAttack.transform.localPosition, Quaternion.identity),1.5f);
                    //            SoundManager.WeaponSound();
                    //            go.SendMessage("Animate",EnumBattleState.Attack.ToString());
                    //   playerToAttack.SendMessage("Animate",EnumBattleState.Hit.ToString());
                    break;

                default://以下が敵の行動
                    if (0 <= indexSelectEnemyAction && indexSelectEnemyAction < 50)
                    {
                        calculatedDamage = Mathf.CeilToInt((enemyCharacterdatas.Attack * enemyAttackCorrection) - (playerToAttackDatas.Defense * playerDefenseCorrection));
                        calculatedDamage = Mathf.Clamp(calculatedDamage, 0, calculatedDamage);
                        playerToAttackDatas.HP = Mathf.Clamp(playerToAttackDatas.HP - calculatedDamage, 0, playerToAttackDatas.HP - calculatedDamage);
                        //与えたダメージ量をホップアップ
                        Log("味方に" + calculatedDamage.ToString() + "ダメージ");
                        EnemyActions.Add("通常攻撃");//敵の行動を記録しておく（後で変更!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!）
                        ShowPopup("-" + calculatedDamage.ToString(), playerToAttack.transform.position);
                        playerToAttack.BroadcastMessage("SetHPValue", playerToAttackDatas.MaxHP <= 0 ? 0 : playerToAttackDatas.HP * 100 / playerToAttackDatas.MaxHP);
                        Destroy(Instantiate(WeaponParticleEffect, playerToAttack.transform.localPosition, Quaternion.identity), 1.5f);
                        SoundManager.WeaponSound();
                        go.SendMessage("Animate", EnumBattleState.Attack.ToString());
                        playerToAttack.SendMessage("Animate", EnumBattleState.Hit.ToString());
                    }
                    else if (50 <= indexSelectEnemyAction && indexSelectEnemyAction < 70)
                    {
                        enemyAttackCorrection *= 1.2f;
                        Log("敵の攻撃アップ！");
                        EnemyActions.Add("攻撃アップ");
                        
                        ShowPopup("UP!!"  , generatedEnemyList[0].transform.position);
                        go.SendMessage("Animate", EnumBattleState.Idle.ToString());

                    }
                    else if (70 <= indexSelectEnemyAction && indexSelectEnemyAction < 90)
                    {
                        enemyDefenseCorrection *= 1.2f;
                        Log("敵の防御アップ");
                        EnemyActions.Add("防御アップ");
                        
                        ShowPopup("UP!!" , generatedEnemyList[0].transform.position);
                        go.SendMessage("Animate", EnumBattleState.Idle.ToString());

                    }
                    else if (90 < indexSelectEnemyAction)
                    {
                        selectedEnemy = GameObject.FindGameObjectWithTag("Enemy");
                        //var EnemyDatas = GetCharacterDatas(turnByTurnSequenceList[2].Second.name);
                        var EnemyDatas = selectedEnemy.GetComponent<EnemyCharacterDatas>();

                        if (EnemyDatas.MaxHP < EnemyDatas.HP + 50)
                        {
                            EnemyDatas.HP = EnemyDatas.MaxHP;
                        }
                        else
                        {
                            EnemyDatas.HP += 50;
                        }

                        EnemyDatas.BroadcastMessage("SetHPValue", EnemyDatas.MaxHP <= 0 ? 0 : EnemyDatas.HP * 100 / EnemyDatas.MaxHP);
                        EnemyDatas.BroadcastMessage("SetHPValue", EnemyDatas.MaxHP <= 0 ? 0 : EnemyDatas.HP * 100 / EnemyDatas.MaxHP);
                        //generatedEnemyList[0].BroadcastMessage("SetHPValue", enemyCharacterdatas.MaxHP <= 0 ? 0 : enemyCharacterdatas.HP * 100 / enemyCharacterdatas.MaxHP);
                        Log("敵の回復");
                        EnemyActions.Add("回復");

                        ShowPopup("+100", generatedEnemyList[0].transform.position);
                        go.SendMessage("Animate", EnumBattleState.Idle.ToString());

                    }

                    break;

            }
        }
        if (playerToAttackDatas.HP <= 0)
            KillCharacter(playerToAttack);
        //selectedPlayer.SendMessage ("ChangeEnumCharacterState", battlection);
        selectedEnemy = null;
        selectedPlayerDatas = null;
        //NextBattleSequence();
        Invoke("NextBattleSequence", 1.5f);
        Invoke("AggregateFB", 1.5f);

    }

    /// <summary>
    /// Kills the character.
    /// </summary>
    /// <param name="go">The go.</param>
    public void KillCharacter(GameObject go)
    {
        float time = 0.75f;
        Holoville.HOTween.Sequence actions = new Holoville.HOTween.Sequence(new SequenceParms());
        TweenParms parms = new TweenParms().Prop("color", new Color(1.0f, 1.0f, 1.0f, 0.0f)).Ease(EaseType.EaseOutQuart);
        actions.Append(HOTween.To(go.GetComponent<SpriteRenderer>(), time, parms));
        actions.Play();
        go.SetActive(false);
        turnByTurnSequenceList.RemoveAll(r => r.Second.GetInstanceID() == go.GetInstanceID());
        var id = sequenceEnumerator.Current.Second.GetInstanceID();
        sequenceEnumerator = turnByTurnSequenceList.GetEnumerator();
        sequenceEnumerator.MoveNext();
        while (sequenceEnumerator.Current.Second.GetInstanceID() != id)
            sequenceEnumerator.MoveNext();




    }

    /// <summary>
    /// Logs the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    public void Log(string text)
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("LogText", text);
    }

    /// <summary>
    /// Shows the popup.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position.</param>
    public void ShowPopup(string text, Vector3 position)
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("ShowPopup", new object[] { text, position });
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public void HidePopup()
    {
        if (uiGameObject)
            uiGameObject.BroadcastMessage("HidePopup");
    }


    /// <summary>
    /// Shows the drop menu.
    /// </summary>
    /// <param name="text">The text.</param>
    public void ShowDropMenu(string text)
    {
        if (uiGameObject)
        {
            uiGameObject.BroadcastMessage("ShowDropMenu", text);
        }

    }


}