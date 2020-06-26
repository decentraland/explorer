import { engine, Entity, Transform, AvatarShape, Vector3, Vector2, Color4, executeTask, log, BoxShape, Material, OnPointerDown, ActionButton } from 'decentraland-ecs/src'

const PREVIEW_DEBUGGING = false;

const sceneCoords = new Vector2(-149, -144);
const parcelSize = 16;

const blockConfig = {
  offset: new Vector3(0, 1, 0),
  scale: new Vector3(1, 2, 1),
  color: new Color4(0.69, 0.85, 0.96, 0.2),
};

const avatarConfig = {
  offset: new Vector3(0, 0.01, 0),
  nameTag: "Mannequin",
};

let wearablesCatalog: WearableCatalog;

export class Mannequin {
  private mainEntity: Entity | any = null;
  private mainTransform: Transform | any = null;

  private avatar: MannequinAvatar | any = null;
  private avatarTransform: Transform | any = null;

  private position: Vector3 = Vector3.Zero();
  private userId: string;

  constructor(userId: string) {
    this.userId = userId;
    // const wearablesUrl = `https://dcl-wearables-dev.now.sh/expected.json`;
    const wearablesUrl = `https://wearable-api.decentraland.org/v2/collections/`;

    executeTask(async () => {
      try {
        let response = await fetch(wearablesUrl);
        let json = await response.json();

        wearablesCatalog = new WearableCatalog(json);

        this.setupMainEntity();
        this.setupAvatar();
        this.setPosition(this.position);
        this.fetchProfile();
      } catch {
        log("mannequin: an error occurred while reaching for wearables data");
      }
    });
  }

  public setPosition(position: Vector3) {
    this.position = position;
    if (this.mainTransform) {
      this.mainTransform.position = position.add(blockConfig.offset);
    }
    if (this.avatarTransform) {
      const sceneWorldPosition = sceneCoords.multiplyByFloats(
        parcelSize,
        parcelSize
      );
      this.avatarTransform.position = new Vector3(
        sceneWorldPosition.x,
        0,
        sceneWorldPosition.y
      ).add(position.add(avatarConfig.offset));
    }
  }

  public addWearable(wearableId: string) {
    this.avatar.addWearable(wearableId);
  }

  private fetchProfile() {
    const url =
      "https://peer.decentraland.org/content/entities/profiles?pointer=" +
      this.userId;

    executeTask(async () => {
      try {
        let response = await fetch(url);
        let json = await response.json();
        this.avatar.setFromProfileData(json);
      } catch {
        log("mannequin: an error occurred while reaching for player data");
      }
    });
  }

  //It's just a box to block clicking over avatar
  private setupMainEntity() {
    this.mainEntity = new Entity();
    this.mainTransform = new Transform({ scale: blockConfig.scale });
    this.mainEntity.addComponent(new BoxShape());
    const material = new Material();
    material.albedoColor = blockConfig.color;
    this.mainEntity.addComponent(material);
    this.mainEntity.addComponent(this.mainTransform);

    this.mainEntity.addComponent(
      new OnPointerDown(
        () => {
          this.fetchProfile();
        },
        {
          button: ActionButton.PRIMARY,
          hoverText: "Reset",
        }
      )
    );

    engine.addEntity(this.mainEntity);
  }

  private setupAvatar() {
    this.avatar = new MannequinAvatar();
    this.avatarTransform = new Transform({ position: avatarConfig.offset });
    this.avatar.addComponent(this.avatarTransform);

    engine.addEntity(this.avatar);
  }
}

class MannequinAvatar extends Entity {
  private avatarShape: AvatarShape;
  private avatarWearables: AvatarWearables;

  constructor() {
    super();
    this.avatarShape = new AvatarShape();
    this.avatarWearables = new AvatarWearables();
    this.setDefaultAvatar();
    this.addComponent(this.avatarShape);
  }

  setDefaultAvatar() {
    this.avatarShape.name = avatarConfig.nameTag;
    this.avatarShape.bodyShape = "dcl://base-avatars/BaseFemale";
    this.avatarShape.wearables = this.avatarWearables.getWearables();
    this.avatarShape.skinColor = new Color4(0.8671875, 0.6953125, 0.5625, 1);
  }

  setFromProfileData(profile: any) {
    const profileData = profile[0].metadata.avatars[0];
    const avatarData = profileData.avatar;

    if (!PREVIEW_DEBUGGING) {
      this.avatarWearables.setWearables(avatarData.wearables);
      this.avatarShape.bodyShape = avatarData.bodyShape;
    }

    this.avatarShape.wearables = this.avatarWearables.getWearables();
    this.avatarShape.name = profileData.name + " - " + avatarConfig.nameTag;
    this.avatarShape.eyeColor = avatarData.eyes.color.color;
    this.avatarShape.hairColor = avatarData.hair.color.color;
    this.avatarShape.skinColor = avatarData.skin.color.color;
  }

  addWearable(wearableId: string) {
    this.avatarWearables.addWearable(wearableId);
    this.avatarShape.wearables = this.avatarWearables.getWearables();
  }
}

type WearableData = { category: string };

class WearableCatalog {
  private catalog: any = {};

  constructor(catalogResponse: any) {
    for (let index in catalogResponse) {
      const w = catalogResponse[index] as any;
      this.catalog[w.id] = { category: w.category };
    }
  }

  getWearable(wearableId: string): WearableData {
    return this.catalog[wearableId];
  }
}

class AvatarWearables {
  private wearables: any = {};

  constructor() {
    this.addWearable("dcl://base-avatars/f_sweater");
    this.addWearable("dcl://base-avatars/f_jeans");
    this.addWearable("dcl://base-avatars/bun_shoes");
    this.addWearable("dcl://base-avatars/standard_hair");
    this.addWearable("dcl://base-avatars/f_eyes_00");
    this.addWearable("dcl://base-avatars/f_eyebrows_00");
    this.addWearable("dcl://base-avatars/f_mouth_00");
  }

  addWearable(wearableId: string) {
    const wearable = wearablesCatalog.getWearable(wearableId);
    if (wearable !== undefined) {
      this.wearables[wearable.category] = wearableId;
    }
  }

  setWearables(wearableIds: string[]) {
    this.wearables = {};
    for (let index in wearableIds) {
      let wearableId = wearableIds[index];
      this.wearables[
        wearablesCatalog.getWearable(wearableId).category
      ] = wearableId;
    }
  }

  getWearables(): string[] {
    let ret = [];
    for (let category in this.wearables) {
      ret.push(this.wearables[category]);
    }
    return ret;
  }
}
