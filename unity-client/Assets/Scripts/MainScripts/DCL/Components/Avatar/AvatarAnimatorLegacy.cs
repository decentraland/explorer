using System;
using UnityEngine;

public class AvatarAnimatorLegacy : MonoBehaviour
{
    const float IDLE_TRANSITION_TIME = 0.15f;
    const float STRAFE_TRANSITION_TIME = 0.25f;
    const float RUN_TRANSITION_TIME = 0.01f;
    const float WALK_TRANSITION_TIME = 0.01f;
    const float JUMP_TRANSITION_TIME = 0.01f;
    const float FALL_TRANSITION_TIME = 0.5f;
    const float EXPRESSION_TRANSITION_TIME = 0.2f;

    const float AIR_EXIT_TRANSITION_TIME = 0.2f;
    const float GROUND_BLENDTREE_TRANSITION_TIME = 0.15f;

    const float RUN_SPEED_THRESHOLD = 0.05f;
    const float WALK_SPEED_THRESHOLD = 0.03f;

    const float ELEVATION_OFFSET = 0.6f;
    const float RAY_OFFSET_LENGTH = 3.0f;

    const float MAX_VELOCITY = 6.25f;

    [System.Serializable]
    public class Clips
    {
        public string idle;
        public string walk;
        public string run;
        public string jump;
        public string fall;
        public string special; //TODO(Brian): Not implemented yet
    }

    [System.Serializable]
    public class BlackBoard
    {
        public float walkSpeedFactor;
        public float runSpeedFactor;
        public float movementSpeed;
        public float verticalSpeed;
        public bool isGrounded;
        public string expressionTriggerId;
        public long expressionTriggerTimestamp;
    }

    [SerializeField] private AnimationClipsVariable maleAnimations;
    [SerializeField] private AnimationClipsVariable femaleAnimations;
    public new Animation animation;
    public Clips clips;
    public BlackBoard blackboard;
    public Transform target;

    public AnimationCurve walkBlendtreeCurve;
    public AnimationCurve runBlendtreeCurve;
    public AnimationCurve idleBlendtreeCurve;

    public bool useDeltaTimeInsteadOfGlobalSpeed = false;
    public float globalSpeed = 0.05f;

    System.Action<BlackBoard> currentState;

    Vector3 lastPosition;

    void Start()
    {
        currentState = State_Init;
    }

    void Update()
    {
        if (target == null || animation == null)
            return;

        UpdateInterface();
        currentState?.Invoke(blackboard);
    }


    void UpdateInterface()
    {
        Vector3 flattenedVelocity = target.position - lastPosition;

        //NOTE(Brian): Vertical speed
        float verticalVelocity = flattenedVelocity.y;
        blackboard.verticalSpeed = verticalVelocity;

        flattenedVelocity.y = 0;

        blackboard.movementSpeed = flattenedVelocity.magnitude;

        Vector3 rayOffset = Vector3.up * RAY_OFFSET_LENGTH;
        //NOTE(Brian): isGrounded?
        blackboard.isGrounded = Physics.Raycast(target.transform.position + rayOffset,
                                                Vector3.down,
                                                RAY_OFFSET_LENGTH - ELEVATION_OFFSET,
                                                DCLCharacterController.i.groundLayers);

#if UNITY_EDITOR
        Debug.DrawRay(target.transform.position + rayOffset, Vector3.down * (RAY_OFFSET_LENGTH - ELEVATION_OFFSET), blackboard.isGrounded ? Color.green : Color.red);
#endif

        lastPosition = target.position;
    }



    void State_Init(BlackBoard bb)
    {
        if (bb.isGrounded == true)
        {
            currentState = State_Ground;
        }
        else
        {
            currentState = State_Air;
        }
    }



    void State_Ground(BlackBoard bb)
    {
        float dt;

        if (useDeltaTimeInsteadOfGlobalSpeed)
            dt = Time.deltaTime;
        else
            dt = globalSpeed;

        animation[clips.run].normalizedSpeed = bb.movementSpeed / dt * bb.runSpeedFactor;
        animation[clips.walk].normalizedSpeed = bb.movementSpeed / dt * bb.walkSpeedFactor;

        float normalizedSpeed = bb.movementSpeed / dt / MAX_VELOCITY;

        float idleWeight = idleBlendtreeCurve.Evaluate(normalizedSpeed);
        float runWeight = runBlendtreeCurve.Evaluate(normalizedSpeed);
        float walkWeight = walkBlendtreeCurve.Evaluate(normalizedSpeed);

        //NOTE(Brian): Normalize weights
        float weightSum = idleWeight + runWeight + walkWeight;

        idleWeight /= weightSum;
        runWeight /= weightSum;
        walkWeight /= weightSum;

        animation.Blend(clips.idle, idleWeight, GROUND_BLENDTREE_TRANSITION_TIME);
        animation.Blend(clips.run, runWeight, GROUND_BLENDTREE_TRANSITION_TIME);
        animation.Blend(clips.walk, walkWeight, GROUND_BLENDTREE_TRANSITION_TIME);

        if (!bb.isGrounded)
        {
            currentState = State_Air;
            Update();
        }
    }

    void State_Air(BlackBoard bb)
    {
        if (bb.verticalSpeed > 0)
        {
            animation.CrossFade(clips.jump, JUMP_TRANSITION_TIME, PlayMode.StopAll);
        }
        else
        {
            animation.CrossFade(clips.fall, FALL_TRANSITION_TIME, PlayMode.StopAll);
        }

        if (bb.isGrounded)
        {
            animation.Blend(clips.jump, 0, AIR_EXIT_TRANSITION_TIME);
            animation.Blend(clips.fall, 0, AIR_EXIT_TRANSITION_TIME);
            currentState = State_Ground;
            Update();
        }
    }

    void State_Expression(BlackBoard bb)
    {
        var animationInfo = animation[bb.expressionTriggerId];
        animation.CrossFade(bb.expressionTriggerId, EXPRESSION_TRANSITION_TIME, PlayMode.StopAll);

        var mustExit = Math.Abs(bb.movementSpeed) > Mathf.Epsilon || animationInfo.length - animationInfo.time < EXPRESSION_TRANSITION_TIME || !bb.isGrounded;
        if (mustExit)
        {
            animation.Blend(bb.expressionTriggerId, 0, EXPRESSION_TRANSITION_TIME);
            bb.expressionTriggerId = null;
            if (!bb.isGrounded)
                currentState = State_Air;
            else
                currentState = State_Ground;
            Update();
        }
    }

    public void SetExpressionValues(string expressionTriggerId, long expressionTriggerTimestamp)
    {
        var mustTriggerAnimation = !string.IsNullOrEmpty(expressionTriggerId) && blackboard.expressionTriggerTimestamp != expressionTriggerTimestamp;

        if (!string.IsNullOrEmpty(expressionTriggerId))
        {
            animation.Stop(expressionTriggerId);
        }
        
        blackboard.expressionTriggerId = expressionTriggerId;
        blackboard.expressionTriggerTimestamp = expressionTriggerTimestamp;

        if (mustTriggerAnimation)
        {
            currentState = State_Expression;
            Update();
        }
    }

    public void Reset()
    {
        //It will set the animation to the first frame, but due to the nature of the script and its Update. It wont stop the animation from playing
        animation.Stop();
    }

    public void SetIdleFrame()
    {
        animation.Play(clips.idle);
    }

    public void BindBodyShape(Animation animation, string bodyShapeType, Transform target)
    {
        this.target = target;
        this.animation = animation;
        AnimationClip[] animArray = null;

        if (bodyShapeType.Contains(WearableLiterals.BodyShapes.MALE))
        {
            animArray = maleAnimations;
        }
        else if (bodyShapeType.Contains(WearableLiterals.BodyShapes.FEMALE))
        {
            animArray = femaleAnimations;
        }

        for (int index = 0; index < animArray.Length; index++)
        {
            var clip = animArray[index];
            if (this.animation.GetClip(clip.name) == null)
                this.animation.AddClip(clip, clip.name);
        }

        SetIdleFrame();
    }
}
