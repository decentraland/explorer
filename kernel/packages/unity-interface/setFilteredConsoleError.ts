let isOverridden = false

const ignoredErrors = ['Asset Bundle download is complete, but no data have been received\n']

export function setFilteredConsoleError() {
  if (isOverridden) return

  isOverridden = true
  const defaultConsoleError = window.console.error

  window.console.error = function () {
    if (!ignoredErrors.includes(arguments[0])) {
      defaultConsoleError.apply(console, arguments as any)
    }
  }
}
