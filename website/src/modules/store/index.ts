import { Kernel } from '../../components/types'

const kernel = (window as Kernel).webApp
export const getKernelStore = () => {
  // create kernel store
  const store = kernel.createStore()
  const container = document.getElementById('gameContainer') as HTMLElement

  kernel
    .initWeb(container)
    .then(() => console.log('website-initUnity completed'))
    .catch((error) => console.error('website-initUnity', error))

  return store
}
