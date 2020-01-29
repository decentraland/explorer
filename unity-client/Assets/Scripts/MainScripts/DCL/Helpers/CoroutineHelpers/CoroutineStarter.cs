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
            if (instanceValue == null)
            {
                instanceValue = new GameObject("_CoroutineStarter").AddComponent<CoroutineStarter>();
            }

            return instanceValue;
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
            int count = coroutines.Count;

            for (int i = 0; i < count; i++)
            {
                yield return RunIterator(coroutines[i]);
            }

            coroutines = coroutines.Where(x => x.stack.Count > 0).ToList();

            yield return null;
        }
    }

    bool DecideIfObjectShouldSkipFrame(object yieldedObject)
    {
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

        if (DecideIfObjectShouldSkipFrame(coroutine.currentYieldInstruction))
            yield break;

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

            if (DecideIfObjectShouldSkipFrame(currentYieldedObject))
            {
                coroutine.currentYieldInstruction = currentYieldedObject;
                yield break;
            }
            else if (currentYieldedObject is IEnumerator)
            {
                stack.Push(currentYieldedObject as IEnumerator);
            }
        }
    }

    public static Coroutine Start(IEnumerator function)
    {
        Coroutine coroutine = new Coroutine();
        coroutine.stack.Push(function);
        instance.coroutines.Add(coroutine);
        return coroutine;
    }

    public static void Stop(Coroutine coroutine)
    {
        if (instanceValue != null)
            instance.coroutines.Remove(coroutine);
    }

}
