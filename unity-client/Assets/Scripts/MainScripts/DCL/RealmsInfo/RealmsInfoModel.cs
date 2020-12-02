using System;
using Variables.RealmsInfo;

[Serializable]
public class RealmsInfoModel
{
    public CurrentRealmModel current;
    public RealmModel[] realms;

    public RealmsInfoModel Clone()
    {
        RealmsInfoModel clone = (RealmsInfoModel)this.MemberwiseClone();
        return clone;
    }
}