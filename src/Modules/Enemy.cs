using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TestingLib.Modules;
/// <summary>
/// Helpful methods for making debugging of enemies easier.
/// </summary>
public class Enemy {
    /// <summary>
    /// Draws the NavMeshAgent's pathfinding. Should be used in `DoAIInterval()`. Do note that you need to add line renderer in your enemy prefab.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="agent"></param>
    /// <returns></returns>
    public static IEnumerator DrawPath(LineRenderer line, NavMeshAgent agent){
        if(!agent.enabled) yield break;
        yield return new WaitForEndOfFrame();
        line.SetPosition(0, agent.transform.position); //set the line's origin
        
        line.positionCount = agent.path.corners.Length; //set the array of positions to the amount of corners
        for(var i = 1; i < agent.path.corners.Length; i++){
            line.SetPosition(i, agent.path.corners[i]); //go through each corner and set that to the line renderer's position
        }
    }
}