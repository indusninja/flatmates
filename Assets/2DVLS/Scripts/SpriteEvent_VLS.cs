using UnityEngine;
using System.Collections;

public class SpriteEvent_VLS : MonoBehaviour
{
    SpriteRenderer sRender;
    bool error = false;
    Color shadedColor = new Color(0.1f, 0.1f, 0.1f, 1f);

    public GameObject colliderObject;

    void Start()
    {
        sRender = gameObject.GetComponent<SpriteRenderer>();

        if (sRender == null)
        {
            Debug.LogError("Object must have a sprite renderer attached", this);
            error = true;
            return;
        }

        if (colliderObject == null)
        {
	        colliderObject = gameObject;
	        //Debug.LogError("Collider Object must be assigned", this);
	        //error = true;
	        //return;
        }

        shadedColor = avgColor = sRender.color;

        Light2D.RegisterEventListener(LightEventListenerType.OnStay, OnStayEvent);
    }

    void Update()
    {
        if (error)
            return;

        sRender.color = GetAvgColor();
    }

    void OnStayEvent(Light2D light, GameObject go)
    {
        if (go.GetInstanceID() == colliderObject.GetInstanceID())
        {
            float d = Vector3.Distance(light.gameObject.transform.position, colliderObject.transform.position) / light.LightRadius;
            Color c = new Color(light.LightColor.r, light.LightColor.g, light.LightColor.b);
            AddColor(Color.Lerp(c, new Color(0, 0, 0, 1f), Mathf.Clamp(d, 0, 1f)));
        }
    }

    Color avgColor;
    void AddColor(Color c)
    {
        avgColor += c;
    }
    Color GetAvgColor()
    {
        Color c = new Color(avgColor.r, avgColor.g, avgColor.b, shadedColor.a);
        avgColor = shadedColor;
        return c;
    }
}
