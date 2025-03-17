using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;

public static class UnitLogicExtension
{
    public static Dictionary<Define.EStatType, long> GetUnitAllStatInfo(int unitID)
    {
        Dictionary<Define.EStatType, long> statValue = new Dictionary<Define.EStatType, long>();
        var statList = Managers.Data.GetUnitStatData(unitID);
        for (int i = 0; i < statList.Count; i++)
        {
            statValue[statList[i].stat] = statList[i].val;
        }

        return statValue;
    }

    public static void SetUnitStatData(StatData _stat, UnitLogicStat _unit)
    {
        switch (_stat.stat)
        {
            case Data.Define.EStatType.NONE:
                break;
            case Data.Define.EStatType.STR:
                _unit.strength += _stat.val;
                _unit.life += _stat.val * 10;
                break;
            case Data.Define.EStatType.AGI:
                _unit.agility += _stat.val;
                _unit.evasion += _stat.val * 5f;
                _unit.atk_speed = 1 + (_stat.val / 10);
                _unit.critical += _stat.val / 3 * 5f;
                break;
            case Data.Define.EStatType.INT:
                _unit.intelligence += _stat.val;
                _unit.def += _stat.val * 5f;
                break;
            default:
                break;
        }

        SetUnitTypeStatData(_stat, ref _unit);
    }

    public static void SetUnitTypeStatData(StatData _stat, ref UnitLogicStat _unit)
    {
        switch (_unit.unitType)
        {   
            case Define.EUnitType.None:
                break;
            case Define.EUnitType.Melee:
                if(_stat.stat == Define.EStatType.STR)
                    _unit.atk += _stat.val;
                break;
            case Define.EUnitType.Ranged:
                if (_stat.stat == Define.EStatType.AGI)
                    _unit.atk += _stat.val;
                break;
            case Define.EUnitType.Support:
                if (_stat.stat == Define.EStatType.INT)
                    _unit.atk += _stat.val;
                break;
            default:
                break;
        }
    }

    
    public static async void AddMoveAreaDamage(this UnitLogic _unit
        , int _skillID
       , Define.EUNIT_STATE _unitState
       , Vector3 _pos
       , float moveTime
       , Vector3 _size, float _tick,
       float _delayTime, float _duration, float _damage, Define.EDAMAGE_TYPE _damageType)
    {
        ProjectileLogic collider = null;
        collider = Managers.Pool.PopBoxCollider();
        if (collider == null)
            collider = await Managers.Pool.CreateBoxCollider();

        if (collider == null)
            return;

#if UNITY_EDITOR
        collider.name = $"{_unit.name}_{_unitState}";
#endif
        collider.transform.SetParent(_unit.GetOwner.GameData.PlayInfo.transform);
        collider.transform.position = _pos;

        collider.Init(_unit, _skillID, _unitState, _tick, _delayTime, _duration, _damage, _damageType);
        _unit.GetOwner.AddObject(collider);
    }

    public static Define.EDAMAGE_TYPE GetDamageType(this UnitLogic _unit)
    {
        switch (_unit.stat.unitType)
        {
           
            case Define.EUnitType.Melee:
            case Define.EUnitType.Ranged:
                return Define.EDAMAGE_TYPE.PHYSICAL;

            case Define.EUnitType.Support:
                return Define.EDAMAGE_TYPE.MAGICAL;

            default:
                return Define.EDAMAGE_TYPE.PHYSICAL;
        }
    }
}

public class StatData
{
    public Define.EStatType stat;
    public long val;
}
