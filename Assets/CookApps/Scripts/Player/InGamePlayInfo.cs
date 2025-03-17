using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

public struct IngamePlayData
{
    public float currentTime;
    public float limitTime;
}

public class InGamePlayInfo : MonoBehaviour
{
    public enum EPLAY_STATE
    {
        SORT,
        READY,
        PLAY,
        PAUSE,
        STOP,
        END,
    }
    public ReactiveProperty<EPLAY_STATE> playState = new ReactiveProperty<EPLAY_STATE>(EPLAY_STATE.SORT);

    protected IGameData gameData;
    private GameObject particleParentObj;
    public GameObject GetParticleParentObject()
    {
        if (particleParentObj == null)
        {
            particleParentObj = new GameObject("Particle");
            particleParentObj.transform.SetParent(transform);
        }

        return particleParentObj;
    }

    private IStageClearCondition condition;
    [HideInInspector]
    public List<InGamePlayerInfo> listPlayer = new List<InGamePlayerInfo>();

    public bool isLastEvent { get; private set; } = false;

    public IngamePlayData playData;
    private bool isStop;
    

    public static Subject<(Define.ETRIGGER_TYPE type, int index, UnitLogic unit, Define.EVENT_TYPE eventType)> OnUnitSelect = new Subject<(Define.ETRIGGER_TYPE, int, UnitLogic unit, Define.EVENT_TYPE eventType)>();
    public static Subject<(Define.ETRIGGER_TYPE type, int index, UnitLogic unit, Define.EVENT_TYPE eventType)> OnUnitDeSelect = new Subject<(Define.ETRIGGER_TYPE, int, UnitLogic unit, Define.EVENT_TYPE eventType)>();
    public static Subject<(Define.EVENT_TYPE eventType, int index)> OnExitEvent = new Subject<(Define.EVENT_TYPE eventType, int index)>();
    public static Subject<Vector3> OnMoveCam = new Subject<Vector3>();
    public static Subject<int> OnHpChange = new Subject<int>();

    public ConcurrentBag<DamageParticleData> damageParticleInfo = new ConcurrentBag<DamageParticleData>();


    private void Start()
    {
        gameData = Managers.Scene.CurrentScene as IGameData;

        gameData.OnLoadingComplete.Subscribe(_ =>
        {

        }).AddTo(this);

        OnUnitSelect.Subscribe(_ =>
        {
            //if (_.type == Define.ETRIGGER_TYPE.WaitingRoom)
            //{
            //    var room = roomInfo.GetRoomInfo(_.index);
            //    _.unit.SetDestination(_.type, _.index);
            //    _.unit.SetTarget(room.transform);
            //    _.unit.SetState(Define.EUNIT_STATE.Move);
            //}
            //else if (_.type == Define.ETRIGGER_TYPE.EventSpot)
            //{
            //    var controller = events.Find(_1 => _1.eventType == _.eventType);
            //    var _event = controller.GetEventSpot(_.index);
            //    _.unit.SetDestination(_.type, _.index);
            //    _.unit.SetTarget(_event.Item1.transform);
            //    _.unit.SetState(Define.EUNIT_STATE.Move);
            //    _.unit.curEventType = _.eventType;
            //}
        }).AddTo(this);

        OnUnitDeSelect.Subscribe(_ =>
        {
            //if (_.type == Define.ETRIGGER_TYPE.WaitingRoom)
            //{
            //    _.unit.SetState(Define.EUNIT_STATE.Idle);
            //}
            //else if (_.type == Define.ETRIGGER_TYPE.EventSpot)
            //{
            //    _.unit.SetState(Define.EUNIT_STATE.Idle);
            //}
        }).AddTo(this);

        OnExitEvent.Subscribe(_ =>
        {
            //foreach (var player in listPlayer)
            //{

            //    foreach (var unit in player.listUnit)
            //    {
            //        if (unit.curEventType == _.eventType && unit.destination.index == _.index)
            //        {
            //            unit.Clear();
            //        }
            //    }

            //}
        }).AddTo(this);

        OnMoveCam.Subscribe(pos =>
        {
            //if (cam.Camera == null)
            //    return;

            //var position = pos;
            //position.y = cam.Camera.transform.position.y;
            //if (cam.Camera.draglockZ)
            //    position.z = cam.Camera.transform.position.z;
            //cam.Camera.Move(position);
        }).AddTo(this);

        OnHpChange.Subscribe(calcHp =>
        {
            //playData.life += calcHp;
        }).AddTo(this);
    }

    private void Update()
    {
        SpawnDamageParticle();
    }
    public void Init(InGamePlayerInfo player, InGamePlayerInfo enemy)
    {
        foreach (var mit in player.listUnit)
        {
            mit.OnDieEvent += (damage) => 
            {
                if(IsAnyPlayerDie())
                {
                    Managers.Time.SetGameSpeed(0.2f);
                }
            };
        }

        foreach (var mit in enemy.listUnit)
        {
            mit.OnDieEvent += (damage) => 
            {
                if (IsAnyPlayerDie())
                {
                    Managers.Time.SetGameSpeed(0.2f);
                }
            };
        }

        playData.limitTime = GameSceneInit.playTime;
        Clear();
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if (playState.Value != EPLAY_STATE.END)
        {
            if (IsEndCondition() || (condition != null && condition.IsStageEndCondition()))
            {
                playState.Value = EPLAY_STATE.END;
                //Managers.Time.SetGameSpeed(1);
                
                return;
            }
        }
        switch (playState.Value)
        {
            case EPLAY_STATE.SORT:
                return;
            case EPLAY_STATE.READY:
                break;
            case EPLAY_STATE.PLAY:
                playData.currentTime = Math.Min(playData.currentTime + _deltaTime, playData.limitTime);
               
                break;
            case EPLAY_STATE.PAUSE:
            case EPLAY_STATE.STOP:
                break;
            case EPLAY_STATE.END:
                return;
            default:
                break;
        }

        foreach (var mit in listPlayer)
        {
            mit.FrameMove(_deltaTime);
        }
    }

    public void EndGame()
    {
        foreach (var player in listPlayer)
        {
            foreach (var unit in player.listUnit)
            {
                unit.Clear();
            }
        }
    }
    public void Clear()
    {
    }

    internal bool IsStageEndCondition()
    {
        return playState.Value == EPLAY_STATE.END;
    }

    public void SetStageClearCondition(IStageClearCondition _condition)
    {
        condition = _condition;
    }

    public void JoinPlayers(IEnumerable<InGamePlayerInfo> players)
    {
        foreach (var mit in players)
        {
            mit.transform.SetParent(gameObject.transform);
            mit.Entry();
            listPlayer.Add(mit);
        }
    }

    public async UniTask SetStageReady()
    {
        await UniTask.WaitUntil(() => IngameLoadingImage.instance != null ? IngameLoadingImage.instance.isloadingComplete : true);
        playState.Value = EPLAY_STATE.READY;
        bool isReady = false;
        Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupCountDown, new PBCountDown
        {
            limitTime = 3f,
            onClose = () =>
            {
                Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
                {
                    strDesc = "start",
                    pushTime = 1f,
                    isTextAni = true,

                }, false);
                isReady = true;
            },
        }, false);
        await UniTask.WaitUntil(() => isReady);
        StageStart();
    }
    public void StageStart()
    {
        playState.Value = EPLAY_STATE.PLAY;


        Debug.ColorLog("게임 시작", Color.green);
    }

    protected virtual bool IsEndCondition()
    {
#if UNITY_EDITOR
        if (Managers.isinfinityMode) return false;
#endif
        return IsAnyPlayerDie() || playData.currentTime >= playData.limitTime || isStop;
    }

    internal bool IsAnyPlayerDie()
    {
        foreach (var player in listPlayer)
        {
            if(player.IsPlayerDie())
                return true;
        }
        return false;
    }

    public void AddDamageParticle(UnitLogic _unitLogic, Define.EDAMAGE_TYPE _type, float _calcHp, bool _isCritical, bool _isAvoid, Vector3 position, bool isMoveRight, long accountID)
    {
        damageParticleInfo.Add(new DamageParticleData
        {
            type = _type,
            calcHp = _calcHp,
            isCritical = _isCritical,
            isAvoid = _isAvoid,
            position = position,
            isMoveRight = isMoveRight,
            accountID = accountID,
        });
    }

    private void SpawnDamageParticle()
    {
        while (!damageParticleInfo.IsEmpty)
        {
            if (damageParticleInfo.TryTake(out var info))
            {
                var clone = Managers.Pool.PopDamageParticle();
                if (clone == null)
                    return;
                clone.transform.SetParent(GetParticleParentObject().transform);
                if (info.isAvoid)
                {
                    clone.transform.position = new Vector3(info.position.x, info.position.y + 0.1f, info.position.z);
                }
                else if (false == info.isCritical)
                {
                    //            clone.transform.position = new Vector3(position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                    //              position.y + UnityEngine.Random.Range(-0.8f, 0), position.z);

                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                        info.position.y + UnityEngine.Random.Range(-0.5f, -0.2f), info.position.z);
                }
                else
                {
                    //            clone.transform.position = new Vector3(position.x, position.y + 0.4f, position.z);
                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.15f, 0.15f),
                        info.position.y + UnityEngine.Random.Range(-0.15f, 0.15f), info.position.z);
                }

                var particle = clone.GetComponent<DamageParticle>();

                particle.Init(info.type, info.calcHp, info.isCritical, info.isAvoid);
                //Debug.ColorLog($"Take Damage Particle spawned {info.type} {info.calcHp} / {info.accountID}");
            }
        }
    }
}
