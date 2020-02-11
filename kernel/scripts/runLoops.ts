const puppeteer = require('puppeteer')

const position = process.env.BOT_POSITION ? process.env.BOT_POSITION : '20,20'
const dclEnv = process.env.DCL_ENV ? process.env.DCL_ENV : 'zone'

async function main() {
  const browser = await puppeteer.launch()
  const page = await browser.newPage()

  page.on('console', msg => {
    for (let i = 0; i < msg.args().length; ++i) {
      console.info(`${i}: ${msg.args()[i]}`)
    }
  })

  await page.goto(`http://localhost:8080/?ENV=${dclEnv}&position=${position}&ws=ws%3A%2F%2Flocalhost%3A5001%2Floop`, {
    waitUntil: 'networkidle2'
  })

  while (true) {
    // await page.screenshot({ path: `example-${i++}.png` })

    await page.waitFor(30 * 1000)
  }
}

main().catch(console.error)
