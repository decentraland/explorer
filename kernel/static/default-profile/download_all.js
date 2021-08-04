const fs = require('fs')
const path = require('path')
const fetch = require('node-fetch')

const contentServerUrl = 'https://content.decentraland.org/contents/'
const contentsDir = 'contents'

const downloadFile = async (url, path) => {
  const res = await fetch(url)
  const fileStream = fs.createWriteStream(path)
  await new Promise((resolve, reject) => {
    res.body.pipe(fileStream)
    res.body.on('error', (err) => {
      reject(err)
    })
    fileStream.on('finish', function () {
      resolve()
    })
  })
}

const catalog = require('./basecatalog.json')

let contentPath
try {
  contentPath = path.join(__dirname, contentsDir)
  fs.mkdirSync(contentPath)
} catch (e) {}

const hashes = new Set()

for (let wearable of catalog) {
  hashes.add(wearable.thumbnail)
  for (let representation of wearable.data.representations) {
    for (let contentItem of representation.contents) {
      hashes.add(contentItem.hash)
    }
  }
}

async function main() {
  for (let url of hashes) {
    const finalPath = path.join(contentPath, url)
    if (!fs.existsSync(finalPath)) {
      console.log(`Downloading ${finalPath}`)
      await downloadFile(contentServerUrl + url, finalPath)
    }
  }
}

main().catch((e) => {
  console.error(e)
  process.exit(1)
})
