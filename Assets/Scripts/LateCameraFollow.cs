using UnityEngine;

public class LateCameraFollow : MonoBehaviour
{
    public GameObject TrackingObject;

    void LateUpdate()
    {
        if (TrackingObject is null || TrackingObject == null) return;

        Vector3 targetPos = TrackingObject.transform.position;
        targetPos.y = transform.position.y;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
    }
}
