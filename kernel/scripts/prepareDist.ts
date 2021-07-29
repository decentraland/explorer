#!/usr/bin/env node
// tslint:disable:no-console

import path = require('path')
import { readFileSync, writeFileSync } from 'fs-extra'

const root = path.resolve(__dirname, '..')

async function injectDependencies(folder: string, dependencies: string[], devDependency = false) {
  console.log(`> update ${folder}/package.json (injecting dependencies)`)
  {
    const file = path.resolve(root, `${folder}/package.json`)
    const packageJson = JSON.parse(readFileSync(file).toString())
    const localPackageJson = JSON.parse(readFileSync(path.resolve(root, `package.json`)).toString())

    const deps = new Set(dependencies)

    const target = devDependency ? 'devDependencies' : 'dependencies'

    packageJson[target] = packageJson[target] || {}

    deps.forEach((dep) => {
      if (localPackageJson.dependencies[dep]) {
        packageJson[target][dep] = localPackageJson.dependencies[dep]
        deps.delete(dep)
        console.log(`  using dependency: ${dep}@${packageJson[target][dep]}`)
      }
    })

    deps.forEach((dep) => {
      if (localPackageJson.devDependencies[dep]) {
        packageJson[target][dep] = localPackageJson.devDependencies[dep]
        deps.delete(dep)
        console.log(`  using devDependency: ${dep}@${packageJson[target][dep]}`)
      }
    })

    if (deps.size) {
      throw new Error(`Missing dependencies "${Array.from(deps).join('", "')}"`)
    }

    writeFileSync(file, JSON.stringify(packageJson, null, 2))
  }
}

// tslint:disable-next-line:semicolon
;(async function () {
  // Update versions in package.json
  await injectDependencies('static', ['@dcl/unity-renderer'], true)
})().catch((e) => {
  // tslint:disable-next-line:no-console
  console.error(e)
  process.exit(1)
})
