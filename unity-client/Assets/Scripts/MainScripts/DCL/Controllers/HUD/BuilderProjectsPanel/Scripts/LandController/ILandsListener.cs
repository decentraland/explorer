using System.Collections.Generic;

internal interface ILandsListener
{
    void OnSetLands(List<LandWithAccess> lands);
}
