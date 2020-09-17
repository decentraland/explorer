using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLerpController : MonoBehaviour
{
    public float lerpingSpeed = 20f;

    Queue<Vector3> targetPositions = new Queue<Vector3>();

    public void AddTargetPosition(Vector3 newTargetPos)
    {
        targetPositions.Enqueue(newTargetPos);
    }

    void Update()
    {
        if (targetPositions.Count == 0) return;
        var targetPos = targetPositions.Peek();

        var currentTime = lerpingSpeed * Time.deltaTime;

        if (lerpingSpeed <= 0 || currentTime >= 1)
        {
            transform.localPosition = targetPos;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, currentTime);
        }

        if (Vector3.Distance(transform.localPosition, targetPos) < 0.1)
            targetPositions.Dequeue();
    }
}
