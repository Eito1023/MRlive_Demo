using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_positionleft : MonoBehaviour
{
    // カメラからの距離
    public float distanceFromCameraY = 1.0f;
    // オブジェクトの回転オフセット
    public Quaternion rotationOffset = Quaternion.identity;
    public GameObject planeRightObject;

    void Update()
    {
        planeRightObject = GameObject.Find("Plane_left");
        // カメラの位置と回転を取得

        Transform ObjectTransform = planeRightObject.transform;
        Vector3 cameraPosition = ObjectTransform.position;
        Quaternion cameraRotation = ObjectTransform.rotation;

        // オブジェクトをカメラの正面に配置
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + ObjectTransform.forward * distanceFromCameraY;
        objectTransform.rotation = cameraRotation * rotationOffset;

    }
}
