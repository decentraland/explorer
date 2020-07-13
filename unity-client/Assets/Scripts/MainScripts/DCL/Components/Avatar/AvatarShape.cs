using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

        public AvatarName avatarName;
        public AvatarRenderer avatarRenderer;
        public AvatarMovementController avatarMovementController;
        [SerializeField] internal GameObject minimapRepresentation;
        [SerializeField] private AvatarOnPointerDown onPointerDown;
        private StringVariable currentPlayerInfoCardId;

        private string currentSerialization = "";
        public AvatarModel model = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        private MinimapMetadata.MinimapUserInfo avatarUserInfo = new MinimapMetadata.MinimapUserInfo();

        private void Awake()
        {
            OnReset();
        }

        void Start()
        {
            if (poolableObject != null)
            {
                poolableObject.OnRelease += Cleanup;
                poolableObject.OnReset += OnReset;
            }
        }

        private void PlayerClicked()
        {
            currentPlayerInfoCardId.Set(model?.id);
        }

        void OnDestroy()
        {
            Cleanup();

            if (poolableObject != null && poolableObject.isInsidePool)
                poolableObject.pool.RemoveFromPool(poolableObject);
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): Horrible fix to the double ApplyChanges call, as its breaking the needed logic.
            if (newJson == "{}")
                yield break;

            if (currentSerialization == newJson)
                yield break;

            model = SceneController.i.SafeFromJson<AvatarModel>(newJson);

            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            avatarRenderer.ApplyModel(model, () => avatarDone = true, () => avatarFailed = true);

            yield return new WaitUntil(() => avatarDone || avatarFailed);

            avatarName.SetName(model.name);
            SetMinimapRepresentationActive(true);
            everythingIsLoaded = true;

            onPointerDown.collider.enabled = true;

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = lastAvatarPosition != null ? lastAvatarPosition.Value : minimapRepresentation.transform.position;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo);
        }

        void SetMinimapRepresentationActive(bool active)
        {
            if (minimapRepresentation == null)
                return;

            minimapRepresentation.SetActive(active);
        }

        private void OnEntityTransformChanged(DCLTransform.Model updatedModel)
        {
            lastAvatarPosition = updatedModel.position;

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = updatedModel.position;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo);
        }

        public void OnReset()
        {
            AvatarAnimatorLegacy animator = GetComponent<AvatarAnimatorLegacy>();

            if (animator != null)
                animator.OnReset();

            AvatarMovementController movement = GetComponent<AvatarMovementController>();

            if (movement != null)
                movement.OnReset();

            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);

            if (string.IsNullOrEmpty(currentSerialization))
                SetMinimapRepresentationActive(false);

            onPointerDown.OnPointerDownReport += PlayerClicked;
            onPointerDown.Setup(scene, entity, new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                hoverText = "view profile"
            });

            everythingIsLoaded = false;
            currentSerialization = "";
            model = new AvatarModel();
            lastAvatarPosition = null;
            avatarUserInfo = new MinimapMetadata.MinimapUserInfo();

            if (entity != null && entity.OnTransformChange == null)
            {
                entity.OnTransformChange += avatarMovementController.OnTransformChanged;
                entity.OnTransformChange += OnEntityTransformChanged;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();

            //Debug.Log("Avatar shape clean " + avatarUserInfo.userName);
            avatarRenderer.CleanupAvatar();

            if (poolableObject != null)
            {
                poolableObject.OnRelease -= Cleanup;
            }

            onPointerDown.OnPointerDownReport -= PlayerClicked;

            if (entity != null)
            {
                entity.OnTransformChange = null;
                avatarUserInfo.userId = model.id;
                MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo, true);
            }
        }
    }
}