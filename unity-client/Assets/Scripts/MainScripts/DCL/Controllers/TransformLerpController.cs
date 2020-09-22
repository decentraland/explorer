using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Monobehaviour that handles updating the entities transformation updates, if a lerpingSpeed is given throught the SDK or auto-calculated, the updates will be lerped, otherwise they will be applied instantly
/// </summary>
public class TransformLerpController : MonoBehaviour
{
    public float lerpingSpeed = 0;

    Queue<Vector3> targetPositions = new Queue<Vector3>();

    public void AddTargetPosition(Vector3 newTargetPos)
    {
        if (lerpingSpeed <= 0)
        {
            transform.localPosition = newTargetPos;
            return;
        }

        targetPositions.Enqueue(newTargetPos);
    }

    void Update()
    {
        if (targetPositions.Count == 0) return;

        var targetPos = targetPositions.Peek();
        var currentTime = lerpingSpeed * Time.deltaTime;

        if (lerpingSpeed <= 0 || currentTime >= 1)
            transform.localPosition = targetPos;
        else
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, currentTime);

        if (Vector3.Distance(transform.localPosition, targetPos) < 0.1)
            targetPositions.Dequeue();
    }
}
