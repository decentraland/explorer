import { initShared, initWeb, loadUnity } from "decentraland-kernel";

export const getKernelStore = () => {
  const start = Date.now();
  // create kernel store
  initShared();
  const container = document.getElementById("gameContainer") as HTMLElement;
  //async initialize
  initWeb(container)
    .then((initResult: any) => {
      console.log("website-initWeb completed at: ", Date.now() - start);
      return loadUnity(initResult).then(() => {
        console.log("website-loadUnity completed at: ", Date.now() - start);
      });
    })
    .then(() => console.log("website-initUnity completed"));

  return (window as any).globalThis.globalStore;
};
