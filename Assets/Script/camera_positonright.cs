using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_positonright : MonoBehaviour
{
    // �J��������̋���
    public float distanceFromCameraZ = 1.0f;
    // �I�u�W�F�N�g�̉�]�I�t�Z�b�g
    public Quaternion rotationOffset = Quaternion.identity;
    public GameObject planeRightObject;

    void Update()
    {
        planeRightObject = GameObject.Find("Plane_right");
        // �J�����̈ʒu�Ɖ�]���擾

        Transform ObjectTransform = planeRightObject.transform;
        Vector3 cameraPosition = ObjectTransform.position;
        Quaternion cameraRotation = ObjectTransform.rotation;

        // �I�u�W�F�N�g���J�����̐��ʂɔz�u
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + ObjectTransform.forward * distanceFromCameraZ;
        objectTransform.rotation = cameraRotation * rotationOffset;

    }
}
