using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCreator : MonoBehaviour
{
    public GameObject fire;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.GetChild(1).localScale = new Vector3(0.035f, 0.035f, 0.035f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        Debug.Log("Collider HIT : " + collision.transform.name);
        if (collision.gameObject.name == "DraggableStick")
        {
            Debug.Log("FLAME SIZE INCRESE");
            fire.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
