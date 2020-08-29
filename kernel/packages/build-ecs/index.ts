#!/usr/bin/env node

// tslint:disable:no-console
import * as ts from 'typescript'
import * as terser from 'terser'
import { inspect } from 'util'
import { resolve, dirname, relative } from 'path'

type JsonManifest = {
  // scene.json & package.json
  main: string
  bundleDependencies: string[]
  decentralandLibrary?: any
}

type DecentralandLib = {
  typings?: string
  main: string
}

type ProjectConfig = ts.ParsedCommandLine & { libs: DecentralandLib[]; isDecentralandLib: boolean }

// nameCache for the minifier
const nameCache = {}

const WATCH = process.argv.indexOf('--watch') !== -1 || process.argv.indexOf('-w') !== -1

const watchedFiles = new Set<string>()

type FileMap = ts.MapLike<{ version: number }>

async function compile(watch: boolean) {
  // current working directory
  let CWD = process.cwd()
  ts.sys.getCurrentDirectory = () => CWD

  {
    // Read the target folder, if specified.
    // -p --project, like typescript
    const projectArgIndex = Math.max(process.argv.indexOf('-p'), process.argv.indexOf('--project'))
    if (projectArgIndex != -1 && process.argv.length > projectArgIndex) {
      const folder = resolve(process.cwd(), process.argv[projectArgIndex + 1])
      if (ts.sys.directoryExists(folder)) {
        CWD = folder
      } else {
        throw new Error(`Folder ${folder} does not exist!.`)
      }
    }
  }

  console.log(`> Working directory: ${ts.sys.getCurrentDirectory()}`)

  let shouldEmitLib = false
  let packageJson: JsonManifest | null = null

  if (resolveFile('package.json')) {
    packageJson = JSON.parse(loadArtifact('package.json'))
    packageJson!.bundleDependencies = packageJson!.bundleDependencies || []
    shouldEmitLib = 'decentralandLibrary' in packageJson!
  }

  const cfg = getConfiguration(packageJson)

  cfg.isDecentralandLib = shouldEmitLib

  console.log('')

  if (cfg.fileNames.length === 0) {
    console.error('! Error: There are no matching .ts files to process')
    process.exit(4)
  }

  const files: FileMap = {}

  // initialize the list of files
  cfg.fileNames.forEach((fileName) => {
    files[fileName] = { version: 0 }
  })

  // Create the language service host to allow the LS to communicate with the host
  const services = ts.createLanguageService(
    {
      getScriptFileNames: () => cfg.fileNames,
      getScriptVersion: (fileName) => files[fileName] && files[fileName].version.toString(),
      getScriptSnapshot: (fileName) => {
        if (!ts.sys.fileExists(fileName)) {
          return undefined
        }

        if (watch) {
          watchFile(fileName, services, files, cfg)
        }

        return ts.ScriptSnapshot.fromString(ts.sys.readFile(fileName)!.toString())
      },
      getCurrentDirectory: ts.sys.getCurrentDirectory,
      getCompilationSettings: () => cfg.options,
      getDefaultLibFileName: (options) => ts.getDefaultLibFilePath(options),
      fileExists: ts.sys.fileExists,
      readFile: ts.sys.readFile,
      readDirectory: ts.sys.readDirectory
    },
    ts.createDocumentRegistry()
  )

  if (watch) {
    // Now let's watch the files
    cfg.fileNames.forEach((fileName) => {
      watchFile(fileName, services, files, cfg)
    })
  }

  // First time around, emit all files
  await emitFile(cfg.fileNames[0], services, cfg)
}

function watchFile(fileName: string, services: ts.LanguageService, files: FileMap, cfg: ProjectConfig) {
  if (!watchedFiles.has(fileName)) {
    watchedFiles.add(fileName)

    files[fileName] = { version: 0 }

    // Add a watch on the file to handle next change
    ts.sys.watchFile!(
      fileName,
      (fileName, type) => {
        // Update the version to signal a change in the file
        files[fileName].version++

        // write the changes to disk
        emitFile(fileName, services, cfg)
      },
      250
    )
  }
}

async function minify(files: string | string[] | { [file: string]: string }) {
  const result = (await terser.minify(files, {
    ecma: 5,
    nameCache,
    mangle: {
      toplevel: false,
      module: false,
      eval: true,
      keep_classnames: true,
      keep_fnames: true,
      reserved: ['global', 'define']
    },
    compress: {
      passes: 2
    },
    output: {
      ecma: 5,
      comments: /^!/,
      beautify: false
    },
    sourceMap: false,
    toplevel: false
  } as any)) as any

  if (result.warnings) {
    for (let warning of result.warnings) {
      console.warn('!   Warning: ' + warning)
    }
  }

  if (result.error) {
    console.warn('!   Error: ' + result.error)
  }

  return result
}

async function emitFile(fileName: string, services: ts.LanguageService, cfg: ProjectConfig) {
  let output = services.getEmitOutput(fileName)

  if (!output.emitSkipped) {
    console.log(`> Processing ${fileName.replace(ts.sys.getCurrentDirectory(), '')}`)
  } else {
    console.log(`> Processing ${fileName.replace(ts.sys.getCurrentDirectory(), '')} failed`)
  }

  logErrors(services)

  type OutFile = {
    readonly path: string
    definition?: {
      path: string
      content: string
    }
    content?: string
  }

  const loadedLibs: OutFile[] = []

  function loadUllaLib(lib: string) {
    const path = resolveFile(lib)

    if (path) {
      const json: OutFile[] = JSON.parse(loadArtifact(lib))
      loadedLibs.push(...json)
      return true
    }

    return false
  }

  function loadJsLib(lib: string) {
    const path = resolveFile(lib)

    if (path) {
      loadedLibs.push({
        path: relative(ts.sys.getCurrentDirectory(), path),
        content: loadArtifact(lib)
      })
      return true
    }

    return false
  }

  function loadLibOrJs(lib: string) {
    return loadUllaLib(lib + '.lib') || loadJsLib(lib) || false
  }

  cfg.libs.forEach((lib) => {
    if (!loadLibOrJs(lib.main)) {
      console.error(`! Error: could not load lib: ${lib.main}`)
    }
  })

  const out = new Map<string, OutFile>()

  function getOutFile(path: string) {
    let f = out.get(path)
    if (!f) {
      f = { path }
      out.set(path, f)
    }
    return f
  }

  function normalizePath(path: string) {
    return path.replace(ts.sys.getCurrentDirectory(), '')
  }

  for (let o of output.outputFiles) {
    if (o.name.endsWith('.d.ts')) {
      const filePath = o.name.replace(/\.d\.ts$/, '.js')
      const f = getOutFile(filePath)

      f.definition = {
        content: o.text,
        path: o.name
      }
    } else {
      const f = getOutFile(o.name)
      f.content = o.text
    }
  }

  for (let [, file] of out) {
    if (file.path.endsWith('.js') && file.content && !file.path.endsWith('.min.js')) {
      loadedLibs.push({
        path: relative(ts.sys.getCurrentDirectory(), fileName),
        content: file.content
      })

      const ret: string[] = []

      for (let { path, content } of loadedLibs) {
        const code = content + '\n//# sourceURL=ulla-build:///' + path

        ret.push(`/* ${JSON.stringify(path)} */ eval(${JSON.stringify(code)})`)
      }

      file.content = ret.join('\n')

      if (cfg.isDecentralandLib) {
        // deps
        const deps = getOutFile(file.path + '.lib')
        deps.content = JSON.stringify(loadedLibs, null, 2)
      }

      {
        // minify && map
        const minifiedFile = getOutFile(file.path.replace(/\.js$/, '.min.js'))
        console.log(`> preparing ${normalizePath(minifiedFile.path)}`)

        const m = await minify(loadedLibs.map(($) => $.content).join(';\n'))
        minifiedFile.content = m.code

        if (m.map) {
          const f = getOutFile(file.path.replace(/\.js$/, '.js.map'))
          f.content = typeof m.map === 'string' ? m.map : JSON.stringify(m.map)
        }
      }
    }
  }

  for (let [, file] of out) {
    ensureDirectoriesExist(dirname(file.path))
    if (file.content) {
      console.log(`> writing ${normalizePath(file.path)}`)
      ts.sys.writeFile(file.path, file.content)
    }
    if (file.definition) {
      console.log(`> writing ${normalizePath(file.definition.path)}`)
      ts.sys.writeFile(file.definition.path, file.definition.content)
    }
  }

  if (WATCH) {
    console.log('\nThe compiler is watching file changes...\n')
  }
}

function logErrors(services: ts.LanguageService) {
  let allDiagnostics = services
    .getCompilerOptionsDiagnostics()
    .concat(services.getProgram()!.getGlobalDiagnostics())
    .concat(services.getProgram()!.getSemanticDiagnostics())
    .concat(services.getProgram()!.getSyntacticDiagnostics())

  allDiagnostics.forEach(printDiagnostic)
}

function getConfiguration(packageJson: JsonManifest | null): ProjectConfig {
  const host: ts.ParseConfigHost = {
    useCaseSensitiveFileNames: ts.sys.useCaseSensitiveFileNames,
    fileExists: ts.sys.fileExists,
    readFile: ts.sys.readFile,
    readDirectory: ts.sys.readDirectory
  }

  const tsconfigPath = ts.sys.resolvePath(resolve(ts.sys.getCurrentDirectory(), 'tsconfig.json'))
  const tsconfigContent = ts.sys.readFile(tsconfigPath)

  if (!tsconfigContent) {
    console.error(`! Error: missing tsconfig.json file`)
    process.exit(1)
  }

  const parsed = ts.parseConfigFileTextToJson('tsconfig.json', tsconfigContent)

  if (parsed.error) {
    printDiagnostic(parsed.error)
    process.exit(1)
  }

  const tsconfig = ts.parseJsonConfigFileContent(parsed.config, host, ts.sys.getCurrentDirectory(), {}, 'tsconfig.json')

  let hasError = false

  // should this project be compiled as a lib? or as a scene?
  let isDecentralandLib = false

  if (tsconfig.options.target !== ts.ScriptTarget.ES5) {
    console.error('! Error: tsconfig.json: Decentraland only allows ES5 targets')
    hasError = true
  }

  if (tsconfig.options.module !== ts.ModuleKind.AMD) {
    console.error('! Error: tsconfig.json: Decentraland only allows AMD modules')
    hasError = true
  }

  if (!tsconfig.options.outFile) {
    console.error('! Error: tsconfig.json: invalid outFile')
    hasError = true
  }

  if (tsconfig.options.outFile) {
    const outFile = ts.sys.resolvePath(tsconfig.options.outFile)

    if (!outFile) {
      console.error(`! Error: field "outFile" in tsconfig.json cannot be resolved.`)
      hasError = true
    }

    if (packageJson) {
      if (!packageJson.main) {
        console.error(`! Error: field "main" in package.json is missing.`)
        hasError = true
      } else {
        const mainFile = ts.sys.resolvePath(packageJson.main)
        if (!mainFile) {
          console.error(`! Error: field "main" in scene.json cannot be resolved.`)
          hasError = true
        }

        if (outFile !== mainFile) {
          console.error(
            `! Error: tsconfig.json .outFile is not equal to scene.json .main\n       (${outFile.replace(
              ts.sys.getCurrentDirectory(),
              ''
            )} != ${mainFile.replace(ts.sys.getCurrentDirectory(), '')})`
          )
          hasError = true
        }
      }
    } else {
      console.log('! Warning: missing package.json in project folder')
    }
  }

  const libs: DecentralandLib[] = []
  const bundledLibs: string[] = []

  if (packageJson) {
    if (packageJson.decentralandLibrary) {
      isDecentralandLib = true
    } else {
      isDecentralandLib = false

      if (packageJson.bundleDependencies instanceof Array) {
        packageJson.bundleDependencies.forEach(($, ix) => {
          if (typeof $ == 'string') {
            bundledLibs.push($)
          } else {
            console.error(
              `! Error: package.json .bundleDependencies must be an array of strings. The element number bundleDependencies[${ix}] is not a string.`
            )
            hasError = true
          }
        })
      } else if (packageJson.bundleDependencies) {
        console.error(`! Error: package.json .bundleDependencies must be an array of strings.`)
        hasError = true
      }
    }
  }

  if (!isDecentralandLib && process.env.NO_DEFAULT_LIBS !== '1') {
    // most of the decentraland scenes require the following libraries.
    // (order matters, do not change the order)
    libs.unshift({ main: process.env.AMD_PATH || 'decentraland-ecs/artifacts/amd.js' }) // 2nd place
    libs.unshift({ main: process.env.ECS_PATH || 'decentraland-ecs/dist/src/index.js' }) // 1st place
  }

  bundledLibs.forEach((libName) => {
    let resolved: string | null = null

    try {
      resolved = require.resolve(libName + '/package.json', { paths: [ts.sys.getCurrentDirectory()] })
    } catch (e) {
      console.error(`! Error: dependency ${libName} not found (is it installed?)`)
      hasError = true
    }

    if (resolved) {
      try {
        const libPackageJson = JSON.parse(ts.sys.readFile(resolved)!)

        let main: string | null = null
        let typings: string | null = null

        if (!libPackageJson.main) {
          throw new Error(`field "main" is missing in package.json`)
        } else {
          main = resolve(dirname(resolved), libPackageJson.main)
          if (!ts.sys.fileExists(main)) {
            throw new Error(`main file ${main} not found`)
          }
        }

        if (!libPackageJson.typings) {
          throw new Error(`field "typings" is missing in package.json`)
        } else {
          typings = resolve(dirname(resolved), libPackageJson.typings)
          if (!ts.sys.fileExists(typings)) {
            throw new Error(`typings file ${typings} not found`)
          }
        }

        if (!libPackageJson.decentralandLibrary) {
          throw new Error(`field "decentralandLibrary" is missing in package.json`)
        }

        libs.push({ main, typings })
      } catch (e) {
        console.error(`! Error in library ${libName}: ${e.message}`)
        hasError = true
      }
    }
  })

  if (libs.length && isDecentralandLib) {
    console.log(
      '! Error: this project of type decentralandLibrary includes bundleDependencies. bundleDependencies are only allowed in scenes.'
    )
    process.exit(1)
  }

  if (hasError) {
    console.log('tsconfig.json:')
    console.log(inspect(tsconfig, false, 10, true))
    process.exit(1)
  }

  // the new code generation as libraries enables us to leverage source maps
  // source map config is overwritten for that reason.
  tsconfig.options.inlineSourceMap = true
  tsconfig.options.inlineSources = true
  tsconfig.options.sourceMap = false
  tsconfig.options.removeComments = false
  tsconfig.options.sourceRoot = ts.sys.getCurrentDirectory()

  function ensurePathsAsterisk(options: any) {
    options.paths = options.paths || {}
    options.baseUrl = options.baseUrl || '.'
    if (!options.paths.hasOwnProperty('*')) {
      options.paths['*'] = []
    }
    return options.paths['*']
  }

  if (libs.length > 2) {
    const asterisk = ensurePathsAsterisk(tsconfig.options)
    const rawAsterisk = ensurePathsAsterisk(tsconfig.raw!.compilerOptions)

    let shouldRewriteTsconfig = false

    libs.forEach((lib) => {
      if (lib.typings) {
        const relativePath = relative(dirname(tsconfigPath), lib.typings)
        // check if it is in the processed configuration
        if (!asterisk.includes(relativePath)) {
          asterisk.push(relativePath)
        }
        // check if it is in the raw configuration (tsconfig.json contents)
        if (!rawAsterisk.includes(relativePath)) {
          console.warn(`! Warning: ${relativePath} is missing in tsconfig.json paths`)
          rawAsterisk.push(relativePath)
          shouldRewriteTsconfig = true
        }
      }
    })

    // if we had to add a dependency resolution path, we will have to rewrite the tsconfig.json
    if (shouldRewriteTsconfig) {
      ts.sys.writeFile(tsconfigPath, JSON.stringify(tsconfig.raw, null, 2))
    }
  }

  return Object.assign(tsconfig, { libs, isDecentralandLib })
}

function printDiagnostic(diagnostic: ts.Diagnostic) {
  let message = ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n')
  if (diagnostic.file) {
    let { line, character } = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start!)
    console.log(
      `  Error ${diagnostic.file.fileName.replace(ts.sys.getCurrentDirectory(), '')} (${line + 1},${
        character + 1
      }): ${message}`
    )
  } else {
    console.log(`  Error: ${message}`)
  }
}

function loadArtifact(path: string): string {
  try {
    const resolved = resolveFile(path)
    if (resolved) {
      return ts.sys.readFile(resolved)!
    }

    throw new Error()
  } catch (e) {
    console.error(`! Error: ${path} not found. ` + e)
    process.exit(2)
  }
}

function resolveFile(path: string): string | null {
  if (ts.sys.fileExists(path)) {
    return path
  }

  let ecsPackageAMD = resolve(ts.sys.getCurrentDirectory(), path)

  if (ts.sys.fileExists(ecsPackageAMD)) {
    return ecsPackageAMD
  }

  ecsPackageAMD = 'node_modules/' + path

  if (ts.sys.fileExists(ecsPackageAMD)) {
    return ecsPackageAMD
  }

  ecsPackageAMD = '../node_modules/' + path

  if (ts.sys.fileExists(ecsPackageAMD)) {
    return ecsPackageAMD
  }

  ecsPackageAMD = '../../node_modules/' + path

  if (ts.sys.fileExists(ecsPackageAMD)) {
    return ecsPackageAMD
  }

  return null
}

function ensureDirectoriesExist(folder: string) {
  if (!ts.sys.directoryExists(folder)) {
    // resolve takes a relative path, so we won't pass CWD to it.
    ensureDirectoriesExist(resolve(folder, '..'))
    ts.sys.createDirectory(folder)
  }
}

async function testScript(script: string) {
  // TODO: perform vm2 validation of code (future PR)
}

// Start the watcher
compile(WATCH).catch((e) => {
  console.error(e)
  process.exit(1)
})
