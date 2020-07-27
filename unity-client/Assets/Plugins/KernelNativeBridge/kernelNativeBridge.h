typedef struct Color Color;
typedef struct MaterialModel MaterialModel;
typedef struct UpdateEntityComponent UpdateEntityComponent;
typedef struct SendSceneMessage SendSceneMessage;

struct Vector3 
{
	float x,y,z;
};

struct Color
{
    float r;
    float g;
    float b;
    float a; 
};

struct MaterialModel
{
    float alpha;
    Color albedoColor;
    Color emissiveColor;
    float metallic;
    float roughness;
    Color ambientColor;
    Color reflectionColor;
    Color reflectivityColor;
    float directIntensity;
    float microSurface;
    float emissiveIntensity;
    float environmentIntensity;
    float specularIntensity;
    int albedoTexture;
    int alphaTexture;
    int emissiveTexture;
    int bumpTexture;
    int refractionTexture;
    int disableLighting;
    float transparencyMode;
    int hasAlpha;
};

struct UpdateEntityComponent
{
    int classId;
    int name;
    MaterialModel model;
};

struct SendSceneMessage 
{
    int sceneId;
    int tag;
    int entityId;
    
    UpdateEntityComponent updateEntityComponent;
};


typedef void (*callback_v)();
typedef void (*callback_vi)(int32_t a);
typedef void (*callback_vf)(float a);
typedef void (*callback_vs)(const char *a);
typedef void (*callback_vss)(const char *a, const char *b);
typedef void (*callback_vv3)(struct Vector3);
typedef void (*callback_vx)(int32_t,int32_t,struct Vector3,struct Vector3);

typedef void (*callback_message)(const struct SendSceneMessage);

typedef int32_t (*callback_I)();
typedef float (*callback_F)();
typedef const char * (*callback_S)();