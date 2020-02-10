using System.Collections.Generic;

namespace DCL
{
    public class LoadingLock
    {
        public event System.Action OnUnlocked;

        int lockCounter = 0;
        HashSet<object> lockIds = new HashSet<object>();

        public bool isUnlocked => lockCounter == 0;

        public void Lock(object id)
        {
            if (lockIds.Contains(id))
                return;

            lockIds.Add(id);
            lockCounter++;
        }

        public void Unlock(object id)
        {
            if (!lockIds.Contains(id))
                return;

            lockIds.Remove(id);
            lockCounter--;

            if (lockCounter == 0)
            {
                OnUnlocked?.Invoke();
            }
        }
    }
}
