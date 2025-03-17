using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class InGamePlayerInfo : MonoBehaviour
{
    public readonly List<UnitLogic> listUnit = new List<UnitLogic>();

    public IGameData GameData { get; set; }
    public static Subject<InGamePlayerInfo> OnInitPlayer = new Subject<InGamePlayerInfo>();

    public struct PlayerData
    {
        public int playerIndex;
        public string nick;
        public long accountID;
        public float lifeRate;
    }
    public PlayerData playerData;
    public struct DamageLog
    {
        public int unitID;
        public int unitIndex;
        public double damage;
        public double recieve;
        public double heal;
        public bool isDie;
        public double unitHp;
    }
    public List<DamageLog> damageLog = new List<DamageLog>();

    public List<ProjectileLogic> listLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> addLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> removeLogicObject = new List<ProjectileLogic>();

    public void Init(IEnumerable<UnitLogic> _listMyUnit)
    {
        listUnit.Clear();
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                if (mit == null)
                    continue;

                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit.AddRange(_listMyUnit);
        }
    }

    public UnitLogic GetUnitFromID(int _mitUnitID)
    {
        for (var index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (mit.UnitID == _mitUnitID)
                return mit;
        }

        return null;
    }

    public void FrameMove(float _deltaTime)
    {
        for (int index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (!mit.gameObject.activeInHierarchy)
                continue;

            mit.FrameMove(_deltaTime);
        }

        {
            foreach (var mit in listLogicObject)
            {
                if (mit)
                {
                    mit.FrameMove(_deltaTime * Managers.Time.GetGameSpeed());
                }
            }

            listLogicObject.AddRange(addLogicObject);
            addLogicObject.Clear();

            foreach (var mit in removeLogicObject)
            {
                if (mit == null)
                    continue;
                Managers.Pool.PushCollider(mit);
                listLogicObject.Remove(mit);
            }

            removeLogicObject.Clear();
        }
    }

    public void Entry()
    {

    }

    public void AddObject(ProjectileLogic _logic)
    {
        addLogicObject.Add(_logic);
    }

    public void RemoveObject(ProjectileLogic _logic)
    {
        removeLogicObject.Add(_logic);
    }

    public float GetRandom(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    internal bool IsPlayerDie()
    {
        var isDead = true;
        foreach (var unit in listUnit)
        {
            if (unit.IsDie == false)
            {
                isDead = false;
                break;
            }
        }
        return isDead;
    }

    public float GetPartyHpRate()
    {
        long fullHp = 0;
        float hp = 0;
        foreach (var unit in listUnit)
        {
            fullHp += unit.GetOriginalFullHp;
            hp += unit.hp;
        }

        return hp / fullHp;
    }

    public List<DamageLog> GetLog()
    {
        var log = damageLog;
        for (var index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            log.Add(new DamageLog
            {
                unitID = mit.UnitID,
                unitIndex = mit.partyIndex,
                damage = mit.damageLog,
                recieve = mit.recieveLog,
                heal = mit.healLog,
                isDie = mit.IsDie,
                unitHp = mit.hp,
            });
        }

        return log;
    }
}
