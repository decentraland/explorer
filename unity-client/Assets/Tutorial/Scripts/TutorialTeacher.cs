using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// This class controls the behaviour of the teacher (a 3D model character) that will be guiding to the player along the tutorial.
    /// </summary>
    public class TutorialTeacher : MonoBehaviour
    {
        public enum TeacherAnimation
        {
            Hello,
            Idle,
            Goodbye,
            QuickGoodbye
        }

        [SerializeField] Animation teacherAnimation;
        [SerializeField] AnimationClip helloAnimationClip;
        [SerializeField] AnimationClip idleAnimationClip;
        [SerializeField] AnimationClip goodByeAnimationClip;
        [SerializeField] AnimationClip quickkGoodByeAnimationClip;
        [SerializeField] TeacherAnimation defaultAnimationClip = TeacherAnimation.Idle;

        private Coroutine runningCoroutine;

        private void Start()
        {
            PlayAnimation(defaultAnimationClip);
        }

        /// <summary>
        /// Play an animation.
        /// </summary>
        /// <param name="animation">Animation to play.</param>
        public void PlayAnimation(TeacherAnimation animation)
        {
            if (!isActiveAndEnabled)
                return;

            if (runningCoroutine != null)
                StopCoroutine(runningCoroutine);

            switch (animation)
            {
                case TeacherAnimation.Hello:
                    teacherAnimation.clip = helloAnimationClip;
                    break;
                case TeacherAnimation.Idle:
                    teacherAnimation.clip = idleAnimationClip;
                    break;
                case TeacherAnimation.Goodbye:
                    teacherAnimation.clip = goodByeAnimationClip;
                    break;
                case TeacherAnimation.QuickGoodbye:
                    teacherAnimation.clip = quickkGoodByeAnimationClip;
                    break;
                default:
                    break;
            }

            teacherAnimation.Play();
            if (teacherAnimation.clip.wrapMode == WrapMode.Once)
                runningCoroutine = StartCoroutine(WaitForAnimationEndAndReturnToDefault(teacherAnimation.clip));
        }

        private IEnumerator WaitForAnimationEndAndReturnToDefault(AnimationClip clip)
        {
            yield return new WaitForSeconds(clip.length);

            PlayAnimation(defaultAnimationClip);
        }
    }
}