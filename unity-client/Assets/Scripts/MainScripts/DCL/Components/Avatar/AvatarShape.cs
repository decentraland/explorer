using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

        public AvatarName avatarName;
        public AvatarRenderer avatarRenderer;
        public AvatarMovementController avatarMovementController;
        [SerializeField] private AvatarOnPointerDown onPointerDown;
        private StringVariable currentPlayerInfoCardId;

        private string currentSerialization = "";
        public AvatarModel model = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        private MinimapMetadata.MinimapUserInfo avatarUserInfo = new MinimapMetadata.MinimapUserInfo();
        bool initializedPosition = false;

        private void Start()
        {
            OnPoolGet();
        }

        private void PlayerClicked()
        {
            currentPlayerInfoCardId.Set(model?.id);
        }

        public void OnDestroy()
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

            onPointerDown.Setup(scene, entity, new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                hoverText = "view profile"
            });

            DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= PrecisionAdjust;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust += PrecisionAdjust;

            entity.OnTransformChange -= avatarMovementController.OnTransformChanged;
            entity.OnTransformChange += avatarMovementController.OnTransformChanged;

            entity.OnTransformChange -= OnEntityTransformChanged;
            entity.OnTransformChange += OnEntityTransformChanged;

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            onPointerDown.OnPointerDownReport += PlayerClicked;

            // To deal with the cases in which the entity transform was configured before the AvatarShape
            if (!initializedPosition && entity.components.ContainsKey(DCL.Models.CLASS_ID_COMPONENT.TRANSFORM))
            {
                initializedPosition = true;

                avatarMovementController.MoveTo(
                    entity.gameObject.transform.localPosition - Vector3.up * DCLCharacterController.i.characterController.height / 2,
                    entity.gameObject.transform.localRotation, true);
            }

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = lastAvatarPosition != null ? lastAvatarPosition.Value : entity.gameObject.transform.position;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo);

            avatarName.SetName(model.name);

            everythingIsLoaded = true;

            onPointerDown.collider.enabled = true;
        }

        public void DisablePassport()
        {
            onPointerDown.collider.enabled = false;
        }

        public void EnablePassport()
        {
            onPointerDown.collider.enabled = true;
        }

        private void PrecisionAdjust(DCLCharacterPosition obj)
        {
            avatarUserInfo.worldPosition = entity.gameObject.transform.position;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo);
        }


        private void OnEntityTransformChanged(DCLTransform.Model updatedModel)
        {
            lastAvatarPosition = updatedModel.position;

            avatarUserInfo.userId = model.id;
            avatarUserInfo.userName = model.name;
            avatarUserInfo.worldPosition = updatedModel.position;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo);
            SceneController.i.physicsSyncController.MarkDirty();
        }

        public override void OnPoolGet()
        {
            base.OnPoolGet();

            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);

            everythingIsLoaded = false;
            initializedPosition = false;
            currentSerialization = "";
            model = new AvatarModel();
            lastAvatarPosition = null;
            avatarUserInfo = new MinimapMetadata.MinimapUserInfo();
            avatarName.SetName(String.Empty);
        }

        public override void Cleanup()
        {
            base.Cleanup();

            avatarRenderer.CleanupAvatar();

            if (poolableObject != null)
            {
                poolableObject.OnRelease -= Cleanup;
            }

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= PrecisionAdjust;

            if (entity != null)
            {
                entity.OnTransformChange = null;
                entity = null;
            }

            avatarUserInfo.userId = model.id;
            MinimapMetadataController.i?.UpdateMinimapUserInformation(avatarUserInfo, true);
        }
    }
}