using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using CustomNode.State;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Serialization;

public abstract class LogicBase : MonoBehaviour
{
    public abstract void FrameMove(float _delta);
}

public class ProjectileLogic : LogicBase
{
    [NonSerialized]
    public UnitLogic parentUnit;

    [NonSerialized]
    public InGamePlayerInfo owner;

    private float duration;
    private float currentTime;
    private float tick;
    private float currentDelayTime;
    private float delayTime;

    [NonSerialized]
    private float damage;
    private Define.EDAMAGE_TYPE damageType;
    [NonSerialized]
    private int cur = 0;

    private int skillID;
    private Define.EUNIT_STATE unitState;
    private IEnumerable<UnitLogic> enemy;
    private IEnumerable<UnitLogic> allow;
    List<UnitLogic> listTarget = new List<UnitLogic>(10);
    List<UnitLogic> listEffectTarget = new List<UnitLogic>(10);

    public float CurrentDelayTime => currentDelayTime;
    public float DelayTime => delayTime;
    public float DelayRatio => currentDelayTime / delayTime;

    public override void FrameMove(float _delta)
    {
        if (currentDelayTime < delayTime)
        {
            currentDelayTime += _delta;
            return;
        }


        if (duration <= currentTime || parentUnit == null)
        {
            owner.RemoveObject(this);
            return;
        }

        currentTime += _delta;
        var i = (int)(currentTime / tick);
        if (cur > i)
        {
            return;
        }

        cur++;

        enemy = null;
        allow = null;
        float totalDamage = 0;

        listEffectTarget.Clear();
        var targets = GetEnemy();

        totalDamage += DamageToAggroTarget(parentUnit, damageType, damage, ref listEffectTarget);

    }

    private float DamageToAggroTarget(UnitLogic _owner
        , Define.EDAMAGE_TYPE _damageType
        , float _skillDamage
        , ref List<UnitLogic> _listLogic)
    {
        var target = _owner.aggroTarget;
        float ret = 0;
        if (target)
        {
            bool isCritical = false;
            bool retAvoid = false;
            ret += target.TakeDamage(_owner, _damageType, _skillDamage, ref isCritical, ref retAvoid);

            if (!retAvoid)
            {
                _listLogic?.Add(target);
            }
        }

        return ret;
    }

    

    private IEnumerable<UnitLogic> GetAllow()
    {
        if (allow == null)
        {
            allow = listTarget.Where(_2 =>
                _2 != null && !_2.IsDie && parentUnit.CheckSameTeam(_2));
        }

        return allow;
    }

    private IEnumerable<UnitLogic> GetEnemy()
    {
        if (enemy == null)
        {
            enemy = listTarget.Where(_2 =>
                _2 != null && !_2.IsDie && !parentUnit.CheckSameTeam(_2));
        }

        return enemy;
    }

    //private void AddParticle(IEnumerable<UnitLogic> listTarget, GetParticleInfoNode particleInfo)
    //{
    //    if (particleInfo == null)
    //        return;
    //    foreach (var mit in listTarget)
    //    {
    //        switch (particleInfo.particleType)
    //        {
    //            case Define.EPARTICLE_TYPE.TARGET:
    //                if (mit != null)
    //                    UnitLogicExtension.AddParticleTarget(mit,
    //                        parentUnit.IsLeft,
    //                        particleInfo.assetBundle,
    //                        particleInfo.objectName,
    //                        particleInfo.overlayParticle,
    //                        new Vector3(particleInfo.offset.x, particleInfo.offset.y, 0),
    //                        true,
    //                        particleInfo.isDelay ? particleInfo.delay : 0,
    //                        particleInfo.isControlDuration ? particleInfo.duration : float.NaN,
    //                        particleInfo.lookAtCamera, particleInfo.relativeLayer, particleInfo._flipParamType);
    //                break;
    //            case Define.EPARTICLE_TYPE.OBJECT:
    //                if (mit != null)
    //                    UnitLogicExtension.AddParticleObject(mit,
    //                        parentUnit.IsLeft,
    //                        particleInfo.overlayParticle,
    //                        new Vector3(particleInfo.offset.x, particleInfo.offset.y, 0),
    //                        particleInfo.assetBundle,
    //                        particleInfo.objectName,
    //                        particleInfo.isDelay ? particleInfo.delay : 0,
    //                        particleInfo.isControlDuration ? particleInfo.duration : float.NaN, particleInfo.relativeLayer,
    //                        particleInfo._flipParamType);

    //                break;
    //        }
    //    }
    //}

    public void Init(UnitLogic _unit, int _skillID
        , Define.EUNIT_STATE _unitState,
        float _tick, float _delayTime, float _duration, float _damage, Define.EDAMAGE_TYPE _damageType)
    {
        parentUnit = _unit;
        skillID = _skillID;
        unitState = _unitState;
        owner = _unit.GetOwner;
        delayTime = _delayTime;
        duration = _duration;
        tick = _tick;
        damage = _damage;
        damageType = _damageType;

        listTarget.Clear();
        listEffectTarget.Clear();
        currentTime = 0;
        currentDelayTime = 0;
        cur = 0;

    }

    private void OnTriggerEnter(Collider other)
    {
        var unitLogic = other.GetComponent<UnitLogic>();
        if(unitLogic != null)
            listTarget.Add(unitLogic);
    }

    private void OnTriggerExit(Collider other)
    {
        var unitLogic = other.GetComponent<UnitLogic>();
        if (unitLogic != null)
            listTarget.Remove(unitLogic);
    }
}
