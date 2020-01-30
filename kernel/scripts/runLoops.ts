const puppeteer = require('puppeteer')

async function main() {
  const browser = await puppeteer.launch()
  const page = await browser.newPage()

  page.on('console', msg => {
    for (let i = 0; i < msg.args().length; ++i) {
      console.info(`${i}: ${msg.args()[i]}`)
    }
  })

  await page.goto('http://localhost:8080/?position=-5%2C-5&ws=ws%3A%2F%2Flocalhost%3A5001%2Floop&COMMS=v2-server', {
    waitUntil: 'networkidle2'
  })

  let i = 0
  while (true) {
    // await page.screenshot({ path: `example-${i++}.png` })

    await page.waitFor(30 * 1000)
  }
}

main().catch(console.error)
