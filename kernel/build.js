#!/usr/bin/env node
const { build } = require(`estrella`)

const watch = process.argv.includes(`--watch`)
const debug = !process.env.CI
const sourcemap = debug

const common = { bundle: true, sourcemap, watch, debug }

build({
  ...common,
  entry: `./packages/gif-processor/worker.ts`,
  outfile: `./static/gif-processor/worker.js`,
  tsconfig: `./packages/gif-processor/tsconfig.json`
})
gg
;['cli.scene.system', 'stateful.scene.system', 'scene.system'].map(buildSystem)

// build({
//   ...common,
//   entry: `./packages/decentraland-loader/lifecycle/worker.ts`,
//   outfile: `./static/loader/lifecycle/worker.js`,
//   tsconfig: `./packages/decentraland-loader/tsconfig.json`
// })

function buildSystem(file) {
  build({
    ...common,
    entry: `./packages/scene-system/${file}.ts`,
    outfile: `./static/systems/${file}.js`,
    tsconfig: `./packages/scene-system/tsconfig.json`
  })
}
