using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter instanceValue;

    private static CoroutineStarter instance
    {
        get
        {
            EnsureInstance();
            return instanceValue;
        }
    }

    private static void EnsureInstance()
    {
        if (instanceValue == null)
        {
            instanceValue = new GameObject("_CoroutineStarter").AddComponent<CoroutineStarter>();
        }
    }

    public class Coroutine
    {
        public float timeBudget = 0.01f;
        public int priority = 0;
        internal Stack<IEnumerator> stack = new Stack<IEnumerator>();
        internal object currentYieldInstruction = null;
    }

    List<Coroutine> coroutines = new List<Coroutine>();
    Coroutine currentRunningCoroutine;
    float currentRunningCoroutineStartTime;
    float currentRunningCoroutineRemainingTime;

    public static float GetRemainingBudget()
    {
        return instance.currentRunningCoroutineRemainingTime;
    }


    public static float globalTimeBudget = 0.01f;

    public void Start()
    {
        StartCoroutine(MainCoroutine());
    }

    IEnumerator MainCoroutine()
    {
        while (true)
        {
            float globalStartTime = Time.realtimeSinceStartup;

            bool listDirty = false;

            if (coroutinesToAdd.Count > 0)
            {
                for (int i1 = 0; i1 < coroutinesToAdd.Count; i1++)
                    coroutines.Add(coroutinesToAdd[i1]);

                listDirty = true;
                coroutinesToAdd.Clear();
            }

            if (coroutinesToRemove.Count > 0)
            {
                for (int i2 = 0; i2 < coroutinesToRemove.Count; i2++)
                    coroutines.Remove(coroutinesToRemove[i2]);

                listDirty = true;
                coroutinesToRemove.Clear();
            }

            if (listDirty)
            {
                coroutines = coroutines.OrderBy((x) => { return x.priority; }).ToList();
            }

            int count = coroutines.Count;

            if (count <= 0)
                yield return null;

            for (int i = 0; i < count; i++)
            {
                RunCoroutineFrame(coroutines[i]);

                if (Time.realtimeSinceStartup - globalStartTime > globalTimeBudget)
                    break;
            }

            yield return null;
        }
    }

    bool ShouldSkipFrame(object yieldedObject)
    {
        if (yieldedObject == null)
            return false;

        switch (yieldedObject)
        {
            case CustomYieldInstruction yield:
                if (yield.keepWaiting)
                    return true;

                break;

            case AsyncOperation asyncOp:
                if (!asyncOp.isDone)
                    return true;

                break;
        }

        return false;
    }

    public void RunCoroutineFrame(CoroutineStarter.Coroutine coroutine)
    {
        currentRunningCoroutine = coroutine;
        currentRunningCoroutineStartTime = Time.realtimeSinceStartup;

        var stack = coroutine.stack;

        if (stack.Count == 0)
            return;

        if (ShouldSkipFrame(coroutine.currentYieldInstruction))
            return;

        coroutine.currentYieldInstruction = null;

        while (stack.Count > 0)
        {
            var currentEnumerator = stack.Peek();
            object currentYieldedObject;

            if (currentEnumerator.MoveNext() == false)
            {
                stack.Pop();
                continue;
            }

            currentYieldedObject = currentEnumerator.Current;

            if (currentYieldedObject == null)
                break;

            float elapsedTime = Time.realtimeSinceStartup - currentRunningCoroutineStartTime;
            currentRunningCoroutineRemainingTime = coroutine.timeBudget - elapsedTime;

            if (currentYieldedObject is BreakIfBudgetExceededInstruction)
            {
                if (!(currentYieldedObject as BreakIfBudgetExceededInstruction).keepWaiting)
                    break;

                continue;
            }

            if (ShouldSkipFrame(currentYieldedObject))
            {
                coroutine.currentYieldInstruction = currentYieldedObject;
                return;
            }
            else if (currentYieldedObject is IEnumerator)
            {
                stack.Push(currentYieldedObject as IEnumerator);
            }

            if (elapsedTime > coroutine.timeBudget)
            {
                break;
            }
        }

        if (stack.Count == 0)
            coroutinesToRemove.Add(coroutine);
    }



    private static List<Coroutine> coroutinesToAdd = new List<Coroutine>();
    private static List<Coroutine> coroutinesToRemove = new List<Coroutine>();
    public static Coroutine Start(IEnumerator function, int priority = 0)
    {
        EnsureInstance();

        Coroutine coroutine = new Coroutine();
        coroutine.stack.Push(function);
        coroutine.priority = priority;

        coroutinesToAdd.Add(coroutine);
        return coroutine;
    }

    public static void Stop(Coroutine coroutine)
    {
        EnsureInstance();

        if (coroutinesToAdd.Contains(coroutine))
        {
            coroutinesToAdd.Remove(coroutine);
            return;
        }

        if (!coroutinesToRemove.Contains(coroutine))
            coroutinesToRemove.Add(coroutine);
    }

    public class BreakIfBudgetExceededInstruction : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                float elapsedTime = Time.realtimeSinceStartup - instance.currentRunningCoroutineStartTime;
                return elapsedTime <= instance.currentRunningCoroutine.timeBudget;
            }
        }
    }

    BreakIfBudgetExceededInstruction cachedBreakIfBudgetExceededInstruction = new BreakIfBudgetExceededInstruction();
    public static BreakIfBudgetExceededInstruction BreakIfBudgetExceeded()
    {
        return instance.cachedBreakIfBudgetExceededInstruction;
    }

}
