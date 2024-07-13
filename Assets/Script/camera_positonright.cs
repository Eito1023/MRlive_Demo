using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_positonright : MonoBehaviour
{
    // カメラからの距離
    public float distanceFromCameraZ = 1.0f;
    // オブジェクトの回転オフセット
    public Quaternion rotationOffset = Quaternion.identity;
    public GameObject planeRightObject;

    void Update()
    {
        planeRightObject = GameObject.Find("Plane_right");
        // カメラの位置と回転を取得

        Transform ObjectTransform = planeRightObject.transform;
        Vector3 cameraPosition = ObjectTransform.position;
        Quaternion cameraRotation = ObjectTransform.rotation;

        // オブジェクトをカメラの正面に配置
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + ObjectTransform.forward * distanceFromCameraZ;
        objectTransform.rotation = cameraRotation * rotationOffset;

    }
}
