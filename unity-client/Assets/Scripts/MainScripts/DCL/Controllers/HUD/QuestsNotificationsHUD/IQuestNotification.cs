using System.Collections;

namespace DCL.Huds.QuestsNotifications
{
    public interface IQuestNotification
    {
        void Show();
        void Dispose();
        IEnumerator Waiter();
    }
}