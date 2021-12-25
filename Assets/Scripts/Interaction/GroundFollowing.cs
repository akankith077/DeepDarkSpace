using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFollowing : MonoBehaviour
{
    public float distanceToGround=10;

    void Update()
    {
        GroundFollowingFunc();
    }

    void GroundFollowingFunc()
    {
        if (Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.2f))
        {
            transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        }
        else if (!Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.3f))
        {
            transform.Translate(Vector3.down * Time.deltaTime, Space.World);
        }
    }
}
