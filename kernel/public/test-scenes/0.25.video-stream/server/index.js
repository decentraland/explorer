const http = require('https')
const fs = require('fs')
const express = require('express')

const app = express()
const cors = require('cors')

app.use(cors())

const target = './video.mp4'

app.get('/video.*', function(req, res) {
  try {
  const stat = fs.statSync(target)
  const fileSize = stat.size
  const range = req.headers.range
  if (range) {
    const parts = range.replace(/bytes=/, "").split("-")
    const start = parseInt(parts[0], 10)
    const end = parts[1]
      ? parseInt(parts[1], 10)
      : fileSize-1
    const chunksize = (end-start)+1
    const file = fs.createReadStream(target, {start, end})
    const head = {
      'Content-Range': `bytes ${start}-${end}/${fileSize}`,
      'Accept-Ranges': 'bytes',
      'Content-Length': chunksize,
      'Content-Type': 'video/mp4',
    }
    res.writeHead(206, head);
    file.pipe(res);
  } else {
    const head = {
      'Content-Length': fileSize,
      'Content-Type': 'video/mp4',
    }
    res.writeHead(200, head)
    fs.createReadStream(target).pipe(res)
  }

  } catch (e) {
    console.log(e)
  }
});
app.use('/', express.static('static'))

app.listen(4533, () => {
  console.log(`listening on http://localhost:4533/`)
})
