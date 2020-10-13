﻿using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IBlockerAnimationHandler
    {
        void FadeIn(GameObject go);
        void FadeOut(GameObject go, System.Action OnFinish);
    }

    public class BlockerAnimationHandler : IBlockerAnimationHandler
    {
        public void FadeIn(GameObject go)
        {
            CoroutineStarter.Start(FadeInCoroutine(go));
        }

        public void FadeOut(GameObject go, System.Action OnFinish)
        {
            CoroutineStarter.Start(FadeOutCoroutine(go, OnFinish));
        }

        IEnumerator FadeInCoroutine(GameObject go)
        {
            Renderer rend = go.GetComponent<Renderer>();

            Color color = rend.material.GetColor(ShaderUtils._BaseColor);

            while (color.a < 0.5f)
            {
                color.a += Time.deltaTime;
                rend.material.SetColor(ShaderUtils._BaseColor, color);
                yield return null;
            }
        }

        IEnumerator FadeOutCoroutine(GameObject go, System.Action OnFinish)
        {
            Renderer rend = go.GetComponent<Renderer>();

            Color color = rend.material.GetColor(ShaderUtils._BaseColor);

            while (color.a > 0)
            {
                if (rend == null)
                    break;

                color.a -= Time.deltaTime;
                rend.material.SetColor(ShaderUtils._BaseColor, color);
                yield return null;
            }

            OnFinish?.Invoke();
        }
    }
}