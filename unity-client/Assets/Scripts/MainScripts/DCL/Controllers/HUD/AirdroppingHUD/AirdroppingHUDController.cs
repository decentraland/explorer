using System;
using DCL.Interface;

public class AirdroppingHUDController : IHUD, IDisposable
{

    [Serializable]
    public class Model
    {
        public string id;
        public string title;
        public string subtitle;
        public ItemModel[] items;
    }

    [Serializable]
    public class ItemModel
    {
        public string name;
        public string thumbnailURL;
        public string type;
        public string rarity;
        public string subtitle;
    }

    public enum State
    {
        Hidden,
        Initial,
        SingleItem,
        Summary,
        Finish
    }

    private AirdroppingHUDView view;
    private State currentState;
    private Model model;
    private int currentItemShown = -1;

    public AirdroppingHUDController()
    {
        view = AirdroppingHUDView.Create();
        view.Initialize(MoveToNextState);
        currentState = State.Hidden;
        ApplyState();
    }

    public void AirdroppingRequested(Model model)
    {
        if (model == null) return;

        this.model = model;
        currentState = State.Initial;
        ApplyState();
    }

    public void MoveToNextState()
    {
        SetNextState();
        ApplyState();
    }

    public void SetNextState()
    {
        switch (currentState)
        {
            case State.Initial:
                currentItemShown = 0;
                currentState = State.SingleItem;
                break;
            case State.SingleItem:
                currentItemShown++;
                if (model.items == null || currentItemShown > model.items.Length - 1)
                    currentState = State.Summary;
                break;
            case State.Summary:
                currentState = State.Hidden;
                break;
            case State.Finish:
            default:
                currentState = State.Hidden;
                break;
        }
    }

    public void ApplyState()
    {
        switch (currentState)
        {
            case State.Initial:
                view.ShowInitialScreen(model.title, model.subtitle);
                break;
            case State.SingleItem:
                view.ShowItemScreen(model.items[currentItemShown], model.items.Length - (currentItemShown + 1));
                break;
            case State.Summary:
                view.ShowSummaryScreen(model.items);
                break;
            case State.Finish:
                WebInterface.SendUserAcceptedCollectibles(model.id);
                MoveToNextState();
                break;
            case State.Hidden:
            default:
                model = null;
                view.CleanState();
                break;
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(view.gameObject);
    }
}