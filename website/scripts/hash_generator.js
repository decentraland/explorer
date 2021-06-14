const fs = require("fs")
const path = require("path")
const md5File = require("md5-file")
const childProcess = require("child_process")

let ENV_CONTENT = []
if (fs.existsSync(".env")) {
  ENV_CONTENT = fs
    .readFileSync(".env")
    .toString()
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line.trim() !== "")
}

const VARS = ["REACT_APP_EXPLORER_VERSION", "REACT_APP_WEBSITE_VERSION"]

ENV_CONTENT = ENV_CONTENT.filter((l) => {
  const [name] = l.split("=")
  return !VARS.includes(name) && l.trim() !== ""
})

const commitVersion = childProcess.execSync("git rev-parse HEAD").toString().trim()

const websiteVersion = md5File.sync(path.resolve("./public/website.js"))

ENV_CONTENT.push("REACT_APP_EXPLORER_VERSION=" + commitVersion)
ENV_CONTENT.push("REACT_APP_WEBSITE_VERSION=" + websiteVersion)
ENV_CONTENT.push("WEBSITE_COMMIT_HASH=" + commitVersion)
ENV_CONTENT.push("GENERATE_SOURCEMAP=true")

console.log("VERSIONS:", ENV_CONTENT, "\n")

fs.writeFileSync(".env", ENV_CONTENT.join("\n") + "\n")
