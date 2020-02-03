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
        public Stack<IEnumerator> stack = new Stack<IEnumerator>();
        public object currentYieldInstruction = null;
    }

    List<Coroutine> coroutines = new List<Coroutine>();

    public void Start()
    {
        StartCoroutine(MainCoroutine());
    }

    IEnumerator MainCoroutine()
    {
        while (true)
        {
            if (coroutinesToAdd.Count > 0)
            {
                for (int i = 0; i < coroutinesToAdd.Count; i++)
                    coroutines.Add(coroutinesToAdd[i]);

                coroutinesToAdd.Clear();
            }

            if (coroutinesToRemove.Count > 0)
            {
                for (int i = 0; i < coroutinesToRemove.Count; i++)
                    coroutines.Remove(coroutinesToRemove[i]);

                coroutinesToRemove.Clear();
            }

            int count = coroutines.Count;

            if (count <= 0)
                yield return null;

            for (int i = 0; i < count; i++)
            {
                yield return RunIterator(coroutines[i]);
            }

            coroutines = coroutines.Where(x => x.stack.Count > 0).ToList();

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

    public IEnumerator RunIterator(CoroutineStarter.Coroutine coroutine)
    {
        var stack = coroutine.stack;

        if (stack.Count == 0)
            yield break;

        if (ShouldSkipFrame(coroutine.currentYieldInstruction))
            yield break;

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

            if (currentYieldedObject is IEnumerator)
            {
                stack.Push(currentYieldedObject as IEnumerator);
            }
            else if (ShouldSkipFrame(currentYieldedObject))
            {
                coroutine.currentYieldInstruction = currentYieldedObject;
                yield break;
            }
        }
    }

    private static List<Coroutine> coroutinesToAdd = new List<Coroutine>();
    private static List<Coroutine> coroutinesToRemove = new List<Coroutine>();
    public static Coroutine Start(IEnumerator function)
    {
        EnsureInstance();
        Coroutine coroutine = new Coroutine();
        coroutine.stack.Push(function);
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

}
