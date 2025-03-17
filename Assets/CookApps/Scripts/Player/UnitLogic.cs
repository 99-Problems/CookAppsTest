using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Data;
using UnityEngine.AI;
using TMPro;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data.Managers;
using static Data.Define;
using System.Linq;
using CustomNode.StateManager;

public struct UnitBaseData
{
    public UnitInfoScript info;
    public int unitID;
    public UnitData data;

    public UnitBaseData(UnitInfoScript _unitInfo, UnitData _data)
    {
        info = _unitInfo;
        unitID = _unitInfo.unitID;
        data = _data;
    }
}


public class UnitLogic : MonoBehaviour
{
    public float hp = 1; // 유닛 현재 HP 수치

    [LabelText("데미지 표시 위치 보정")]
    public Vector3 textPosition;

    [ReadOnly]
    [HideInInspector]
    public float atkFrame = 0;
    [SerializeField]
    internal UnitLogicStat stat;

    public int curIndex;
    public int partyIndex => unitBaseData.data.Index;

    public ReactiveProperty<EUNIT_STATE> state = new ReactiveProperty<EUNIT_STATE>(EUNIT_STATE.IDLE);
    public IObservable<EUNIT_STATE> unitState => state.AsObservable();
    private protected UnitBaseData unitBaseData;
    private protected InGamePlayerInfo owner;
    public InGamePlayerInfo GetOwner => owner;
    private protected IUnitLogicMovement movement;
    private protected InGamePlayInfo playInfo;

    public UnitStatSO statSo = new UnitStatSO();

    [HideInInspector]
    [NonSerialized]
    public NavMeshPath path;
    private NavMeshAgent navMesh;
    public CapsuleCollider sphere { get; private set; }

    public UnitLogic aggroTarget;
    public LayerMask targetLayerMask;
    [NonSerialized]
    public StateManagerGraph nodeGraph;

    private Transform target;
    [NonSerialized]
    public Vector3 targetPosition = Vector3.positiveInfinity;
    protected bool initialized;
    public bool isMove;
    public float minRotateDistance = 2f;
    public ObstacleAvoidanceType avoidType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    public float avoidRadius = 0.6f;

    private float rotateSpeed = 240f;
    public int UnitID => unitBaseData.unitID;

    public bool IsDie => hp <= 0 || state.Value == Define.EUNIT_STATE.DIE
                                 || state.Value == Define.EUNIT_STATE.DESTROY;

    public long GetOriginalFullHp => stat.life;
    public float GetAtkSpeedPer => stat.atk_speed;

    public Action<float> OnDamageEvent;
    public Action<float> OnDieEvent;
    private int fontTime;
    private int fontInitTime;
    public int fontTimeMax = 30;
    private float dieTime;
    public float dieDelay = 1f;

    public Animator anim { get; private set; }

    [NonSerialized]
    public double damageLog;

    [NonSerialized]
    public float recieveLog;

    [NonSerialized]
    public float healLog;



    private void Awake()
    {
            
        sphere = gameObject.GetComponent<CapsuleCollider>();
        if(sphere == null)
        {
            sphere = gameObject.AddComponent<CapsuleCollider>();
            sphere.radius = 0.6f;
            sphere.height = 1.5f;
            sphere.center = new Vector3(0, 0.5f, -0.1f);
        }
        sphere.material = new PhysicMaterial
        {
            dynamicFriction = 0,
            staticFriction = 0,
        };

        var rigidBody = gameObject.GetOrAddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
        sphere.enabled = false;
        anim = GetComponent<Animator>();
    }

    public virtual void Init(UnitBaseData _unitBaseData, InGamePlayerInfo _owner)
    {
        stat = new UnitLogicStat();
        path = new NavMeshPath();

        sphere.enabled = true;
        initialized = true;
        unitBaseData = _unitBaseData;
        owner = _owner;
        playInfo = this.owner.GameData.PlayInfo;
        if (nodeGraph)
        {
            Destroy(nodeGraph);
        }

        nodeGraph = statSo.stateManager.Copy() as StateManagerGraph;
        nodeGraph.Init(this);
        nodeGraph.SetFindMissingStateFillUp(nodeGraph, this);

        movement = new UnitLogicMovement();
        movement.Init(this);

        SetOnNavMesh();
        SetState(EUNIT_STATE.IDLE);
    }

    public virtual void Reset()
    {
        stat.Set(unitBaseData.info, unitBaseData.data);
        hp = GetOriginalFullHp;
        state.Value = Define.EUNIT_STATE.IDLE;
        target = null;
        dieTime = 0;
    }

    public void Clear()
    {
        SetState(EUNIT_STATE.IDLE);
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if (!initialized)
            return;

        if (fontTime > 0 && fontInitTime >= fontTimeMax)
        {
            fontTime = 0;
            fontInitTime = 0;
        }
        if (fontTime > 0)
            fontInitTime++;

        navMesh.isStopped = true;
        if (Managers.Time.GetGameSpeed() <= 0f)
        {
            return;
        }

       

        //if(aggroTarget == null || aggroTarget.IsDie)
        //SearchTarget(stat.unitType == EUnitType.Support);
        var speed = GetAtkSpeedPer;
       

        switch (state.Value)
        {   
            case Define.EUNIT_STATE.IDLE:
                //SearchTarget(stat.unitType == EUnitType.Support);
                break;
            case Define.EUNIT_STATE.RUN:
                UnitMove(aggroTarget);
                break;
            case Define.EUNIT_STATE.ATTACK:
                break;
            case Define.EUNIT_STATE.DIE:
                DeadTick(_deltaTime);
                return;
            case Define.EUNIT_STATE.DESTROY:
                return;
            default:
                break;
        }

        if (IsDie)
        {
            SetState(Define.EUNIT_STATE.DIE);
            return;
        }

        movement.FrameMove(_deltaTime);
    }
    public void DeadTick(float _deltaTime)
    {
        dieTime += _deltaTime;
        if(dieTime >= dieDelay)
        {
            SetState(Define.EUNIT_STATE.DESTROY);
            gameObject.SetActive(false);
        }
    }
    public void SearchTarget(Define.ESEARCH_TARGET_TYPE type)
    {
        if (aggroTarget != null && !aggroTarget.IsDie)
            return;

        var hits = Physics.OverlapSphere(transform.position, statSo.detectRange, targetLayerMask);
        if(hits.Length == 0)
        {
            aggroTarget = null; // 타겟 없음
            return;
        }
        UnitLogic targetUnit = null;
        switch (type)
        {
            case ESEARCH_TARGET_TYPE.NeareastEnemy:
                var minDistance = float.MaxValue;
                foreach (var hit in hits)
                {
                    target = hit.transform;
                    var aggro = target.GetComponent<UnitLogic>();
                    if (aggro && aggro.owner != this.owner && aggro.IsDie == false)
                    {
                        var distance = Vector3.Distance(transform.position, hit.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            targetUnit = aggro;
                        }
                    }
                }
                aggroTarget = targetUnit;
                return;
            case ESEARCH_TARGET_TYPE.LowestHPTeam:
                double minHP = 0d;
                foreach (var hit in hits)
                {
                    target = hit.transform;
                    var aggro = target.GetComponent<UnitLogic>();
                    if (aggro && aggro.owner == this.owner && aggro.IsDie == false)
                    {
                        var hpLoss = aggro.GetOriginalFullHp - aggro.hp;
                        if (hp >= minHP)
                        {
                            minHP = hpLoss;
                            targetUnit = aggro;
                        }
                    }
                }
                aggroTarget = targetUnit;
                break;
            
        }
        
        List<UnitLogic> aliveList = null;
        //if (IsDie)
        //{
        //    aliveList = UnitLogicExtension.GetCheckDieStateUnits(unitLogicInfo.Owner.GameData.BattleInfo.listPlayer);
        //}
        //else
        //{
        //    aliveList = UnitLogicExtension.GetCheckAliveStateUnits(unitLogicInfo.Owner.GameData.BattleInfo.listPlayer);
        //}
    }


    public void UnitMove(Transform _moveTarget)
    {
        if (_moveTarget == null)
            return;
        navMesh.isStopped = false;
        navMesh.SetDestination(_moveTarget.position);
        //Debug.Log($"{owner.name}'s {UnitID} move to {_moveTarget.name}", Color.cyan);
        return;
    }

    public bool UnitMove(UnitLogic _moveTarget)
    {
        if (_moveTarget == null)
            return false;

        


        var offset = (_moveTarget.transform.position - transform.position).magnitude;
        if (Mathf.Abs(offset) < statSo.attackRange)
        {
            return false;
        }
        else
        {
            UnitMove(_moveTarget.transform);
            return true;
            //normal = (_moveTarget.transform.position - transform.position).normalized;
        }
        
        //AddUnitPosition(normal * Time.deltaTime * statSo.speed);
        //return true;
    }

    public void AddUnitPosition(Vector2 _distance)
    {
        AddUnitPosition(new Vector3(_distance.x, 0, _distance.y));
    }

    public void AddUnitPosition(Vector3 _pos)
    {
        if (IsDie)
            return;

        navMesh.Move(_pos);
    }

    [Button]
    public void RotateUnit(Define.EUNIT_DIRECTION _dir)
    {
        var direction = _dir.GetDirecton();
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void RotateUnit()
    {
        if (aggroTarget == null)
            return;

        var direction = aggroTarget.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    public void SetState(Define.EUNIT_STATE _state)
    {
        if(navMesh && navMesh.isOnNavMesh)
            navMesh.isStopped = true;

        //Debug.Log($"{owner.name}'s {UnitID} change state {_state}", Color.green);
        state.Value = _state;
        anim.SetBool("Idle", _state == EUNIT_STATE.IDLE || _state == EUNIT_STATE.DIE);
        anim.SetBool("Run", _state == EUNIT_STATE.RUN);
        anim.SetBool("Attack", _state == EUNIT_STATE.ATTACK);
        anim.SetBool("Die", _state == EUNIT_STATE.DIE || _state == EUNIT_STATE.DESTROY);
        anim.speed = _state == EUNIT_STATE.ATTACK ? GetAtkSpeedPer : 1;
        nodeGraph?.ChangeState(state.Value);
    }
  

    public async UniTaskVoid SetOnNavMesh()
    {
        NavMeshHit hit;
        var range = 100.0f;
        var isPlace = NavMesh.SamplePosition(transform.position, out hit, range, NavMesh.AllAreas);
        if (isPlace)
        {
            transform.position = hit.position;
            //Debug.ColorLog($"네브메쉬에 없어서 이동 \npos : {hit.position}");
        }
        else
        {
            Debug.ColorLog($"네브메쉬 이동 실패", Color.red);
        }
        if(navMesh == null)
        {
            navMesh = gameObject.GetOrAddComponent<NavMeshAgent>();
            navMesh.enabled = true;
            navMesh.baseOffset = 0.15f;
            navMesh.obstacleAvoidanceType = avoidType;
            navMesh.angularSpeed = rotateSpeed;
            navMesh.speed = statSo.speed;
            navMesh.acceleration = 1000;
            navMesh.stoppingDistance = 0.1f;
            navMesh.autoBraking = false;
            navMesh.updateRotation = false;
            navMesh.destination = transform.position;
            navMesh.isStopped = true;
            navMesh.radius = avoidRadius;
            navMesh.height = 2f;
            navMesh.updateRotation = true;
            //Debug.ColorLog($"네브메쉬 세팅후 SetDestination", Color.green);
        }
    }

    public float GetUseAbleSkillType()
    {
        return 0;
    }
    
    public void SetData(UnitBaseData _unitInitialData)
    {
        unitBaseData = _unitInitialData;
    }

#if LOG_ENABLE && UNITY_EDITOR
    internal string log;
#endif
    public float CalcDamage(Define.EDAMAGE_TYPE edamageType, UnitLogic _attacker, float _skillDamage,
        ref bool _isCritical, ref bool _retAvoid)
    {
        _retAvoid = false;
        bool isHeal = edamageType == EDAMAGE_TYPE.MAGICAL;
        float random = 0f;
#if LOG_ENABLE && UNITY_EDITOR
        log = "";
        if(_attacker != null)
        {
            log += $"{_attacker.owner.name}: {_attacker.name} -> {owner.name}: {this.name} ";
            
            if(isHeal)
            {
                log += $"힐 ".ToColor(Color.green);
            }
            log += $"기본 수치: {_skillDamage}{Environment.NewLine}";
            log += $"계산 전 체력: {hp}{Environment.NewLine}";
        }
#endif
        if (isHeal == false)
        {
            random = owner.GetRandom(0, 100);
#if LOG_ENABLE && UNITY_EDITOR
            log += $"회피 계산 {random} < {stat.evasion} {random < stat.evasion}";
#endif
            if (random < stat.evasion)
            {
                _retAvoid = true;
                _isCritical = false;
#if LOG_ENABLE && UNITY_EDITOR
                log += $"{Environment.NewLine}회피 성공";
                Debug.Log(log);
#endif
                return 0;
            }
        }

        float damage = 0;
        damage = _skillDamage;

        if (isHeal)
        {
#if LOG_ENABLE && UNITY_EDITOR
            Debug.Log(log);
#endif
            return damage;
        }

        random = owner.GetRandom(0, 100);
        if(random < stat.critical)
        {
#if LOG_ENABLE && UNITY_EDITOR
            log += $"크리티컬".ToColor();
#endif
            var criticalDamage = statSo.critDmgRate;
            damage = damage * criticalDamage;
            _isCritical = true;
        }

        var damageReduce = 0f;
        damageReduce += (float)stat.def;
        damage *= 1 - (damageReduce/ 100);
#if LOG_ENABLE && UNITY_EDITOR
        log += $"{Environment.NewLine} 최종 데미지 : "+ $"{damage}".ToColor(Color.green);

        Debug.Log(log);
#endif
        return damage;
    }
    public virtual float TakeDamage(UnitLogic _attacker, Define.EDAMAGE_TYPE _damageType,
        float _skillDamage, ref bool _isCritical,
        ref bool _retAvoid)
    {
        float damage = 0;
        float calc = 0;
        damage = CalcDamage(_damageType, _attacker, _skillDamage, ref _isCritical, ref _retAvoid);
        
        {
            if (!IsDie)
            {
                switch (_damageType)
                {
                    case EDAMAGE_TYPE.PHYSICAL:
                        calc = damage < 0 ? 0 : AddHp(_attacker, -damage);
                        break;
                    case EDAMAGE_TYPE.MAGICAL:
                        calc = damage < 0 ? 0 : AddHp(_attacker, damage);
                        break;
                    default:
                        break;
                }
                
            }
        }
       
        //Debug.ColorLog($"{_attacker.owner}'s {_attacker.name} damaged {this.name} ({calc})");
        playInfo.AddDamageParticle(this,
            _damageType,
            damage.DecimalRound(Define.DECIMALROUND.Round, 1),
            _isCritical,
            _retAvoid,
            transform.position + new Vector3(0, statSo.hpBarPosY + (_isCritical ? fontTime * 0.15f : 0), statSo.hpBarPosZ) + textPosition,
            _attacker.transform.position.x < transform.position.x,
            owner.playerData.accountID);

        if (_isCritical)
        {
            if (fontTime == 0)
                fontInitTime = 0;
            fontTime++;
        }
        //Debug.ColorLog($"Take Damage {_attacker.owner.name}'s {_attacker.name} -> {this.name} {calc}");

        return calc;
    }
    public virtual float AddHp(UnitLogic _effector, float _calcHp)
    {
        if (_calcHp < 0)
        {
            OnDamageEvent?.Invoke(_calcHp);
            if (_effector)
            {
                _effector.AddDamageLog(_calcHp);
            }
            else
            {
                Debug.LogWarning($"유실된 데미지 {_calcHp}");
            }

            AddRecieveLog(_calcHp);
        }
        else
        {
            if (_effector)
            {
                _effector.AddHealLog(_calcHp);
            }
            else
            {
                Debug.LogWarning($"유실된 힐량 {_calcHp}");
            }
        }

        hp += _calcHp;
        if (hp <= 0)
        {
            _calcHp -= hp;
            hp = 0;
            OnDieEvent?.Invoke(_calcHp);
        }
        if(hp > GetOriginalFullHp)
        {
            _effector.AddHealLog(GetOriginalFullHp - hp);
            hp = GetOriginalFullHp;
        }

        return _calcHp;
    }
    private void AddDamageLog(float _log)
    {
        damageLog += _log;
    }

    private void AddHealLog(float _log)
    {
        healLog += _log;
    }

    private void AddRecieveLog(float _log)
    {
        recieveLog += _log;
    }
    public bool CheckSameTeam(UnitLogic _player) => this.owner == _player.owner;

    //private void OnTriggerEnter(Collider other)
    //{
    //    var trigger = other.GetComponent<BaseTrigger>();
    //    if (trigger != null)
    //    {
    //        trigger.Enter(this);
    //    }
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    var trigger = other.GetComponent<BaseTrigger>();
    //    if (trigger && state.Value == EUNIT_STATE.Move)
    //    {
    //        if (IsTarget(trigger.triggerType, trigger.GetIndex))
    //        {
    //            trigger.Enter(this);
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{

    //}
}
