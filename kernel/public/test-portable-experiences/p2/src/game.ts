function createUI() : UIText {
  const ui = new UICanvas()
  const text = new UIText(ui)
  text.outlineWidth = 0.1
  text.outlineColor = Color4.Black()
  text.color = Color4.Yellow()
  text.fontSize = 30
  text.value = ''
  text.hAlign = 'center'
  text.vAlign = 'top'
  text.hTextAlign = 'center'

  return text
}

class Proximity {
  update(dt: number) {
    if (!gameIsRunning) {
      return
    }

    if (countDown > 0) {
      let dist = distance(camera.feetPosition, positionToReach)
      if (dist < 20) {
        text.value = 'CONGRATULATIONS, YOU FOUND IT!!'
        text.color = Color4.Green()
        finishGame()
      }
      else {
        text.value = 'Go to Dragon Rush (-42,54) and find the dragon in\n less than ' + Math.ceil(countDown) + ' seconds!'
      }

      countDown -= dt
    } else {
      text.value = 'GAME OVER'
      text.color = Color4.Red()
      finishGame()
    }
  }
}

function distance(pos1: Vector3, pos2: Vector3): number {
  const a = pos1.x - pos2.x
  const b = pos1.z - pos2.z
  return a * a + b * b
}

async function finishGame() {
  gameIsRunning = false

  await delay(3000)
  
  // TODO: Put here the code for killing the Portable Experience...
}

function delay(ms: number) {
  return new Promise( resolve => setTimeout(resolve, ms) );
}

const camera = Camera.instance
const positionToReach = new Vector3(-658, 7, 864)
let text = createUI()
let countDown = 60
let gameIsRunning = true
engine.addSystem(new Proximity())
