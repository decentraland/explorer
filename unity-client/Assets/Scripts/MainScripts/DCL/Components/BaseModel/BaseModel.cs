using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Every component model must herit from BaseModel, and implement the way he is handling the JSON conversion 
/// It should implement the Equals and GetHashFunction to increase the perfomance
/// </summary>
public abstract class BaseModel
{
    public abstract BaseModel GetDataFromJSON(string json);
}
