#include <stdint.h>
#include "emscripten.h"
#include "kernelNativeBridge.h"

callback_vss cb_createEntity;
callback_vss cb_removeEntity;
callback_vs cb_sceneReady;

void SetCallbacks(
    callback_vss createEntity,
    callback_vss removeEntity,
    callback_vs sceneReady
    )
{
    cb_createEntity = createEntity;
    cb_removeEntity = removeEntity;
    cb_sceneReady = sceneReady;
}

void EMSCRIPTEN_KEEPALIVE call_SceneReady(char* sceneId) {
	cb_sceneReady(sceneId);
}

void EMSCRIPTEN_KEEPALIVE call_CreateEntity(char* sceneId, char* entityId) {
	cb_createEntity(sceneId, entityId);
}

void EMSCRIPTEN_KEEPALIVE call_RemoveEntity(char* sceneId, char* entityId) {
	cb_removeEntity(sceneId, entityId);
}