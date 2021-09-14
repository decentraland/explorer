import { createRealmRec, createNewUnsafeRec } from '@dcl/es5-context'

const whitelistES5: Array<keyof typeof global> = [
  'eval',
  'parseInt',
  'parseFloat',
  'isNaN',
  'isFinite',
  'decodeURI',
  'decodeURIComponent',
  'encodeURI',
  'encodeURIComponent',
  'escape',
  'unescape',
  'Object',
  'Function',
  'String',
  'Boolean',
  'Number',
  'Math',
  'Date',
  'RegExp',
  'Error',
  'EvalError',
  'RangeError',
  'ReferenceError',
  'SyntaxError',
  'TypeError',
  'URIError',
  'JSON',
  'Array',
  'Promise',
  'NaN',
  'Infinity'
]

export const defer: (fn: Function) => void = (Promise.resolve().then as any).bind(Promise.resolve() as any)

export async function customEval(code: string, env: any, isPreview: boolean) {
  // 1 if (!isPreview /* IN PROD WE RUN A FULL RUNTIME */) {
  // 1   let sandbox: any = {}
  // 1
  // 1   let resultKey = 'SAFE_EVAL_' + Math.floor(Math.random() * 1000000)
  // 1   sandbox[resultKey] = {}
  // 1
  // 1   const context = getES5Context(env)
  // 1
  // 1   Object.keys(context).forEach(function (key) {
  // 1     sandbox[key] = context[key]
  // 1   })
  // 1
  // 1   sandbox.window = sandbox
  // 1   sandbox.self = sandbox
  // 1
  // 1   return defer(() => new Function('code', `with (this) { ${code} }`).call(sandbox, code))
  // 1 } else {

  const rec = createRealmRec(createNewUnsafeRec(globalThis))

  rec.safeGlobal.globalThis = rec.safeGlobal
  rec.safeGlobal.global = rec.safeGlobal

  Object.assign(rec.safeGlobal, env)

  const newCode = code + ';\n//# sourceURL=game.js'

  rec.safeEval(newCode)
  // 1 }
}

export function getES5Context(base: Record<string, any>) {
  whitelistES5.forEach(($) => (base[$] = global[$]))

  return base
}
