using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Tests
{
    public class CoroutineStarterTests
    {
        int coroutineCounter = 0;
        bool coroutineEnded2 = false;
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CoroutineStarterWorks()
        {
            CoroutineStarter.Start(CoroutineTest1("#1"));
            //CoroutineStarter.Start(CoroutineTest1("#2"));
            //CoroutineStarter.Start(CoroutineTest1("#3"));
            //CoroutineStarter.Start(CoroutineTest1("#4"));
            //CoroutineStarter.Start(CoroutineTest1("#5"));
            //CoroutineStarter.Start(CoroutineTest1("#6"));
            //CoroutineStarter.Start(CoroutineTest2());
            yield return new WaitUntil(() => coroutineCounter == 1 /*&& coroutineEnded2*/);
        }

        public IEnumerator CoroutineTest1(string argument)
        {
            Debug.Log("1-Frame: " + Time.frameCount);
            yield return NestedIEnumerator(argument);
            Debug.Log("2-Frame: " + Time.frameCount);
            yield return NestedIEnumerator(argument);
            Debug.Log("3-Frame: " + Time.frameCount);
            yield return NestedIEnumerator(argument);
            coroutineCounter++;
        }
        public IEnumerator CoroutineTest2()
        {
            Debug.Log("A");
            yield return null;
            Debug.Log("B");
            yield return null;
            Debug.Log("C");
            yield return NestedIEnumerator("");
            float time = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);
            Debug.Log("After waited " + (Time.realtimeSinceStartup - time));

            var req = UnityWebRequest.Get("www.google.com");
            yield return req.SendWebRequest();
            Debug.Log("req = " + req.downloadHandler.text);
            coroutineEnded2 = true;
        }

        public IEnumerator NestedIEnumerator(string argument)
        {
            yield return null;
            Debug.Log("Nested works? " + argument + " ... " + Time.frameCount);
        }
    }
}
