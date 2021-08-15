# General setup

NODE = node
COMPILER = $(NODE) --max-old-space-size=4096 node_modules/.bin/decentraland-compiler
BUILD_ECS = $(NODE) --max-old-space-size=4096 node_modules/.bin/build-ecs
CWD = $(shell pwd)

# Remove default Makefile rules

.SUFFIXES:

# SDK Files

SOURCE_SUPPORT_TS_FILES := $(wildcard scripts/*.ts)
COMPILED_SUPPORT_JS_FILES := $(subst .ts,.js,$(SOURCE_SUPPORT_TS_FILES))

SCENE_SYSTEM_SOURCES := $(wildcard static/systems/**/*.ts)
SCENE_SYSTEM := static/systems/scene.system.js
DECENTRALAND_LOADER := static/loader/lifecycle/worker.js
GIF_PROCESSOR := static/gif-processor/worker.js
INTERNAL_SCENES := static/systems/decentraland-ui.scene.js
VOICE_CHAT_CODEC_WORKER := static/voice-chat-codec/worker.js static/voice-chat-codec/audioWorkletProcessors.js


scripts/%.js: $(SOURCE_SUPPORT_TS_FILES) scripts/tsconfig.json
	@npx tsc --build scripts/tsconfig.json

static/loader/lifecycle/worker.js: packages/decentraland-loader/**/*.ts
	@$(COMPILER) targets/engine/loader.json

static/gif-processor/worker.js: packages/gif-processor/*.ts
	@$(COMPILER) targets/engine/gif-processor.json

static/voice-chat-codec/worker.js: packages/voice-chat-codec/*.ts
	@$(COMPILER) targets/engine/voice-chat-codec.json

static/default-profile/contents:
	@node ./static/default-profile/download_all.js

static/systems/scene.system.js: $(SCENE_SYSTEM_SOURCES) packages/scene-system/scene.system.ts
	@$(COMPILER) targets/engine/scene-system.json

static/systems/decentraland-ui.scene.js: $(SCENE_SYSTEM) packages/ui/tsconfig.json packages/ui/decentraland-ui.scene.ts
	@$(COMPILER) targets/engine/internal-scenes.json

empty-parcels:
	cd static/loader/empty-scenes && node generate_all.js

build-essentials: $(COMPILED_SUPPORT_JS_FILES) $(SCENE_SYSTEM) $(INTERNAL_SCENES) $(DECENTRALAND_LOADER) $(GIF_PROCESSOR) $(VOICE_CHAT_CODEC_WORKER) empty-parcels generate-mocks ## Build the basic required files for the explorer
# Hellmap scenes

HELLMAP_SOURCE_FILES := $(wildcard public/hell-map/*/game.ts)
HELLMAP_GAMEJS_FILES := $(subst .ts,.js,$(HELLMAP_SOURCE_FILES))

public/hell-map/%/game.js: $(SCENE_SYSTEM) public/hell-map/%/game.ts
	@$(COMPILER) targets/scenes/hell-map.json

# Test scenes

TEST_SCENES_SOURCE_FILES := $(wildcard public/test-scenes/*/game.ts)
TEST_SCENES_GAMEJS_FILES := $(subst .ts,.js,$(TEST_SCENES_SOURCE_FILES))

public/test-scenes/%/game.js: $(SCENE_SYSTEM) public/test-scenes/%/game.ts
	@$(COMPILER) targets/scenes/test-scenes.json

TEST_ECS_SCENE_SOURCES := $(wildcard public/ecs-scenes/*/game.ts)
TEST_ECS_SCENE_GAMEJS_FILES := $(subst .ts,.js,$(TEST_ECS_SCENE_SOURCES))

watch-only-test-scenes:
	@$(COMPILER) targets/scenes/test-scenes.json --watch

# ECS scenes

ECS_LIBRARY := public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.js

public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.js: public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.ts $(BUILD_ECS)
	$(BUILD_ECS) -p public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper

public/ecs-scenes/%/game.js: $(ECS_LIBRARY) $(SCENE_SYSTEM) public/ecs-scenes/%/game.ts
	@node scripts/buildECSprojects.js

# All scenes together

ecs-scenes: $(TEST_ECS_SCENE_GAMEJS_FILES)
	$(MAKE) generate-mocks

test-scenes: $(TEST_SCENES_GAMEJS_FILES) $(HELLMAP_GAMEJS_FILES) $(TEST_ECS_SCENE_GAMEJS_FILES) ## Build the test scenes
	$(MAKE) generate-mocks

# Entry points

static/%.js: build-essentials packages/entryPoints/%.ts
	@$(COMPILER) $(word 2,$^)

# Release

DIST_ENTRYPOINTS := static/editor.js static/index.js
DIST_STATIC_FILES := static/export.html static/preview.html static/default-profile/contents

build-deploy: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) $(SCENE_SYSTEM) $(INTERNAL_SCENES) ## Build all the entrypoints needed for a deployment

build-release: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) $(DIST_PACKAGE_JSON) ## Build all the entrypoints and run the `scripts/prepareDist` script
	@node ./scripts/prepareDist.js

# Testing

TEST_SOURCE_FILES := $(wildcard test/**/*.ts)

test/out/index.js: build-essentials $(TEST_SOURCE_FILES)
	@$(COMPILER) ./targets/test.json

test: build-essentials test-scenes test/out/index.js ## Run all the tests
	$(MAKE) generate-mocks
	@node scripts/runTestServer.js

test-docker: ## Run all the tests using a docker container
	@docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		-p 8080:8080 \
		circleci/node:10-browsers \
			make test

test-ci: # Run the tests (for use in the continuous integration environment)
	@SINGLE_RUN=true NODE_ENV=production $(MAKE) test
	@node_modules/.bin/nyc report --temp-directory ./test/tmp --reporter=html --reporter=lcov --reporter=text

generate-images: ## Generate the screenshots to run the visual diff validation tests
	@docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		-e GENERATE_NEW_IMAGES=true \
		circleci/node:10-browsers \
			make test

public/local-ipfs/mappings:
	@rm -rf ./public/local-ipfs
	@node ./scripts/createMockJson.js

generate-mocks: ./scripts/createMockJson.js public/local-ipfs/mappings ## Build a fake "IPFS" index of all the test scene mappings
	@rm -rf ./public/local-ipfs
	@node ./scripts/createMockJson.js

PARCEL_SCENE_JSONS := $(wildcard public/test-scenes/*/scene.json)
ECS_SCENE_JSONS := $(wildcard public/ecs-scenes/*/scene.json)
HELLMAP_SCENE_JSONS := $(wildcard public/hell-map/*/scene.json)
SCENE_JSONS := $(PARCEL_SCENE_JSONS) $(ECS_SCENE_JSONS) $(HELLMAP_SCENE_JSONS)

# CLI

npm-link: build-essentials static/editor.js ## Run `npm link` to develop local scenes against this project
	cd static; npm link

watch-builder: build-essentials static/editor.js ## Watch the files required for hacking with the builder
	@node_modules/.bin/concurrently \
		-n "scene-system,internal-scenes,loader,builder,server" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/entryPoints/editor.json --watch" \
			"node ./scripts/runTestServer.js --keep-open"

watch-cli: build-essentials ## Watch the files required for building the CLI
	@node_modules/.bin/concurrently \
		-n "scene-system,internal-scenes,loader,kernel,server" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/entryPoints/index.json --watch" \
			"node ./scripts/runTestServer.js --keep-open"

# Aesthetics

lint: ## Validate correct formatting and circular dependencies
	@node_modules/.bin/madge packages/entryPoints/index.ts --circular --warning
	@node_modules/.bin/madge packages --orphans --extensions ts --exclude '.+\.d.ts|.+/dist\/.+'
	@node_modules/.bin/tslint --project tsconfig.json

lint-fix: ## Fix bad formatting on all .ts and .tsx files
	@node_modules/.bin/tslint --project tsconfig.json --fix
	@node_modules/.bin/prettier --write 'packages/**/*.{ts,tsx}'

# Development

watch: $(SOME_MAPPINGS) build-essentials static/index.js ## Watch the files required for hacking the explorer
	@NODE_ENV=development npx concurrently \
		-n "scene-system,internal-scenes,loader,basic-scenes,kernel,test,simulator,server" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/scenes/basic-scenes.json --watch" \
			"$(COMPILER) targets/entryPoints/index.json --watch" \
			"$(COMPILER) targets/test.json --watch" \
			"node ./scripts/runPathSimulator.js" \
			"node ./scripts/runTestServer.js --keep-open"

fetchSceneContents: scripts/fetchSceneContents.js
	@node ./scripts/fetchSceneContents.js

clean: ## Clean all generated files
	@$(COMPILER) targets/clean.json

update-renderer:  ## Update the renderer
	npm install @dcl/unity-renderer@latest

# Makefile

.PHONY: help docs clean watch watch-builder watch-cli lint lint-fix generate-images test-ci test-docker update build-essentials build-deploy build-release update-renderer
.DEFAULT_GOAL := help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo "\nYou probably want to run 'make watch' or 'make test-scenes watch' to build all the test scenes and run the local comms server."
