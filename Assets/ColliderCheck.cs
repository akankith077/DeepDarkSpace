using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck : MonoBehaviour
{
    private GameObject carpetObj;
    public GameObject carpet
    {
        get { return carpetObj; }
    }
    // Start is called before the first frame update
    public void OnCollisionEnter(Collision collision)
    {
        carpetObj = collision.gameObject;

        if (collision.gameObject.name == "carpet(Clone)")
        {
            if (collision.gameObject.transform.localScale.x > 0.2)
            {
            }
        }
    }
}