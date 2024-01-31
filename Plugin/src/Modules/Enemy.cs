using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TestingLib {
    /// <summary>
    /// Helpful methods for making debugging of enemies easier.
    /// </summary>
    public class Enemy {
        /// <summary>
        /// Draws the NavMeshAgent's pathfinding. Should be used in `DoAIInterval()`. Do note that you need to add line renderer in your enemy prefab.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="path"></param>
        /// <param name="fromPosition"></param>
        /// <returns></returns>
        public static IEnumerator DrawPath(LineRenderer line, NavMeshPath path, Transform fromPosition){
            yield return new WaitForEndOfFrame();
            line.SetPosition(0, fromPosition.position); //set the line's origin

            line.positionCount = path.corners.Length; //set the array of positions to the amount of corners
            for(var i = 1; i < path.corners.Length; i++){
                line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
            }
        }
    }
}