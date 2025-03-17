using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System.Linq;
using Unity.Linq;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Playables;
using CustomNode.StateManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IGameData
{
    InGamePlayInfo PlayInfo { get; set; }
    IObservable<bool?> OnLoadingComplete { get; }
    Action<float> OnFrameMove { set; get; }
    Define.ECONTENT_TYPE ContentType { get; set; }
    StateManagerGraph GetBaseNode { get; }
}

public interface ICharacterManager
{
    void RefreshStaffs();
}

public interface IStageClearCondition
{
    bool IsStageEndCondition();
}

public interface IGameState
{
    IObservable<Define.EGAME_STATE> OnStateObservable { get; }

    IObservable<bool> MenuVisibleObservable();
    void SetMenuVisible(bool _b);
    bool GetMenuVisible();
    Define.EGAME_STATE GetGameState { get; }
    bool IsShowClearSequence { get; }
    void Result();

}



public class GameSceneInit : BaseScene, IGameData, ICharacterManager, IStageClearCondition, IGameState
{
    public static float playTime = 60f;

    public StartPositionTrigger playerPos;
    public StartPositionTrigger enemyPos;

    private OneReplaySubject<bool?> loadingComplete = new OneReplaySubject<bool?>(null);
    private InGamePlayerInfo localPlayer;
    private InGamePlayerInfo enemy;
    private InGamePlayInfo playInfo;
    [HideInInspector]
    public List<InGamePlayerInfo> gamePlayerInfo = new List<InGamePlayerInfo>();

    InGamePlayInfo IGameData.PlayInfo
    {
        get => playInfo;
        set => playInfo = value;
    }

    #region 에디터 설정
#if UNITY_EDITOR
    private const string SpeedDebugger = "Menu/스피드토글";
    public static bool isSpeedDebugger => EditorPrefs.GetBool(SpeedDebugger);


    [MenuItem(SpeedDebugger)]
    private static void DebuggerToggle()
    {
        var isDebug = !isSpeedDebugger;
        Menu.SetChecked(SpeedDebugger, isDebug);
        EditorPrefs.SetBool(SpeedDebugger, isDebug);
        SceneView.RepaintAll();
    }
    private static Rect windowRect = new Rect(360, 70, 120, 115+ 30);

    private void OnGUI()
    {
        if (!isSpeedDebugger)
        {
            return;
        }

        var preRect = windowRect;
        windowRect = GUI.Window(0, windowRect, DebugWindow, "Debug");
        if (preRect != windowRect)
        {
            EditorPrefs.SetFloat("GameX", windowRect.x);
            EditorPrefs.SetFloat("GameY", windowRect.y);
        }
    }
#endif
    #endregion

    //IGameData
    public IObservable<bool?> OnLoadingComplete => loadingComplete;
    public Action<float> OnFrameMove { get; set; }
    public Define.ECONTENT_TYPE ContentType { get; set; }

    StateManagerGraph basicNode;
    public StateManagerGraph GetBaseNode => basicNode;

    //IGameState
    [NonSerialized]
    public ReactiveProperty<Define.EGAME_STATE> gameState =
        new ReactiveProperty<Define.EGAME_STATE>(Define.EGAME_STATE.LOADING);

    Define.EGAME_STATE IGameState.GetGameState => gameState.Value;
    private BoolReactiveProperty menuVisible = new BoolReactiveProperty(false);
    private Define.EGAME_STATE prevState;


    public IObservable<Define.EGAME_STATE> OnStateObservable => gameState.AsObservable();

    public bool IsShowClearSequence => showEndSequence;

    Action endSequence;
    private bool showEndSequence;
    private bool isGameStart;


    void Start()
    {
        Loading();
    }

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentSceneType = Define.Scene.GameScene;
        ContentType = Define.ECONTENT_TYPE.INGAME;
    }
    private void Update()
    {
        if (Managers.Popup.IsShowPopup() || Managers.Popup.IsWaitPopup() || !loadingComplete.Value.HasValue || !isGameStart)
            return;

        #region EndPopup
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (Managers.Popup.IsShowSystemMenu())
            {
                Managers.Popup.ShowSystenMenu(false);
                return;
            }


            var gameData = Managers.Scene.CurrentScene as IGameData;
            if (gameData != null && gameData.ContentType.UseIngameExitBtn())
            {
                return;
            }

        }
        #endregion

    }



    public virtual async UniTaskVoid Loading()
    {
        gameObject.FixedUpdateAsObservable().Subscribe(_ =>
                {
                    FrameMove(Time.fixedDeltaTime);
                }).AddTo(this);


        IngameLoadingImage.LoadingEvent.OnNext(10);

        await UniTask.WaitForEndOfFrame();


        IngameLoadingImage.LoadingEvent.OnNext(20);
        await UniTask.DelayFrame(30);

        
        await Managers.Pool.CreateDamageParticle();

        


        var obj = new GameObject { name = "InGamePlayInfo" };
        var _playInfo = obj.AddComponent<InGamePlayInfo>();
        _playInfo.SetStageClearCondition(this);
        playInfo = _playInfo;
        IngameLoadingImage.LoadingEvent.OnNext(40);

        var player = new GameObject("Player1");
        localPlayer = player.AddComponent<InGamePlayerInfo>();
        localPlayer.GameData = this;
        localPlayer.playerData.accountID = UserInfo.account1;
        

        var units = UserInfo.Units.Where(_=>_.AccountID == UserInfo.account1);
        var unitList = new List<UnitLogic>();
        foreach (var unit in units)
        {
            var _unitInfo = Managers.Data.GetUnitInfo(unit.UnitID);
            if (_unitInfo == null)
            {
                Debug.LogError("No unitInfo");
                continue;
            }
            var unitInitialData = new UnitBaseData(_unitInfo, unit);
            var unitLogic = localPlayer.GetUnitFromID(unit.UnitID);
            var unitPos = playerPos.transform.position + new Vector3(0, 0, -unit.Index * 0.5f);
            
            if (unitLogic == null)
            {
                unitLogic = await SpawnUnit(localPlayer, unitInitialData, unitPos, 
                    playerPos.isLookAtLeft ? Define.EUNIT_DIRECTION.LEFT : Define.EUNIT_DIRECTION.RIGHT);
                unitLogic.gameObject.SetActive(false);
            }
            else
            {
                unitLogic.SetData(unitInitialData);
            }
            unitList.Add(unitLogic);

        }
        gamePlayerInfo.Add(localPlayer);
        localPlayer.Init(unitList);

        var enemyObj = new GameObject("Enemy");
        enemy = enemyObj.AddComponent<InGamePlayerInfo>();
        enemy.playerData.accountID = UserInfo.account2;
        enemy.GameData = this;
        var enemies = UserInfo.Units.Where(_ => _.AccountID == UserInfo.account2);
        var spawnList = new List<UnitLogic>();
        foreach (var unit in enemies)
        {
            var _unitInfo = Managers.Data.GetUnitInfo(unit.UnitID);
            if (_unitInfo == null)
            {
                Debug.LogError("No unitInfo");
                continue;
            }
            var unitInitialData = new UnitBaseData(_unitInfo, unit);
            var unitLogic = enemy.GetUnitFromID(unit.UnitID);
            var unitPos = enemyPos.transform.position + new Vector3(0, 0, -unit.Index * 0.5f);

            if (unitLogic == null)
            {
                unitLogic = await SpawnUnit(enemy, unitInitialData, unitPos,
                    enemyPos.isLookAtLeft ? Define.EUNIT_DIRECTION.LEFT : Define.EUNIT_DIRECTION.RIGHT);
                unitLogic.gameObject.SetActive(false);
            }
            else
            {
                unitLogic.SetData(unitInitialData);
            }
            spawnList.Add(unitLogic);
        }

        enemy.playerData.playerIndex = 1;
        gamePlayerInfo.Add(enemy);
        enemy.Init(spawnList);

        IngameLoadingImage.LoadingEvent.OnNext(50);


        await UniTask.Delay(100);
        IngameLoadingImage.LoadingEvent.OnNext(60);

        playInfo.Init(localPlayer, enemy);
        playInfo.JoinPlayers(gamePlayerInfo);

        IngameLoadingImage.LoadingEvent.OnNext(70);




        SetEndSequence();
        
        Resources.UnloadUnusedAssets();

        await UniTask.WaitForEndOfFrame();
        SetGameState(Define.EGAME_STATE.LOADING_COMPLETE);
        if (bgm != null)
        {
            Managers.Sound.Play(bgm, Define.Sound.Bgm);
        }

        loadingComplete.OnNext(true);
        Managers.Input.isInteractable = true;
        IngameLoadingImage.LoadingEvent.OnNext(100);
        Managers.Popup.ShowReservationPopup();

        playInfo.SetStageReady();
    }

    private void FrameMove(float _delta)
    {
        if (!loadingComplete.Value.HasValue)
            return;

        if(Managers.Time.IsPause)
        {
            if (gameState.Value != Define.EGAME_STATE.PAUSE)
            {
                prevState = gameState.Value;
                SetGameState(Define.EGAME_STATE.PAUSE);
            }
            return;
        }

        switch (gameState.Value)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                SetGameState(Define.EGAME_STATE.ENTRY);
                break;
            case Define.EGAME_STATE.ENTRY:
                if(playInfo.playState.Value == InGamePlayInfo.EPLAY_STATE.PLAY)
                    SetGameState(Define.EGAME_STATE.ENTRY_COMPLETE);
                break;
            case Define.EGAME_STATE.PLAY:
                OnFrameMove?.Invoke(_delta);
                playInfo?.FrameMove(_delta);

                if (playInfo.IsStageEndCondition())
                {
                    SetGameState(Define.EGAME_STATE.RESULT);
                }
                break;
            case Define.EGAME_STATE.PAUSE:
                break;
            case Define.EGAME_STATE.RESULT:
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                SetGameState(Define.EGAME_STATE.PLAY);
                break;
            default:
                break;
        }
    }

    public void SetGameState(Define.EGAME_STATE _state)
    {
        switch (_state)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                break;
            case Define.EGAME_STATE.ENTRY:
                break;
            case Define.EGAME_STATE.PLAY:
                break;
            case Define.EGAME_STATE.PAUSE:
                SetGameState(prevState);
                break;
            case Define.EGAME_STATE.RESULT:
                playInfo?.EndGame();
                Result();
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                isGameStart = true;
                break;
            default:
                break;
        }

        gameState.Value = _state;
    }

    public override void Clear()
    {
    }

    public void RefreshStaffs()
    {

    }

    private static async UniTask<UnitLogic> SpawnUnit(InGamePlayerInfo _spawnPlayer, UnitBaseData _unitBaseData, Vector3 _pos,
                                                        Define.EUNIT_DIRECTION direction = Define.EUNIT_DIRECTION.RIGHT)
    {
        var _unitInfo = Managers.Data.GetUnitInfo(_unitBaseData.unitID);
#if UNITY_EDITOR
        if (_unitInfo == null)
        {
            Debug.LogError("Spawn Unit Failed " + _unitBaseData.unitID);
        }
#endif

        var clone = Managers.Pool.PopUnit(_unitInfo);
        if (clone == null)
        {
            clone = await Managers.Pool.CreateUnitPool(_unitInfo);
        }

        clone.transform.SetParent(_spawnPlayer.transform);
        clone.transform.position = _pos;
        clone.gameObject.SetActive(true);
        clone.RotateUnit(direction);
        var unitLogic = clone.GetComponent<UnitLogic>();
        unitLogic.Init(_unitBaseData, _spawnPlayer);

        return unitLogic;
    }

    public bool IsStageEndCondition()
    {
        return false;
    }

    public void Result()
    {
        endSequence?.Invoke();
        endSequence = null;
    }

    private void SetEndSequence()
    {
        if (endSequence == null)
        {
            endSequence = () =>
            {
                Managers.Popup.CloseAllPopupBox();
                OpenResultPopup();
#if LOG_ENABLE && UNITY_EDITOR
                string log = "";
                log += "GAME END".ToColor(Color.red);


                Debug.Log(log);
#endif
            };
        }





        async void OpenResultPopup()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            Managers.Time.SetGameSpeed(1f);
            var playerLog = localPlayer.GetLog();
            var enemyLog = enemy.GetLog();
            var resultInfo = new List<InGameUnitResultInfo>();
            foreach (var log in playerLog)
            {
                InGameUnitResultInfo info = new InGameUnitResultInfo
                {
                    partyIndex = log.unitIndex,
                    damage = log.damage > 0 ? log.damage : -log.damage,
                    recieved = log.recieve > 0 ? log.recieve : -log.recieve,
                    heal = log.heal > 0 ? log.heal : -log.heal,
                    isAlive = !log.isDie,
                    AccountID = localPlayer.playerData.accountID,
                    hp = log.unitHp,
                };
                resultInfo.Add(info);
            }
            foreach (var log in enemyLog)
            {
                InGameUnitResultInfo info = new InGameUnitResultInfo
                {
                    partyIndex = log.unitIndex,
                    damage = log.damage > 0 ? log.damage : -log.damage,
                    recieved = log.recieve > 0 ? log.recieve : -log.recieve,
                    heal = log.heal > 0 ? log.heal : -log.heal,
                    isAlive = !log.isDie,
                    AccountID = enemy.playerData.accountID,
                    hp = log.unitHp,
                };
                resultInfo.Add(info);
            }

            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupInGameResult, new PBInGameResult()
            {
                result = GetBattleResult(),
                unitResultList = resultInfo,
            });
        }
    }

    public Define.EBATTLE_RESULT GetBattleResult()
    {
        Define.EBATTLE_RESULT result = new Define.EBATTLE_RESULT();

        var player1Die = localPlayer.IsPlayerDie();
        var player2Die = enemy.IsPlayerDie();
        if (player1Die && player2Die || (!player1Die && !player2Die))
        {
            var hpRate1 = localPlayer.GetPartyHpRate().DecimalRound(Define.DECIMALROUND.Round, 1);
            var hpRate2 = enemy.GetPartyHpRate().DecimalRound(Define.DECIMALROUND.Round, 1);
            if (hpRate1 > hpRate2)
            {
                result = Define.EBATTLE_RESULT.WIN;
            }
            else if(hpRate1 < hpRate2)
            {
                result = Define.EBATTLE_RESULT.LOSE;
            }
            else
            {
                result = Define.EBATTLE_RESULT.DRAW;
            }
        }
        else if(player1Die)
        {
            result = Define.EBATTLE_RESULT.LOSE;
        }
        else if(player2Die)
        {
            result = Define.EBATTLE_RESULT.WIN;
        }

        return result;
    }

    public IObservable<bool> MenuVisibleObservable()
    {
        return menuVisible;
    }

    public void SetMenuVisible(bool _b)
    {
        menuVisible.Value = _b;
    }

    public bool GetMenuVisible()
    {
        return menuVisible.Value;
    }

    void OnApplicationPause(bool isPaused)
    {
        if (gameState.Value != Define.EGAME_STATE.PLAY || gameState.Value == Define.EGAME_STATE.PAUSE)
            return;

        if(!Managers.Time.IsPause && isPaused)
        {
            //Debug.ColorLog($"퍼즈");
        }
    }
}
