using UnityEngine;
using Variables.RealmsInfo;

public class RealmsInfo
{
    public static RealmsInfo i
    {
        get
        {
            if (info == null)
            {
                info = new RealmsInfo();
            }
            return info;
        }
    }

    public CurrentRealmVariable currentRealm => DataStorage.playerRealm;
    public RealmsVariable realms => DataStorage.realmsInfo;

    static private RealmsInfo info = null;

    private RealmsInfoModel model = new RealmsInfoModel();

    public void Set(string json)
    {
        JsonUtility.FromJsonOverwrite(json, model);
        Set(model);
    }

    public void Set(RealmsInfoModel newModel)
    {
        model = newModel;
        currentRealm.Set(model.current);
        realms.Set(model.realms);
    }
}
