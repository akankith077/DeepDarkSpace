using UnityEngine;

public class CameraTranslateOnly : MonoBehaviour
{
    // Cancels the rotation of the game object such that children
    // are only affected by the translation of its parent

    void LateUpdate()
    {
        this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    }
}
