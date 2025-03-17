using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using CustomNode.State;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public interface IUnitLogicMovement
{
    void Init(UnitLogic _logic);

    void FrameMove(float _delta);
}

public class UnitLogicMovement : IUnitLogicMovement
{
    private UnitLogic unitLogic;


    public void Init(UnitLogic _logic)
    {
        unitLogic = _logic;
    }

    public void FrameMove(float _delta)
    {
        unitLogic.nodeGraph.FrameMove(_delta * unitLogic.GetAtkSpeedPer);
    }

    //private void Move(float _delta)
    //{
    //    var pathCorners = unitLogic.path.corners;
    //    if (pathCorners == null || pathCorners.Length <= 1)
    //    {
    //        if (unitLogic.path.status == NavMeshPathStatus.PathComplete)
    //        {
    //            //unitLogic.SetTargetPosition(Vector3.positiveInfinity);
    //        }

    //        return;
    //    }

    //    Vector3 target = Vector3.positiveInfinity;
    //    for (int i = 1; i < pathCorners.Length; ++i)
    //    {
    //        var offset = pathCorners[i] - pathCorners[0];
    //        if (offset.magnitude >= 0.01f)
    //        {
    //            target = pathCorners[i];
    //            break;
    //        }
    //    }

    //    if (float.IsPositiveInfinity(target.x))
    //    {
    //        //unitLogic.SetTargetPosition(Vector3.positiveInfinity);
    //        return;
    //    }

    //    //unitLogic.AddUnitPosition((target - unitLogic.transform.position).normalized * _delta * unitLogic.statSo.speed);
    //}


    //private void MoveBasic()
    //{
        
    //}

}
