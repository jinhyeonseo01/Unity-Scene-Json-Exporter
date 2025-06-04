using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTargetType(typeof(RectTransform))]
public class RectTransformProperty : BakeObject
{
    public override JObject Bake()
    {
        JObject json = base.Bake();
        var trans = (RectTransform)target;


        json["position"] = BakeExtensions.ToJson(trans.localPosition);
        json["rotation"] = BakeExtensions.ToJson(trans.localRotation);
        json["scale"] = BakeExtensions.ToJson(trans.localScale);
        json["pivot"] = BakeExtensions.ToJson(trans.pivot);
        json["anchorMax"] = BakeExtensions.ToJson(trans.anchorMax);
        json["anchorMin"] = BakeExtensions.ToJson(trans.anchorMin);
        json["anchoredPosition"] = BakeExtensions.ToJson(trans.anchoredPosition);
        json["sizeDelta"] = BakeExtensions.ToJson(trans.sizeDelta);
        json["offsetMin"] = BakeExtensions.ToJson(trans.offsetMin);
        json["offsetMax"] = BakeExtensions.ToJson(trans.offsetMax);
        json["rectSize"] = BakeExtensions.ToJson(new Vector2(trans.rect.width, trans.rect.height));

        return json;
    }
}