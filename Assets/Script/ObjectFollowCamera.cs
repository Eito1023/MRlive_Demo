using UnityEngine;

public class ObjectFollowCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // �J��������̋���
    public float distanceFromCamera = 1.0f;
    // �I�u�W�F�N�g�̉�]�I�t�Z�b�g
    public Quaternion rotationOffset = Quaternion.identity;
    // �I�u�W�F�N�g�̉������I�t�Z�b�g
    public float horizontalOffset = 0.0f;

    void Update()
    {
        // �J�����̈ʒu�Ɖ�]���擾
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraPosition = cameraTransform.position;
        Quaternion cameraRotation = cameraTransform.rotation;

        // �I�u�W�F�N�g���J�����̐��ʂɔz�u
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
        objectTransform.rotation = cameraRotation * rotationOffset;

        // �I�u�W�F�N�g�̉������I�t�Z�b�g��K�p
        Vector3 rightVector = Quaternion.Euler(0.0f, cameraRotation.eulerAngles.y, 0.0f) * Vector3.right;
        objectTransform.position += rightVector * horizontalOffset;
    }
}
