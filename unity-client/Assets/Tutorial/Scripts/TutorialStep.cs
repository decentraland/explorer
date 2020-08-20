using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    public interface ITutorialStep
    {
        void OnStepStart();
        IEnumerator OnStepExecute();
        void OnStepFinished();
    }

    /// <summary>
    /// Class that represents one of the steps included in the onboarding tutorial.
    /// </summary>
    public class TutorialStep : MonoBehaviour, ITutorialStep
    {
        /// <summary>
        /// Step initialization (occurs before OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepStart()
        {
        }

        /// <summary>
        /// Executes the main flow of the step.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepExecute()
        {
            yield break;
        }

        /// <summary>
        /// Step finalization (occurs after OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepFinished()
        {
        }
    }
}