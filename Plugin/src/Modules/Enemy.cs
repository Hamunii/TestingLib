using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TestingLib {
    public class Enemy {
        public static IEnumerator DrawPathIEnumerator(LineRenderer line, NavMeshPath path, Transform fromPosition){
            yield return new WaitForEndOfFrame();
            line.SetPosition(0, fromPosition.position); //set the line's origin

            line.positionCount = path.corners.Length; //set the array of positions to the amount of corners
            for(var i = 1; i < path.corners.Length; i++){
                line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
            }
        }
    }
}