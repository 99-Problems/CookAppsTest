using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "공통/특정 대상에 뭔가를 한다", 1), NodeTitle("공통/특정 대상에 뭔가를 한다")]
    public class ExecuteAttackTargetNode : BaseNode
    {
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;


        public UnitLogic targetUnit;

        public int skillID = -1;

        [LabelText("공간 사이즈")]
        public Vector3 size;


        [LabelText("지연 대기 시간")]
        public float delayTime = 0;

        [LabelText("반복 시간")]
        public float tick = 1;

        [LabelText("재생 시간")]
        public float duration = 0.1f;

        [LabelText("커스텀 위치")]
        public bool isCustomPosition;

        [LabelText("공간 위치")]
        [ShowIf("@isCustomPosition != true")]
        public Vector3 offset;

        [Output]
        public bool output = true;

        public override void OnEnter()
        {
            base.OnEnter();
            var unitLogic = GetUnitLogicInfo();
            if (unitLogic == null)
            {
                MoveNextNode(this, GetPort("output"));
                return;
            }
            //damage = unitLogic.stat.atk;

            var pos = offset;
            

            pos += unitLogic.transform.position;

            unitLogic.AddMoveAreaDamage(skillID, GetStateType(), pos, -1, size, tick, delayTime, duration, unitLogic.stat.atk, unitLogic.GetDamageType());
            //Debug.ColorLog($"{unitLogic.GetOwner.name}'s {unitLogic.name} attacked");
            MoveNextNode(this, GetPort("output"));
        }

        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }
    }
}