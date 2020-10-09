import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { LoadingMessage } from "./LoadingMessage";
import { ProgressBar } from "./ProgressBar";

export type LoadingTip = {
  text: string;
  image: string;
};

export const loadingTips: Array<LoadingTip> = [
  {
    text: `MANA is Decentraland’s virtual currency. Use it to buy LAND and other premium items, vote on key policies and pay platform fees.`,
    image: "images/decentraland-connect/loadingtips/Mana.png",
  },
  {
    text: `Buy and sell LAND, Estates, Avatar wearables and names in the Decentraland Marketplace: stocking the very best digital goods and paraphernalia backed by the ethereum blockchain.`,
    image: "images/decentraland-connect/loadingtips/Marketplace.png",
  },
  {
    text: `Create scenes, artworks, challenges and more, using the simple Builder: an easy drag and drop tool. For more experienced creators, the SDK provides the tools to fill the world with social games and applications.`,
    image: "images/decentraland-connect/loadingtips/Land.png",
  },
  {
    text: `Decentraland is made up of over 90,000 LANDs: virtual spaces backed by cryptographic tokens. Only LANDowners can determine the content that sits on their LAND.`,
    image: "images/decentraland-connect/loadingtips/LandImg.png",
  },
  {
    text: `Except for the default set of wearables you get when you start out, each wearable model has a limited supply. The rarest ones can get to be super valuable. You can buy and sell them in the Marketplace.`,
    image: "images/decentraland-connect/loadingtips/WearablesImg.png",
  },
  {
    text: `Decentraland is the first fully decentralized virtual world. By voting through the DAO  ('Decentralized Autonomous Organization'), you are in control of the policies created to determine how the world behaves.`,
    image: "images/decentraland-connect/loadingtips/DAOImg.png",
  },
  {
    text: `Genesis Plaza is built and maintained by the Decentraland Foundation but is still in many ways a community project. Around here you'll find several teleports that can take you directly to special scenes marked as points of interest.`,
    image: "images/decentraland-connect/loadingtips/GenesisPlazaImg.png",
  },
];

export type LoadingState = {
  status: string;
  helpText: number;
  pendingScenes: number;
  message: string;
  subsystemsLoad: number;
  loadPercentage: number;
  initialLoad: boolean;
  showLoadingScreen: boolean;
};
const mapStateToProps = (state: any) => {
  return {
    state: state.loading,
    showWalletPrompt: state.session.showWalletPrompt || false,
  };
};

export interface LoadingContainerProps {
  state: LoadingState;
  showWalletPrompt: boolean;
}

const changeTip = (current: number) => {
  return current + 1 < loadingTips.length ? current + 1 : 0;
};

export const LoadingContainer: React.FC<LoadingContainerProps> = (props) => {
  const { state, showWalletPrompt } = props;
  const [step, setStep]: [number, any] = useState(0);
  // setting animation of loading
  useEffect(() => {
    const interval = setInterval(() => setStep(changeTip), 10000);
    return () => clearInterval(interval);
  }, []);

  const tip = loadingTips[step];
  const subMessage =
    state.pendingScenes > 0
      ? state.message || "Loading scenes..."
      : state.status;
  const percentage = Math.min(
    state.initialLoad
      ? (state.loadPercentage + state.subsystemsLoad) / 2
      : state.loadPercentage,
    100
  );
  return (
    <React.Fragment>
      {state.showLoadingScreen && (
        <LoadingMessage
          image={tip.image}
          message={tip.text}
          subMessage={subMessage}
          showWalletPrompt={showWalletPrompt}
        />
      )}
      {state.showLoadingScreen && <ProgressBar percentage={percentage} />}
    </React.Fragment>
  );
};

export default connect(mapStateToProps)(LoadingContainer);
