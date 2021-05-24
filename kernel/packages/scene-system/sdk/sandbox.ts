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
  // if (!isPreview /* IN PROD WE RUN A FULL RUNTIME */) {
  //   let sandbox: any = {}

  //   let resultKey = 'SAFE_EVAL_' + Math.floor(Math.random() * 1000000)
  //   sandbox[resultKey] = {}

  //   const context = getES5Context(env)

  //   Object.keys(context).forEach(function (key) {
  //     sandbox[key] = context[key]
  //   })

  //   sandbox.window = sandbox
  //   sandbox.self = sandbox

  //   return defer(() => new Function('code', `with (this) { ${code} }`).call(sandbox, code))
  // } else {

  const rec = createRealmRec(createNewUnsafeRec(globalThis))

  rec.safeGlobal.globalThis = rec.safeGlobal
  rec.safeGlobal.global = rec.safeGlobal

  Object.assign(rec.safeGlobal, env)

  const newCode = '(eval)(' + JSON.stringify(code + ';\n// @dcl/es5-context\n//# sourceURL=game.js') + ')'

  rec.safeEval(newCode)
  // }
}

export function getES5Context(base: Record<string, any>) {
  whitelistES5.forEach(($) => (base[$] = global[$]))

  return base
}
