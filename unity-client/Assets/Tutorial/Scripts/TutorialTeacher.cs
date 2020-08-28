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
            Idle,
            Hello,
            Goodbye,
            QuickGoodbye
        }

        [SerializeField] Animator teacherAnimator;

        /// <summary>
        /// Play an animation.
        /// </summary>
        /// <param name="animation">Animation to play.</param>
        public void PlayAnimation(TeacherAnimation animation)
        {
            if (!isActiveAndEnabled)
                return;

            switch (animation)
            {
                case TeacherAnimation.Idle:
                    teacherAnimator.SetTrigger("Idle");
                    break;
                case TeacherAnimation.Hello:
                    teacherAnimator.SetTrigger("Hello");
                    break;
                case TeacherAnimation.Goodbye:
                    teacherAnimator.SetTrigger("Goodbye");
                    break;
                case TeacherAnimation.QuickGoodbye:
                    teacherAnimator.SetTrigger("QuickGoodbye");
                    break;
                default:
                    break;
            }
        }
    }
}