using Newtonsoft.Json.Linq;
using UnityEngine;


public class BakeObject<T> where T : class
{
    public T target;
    public string guid;
    public string type;

    public virtual void PrevProcessing()
    {
        if (target != null)
        {
            guid = BakeUnity.TrySetGuid(target);
            type = target.GetType().Name;
        }
    }

    public virtual JObject Bake()
    {
        JObject json = new JObject();

        return json;
    }
}
