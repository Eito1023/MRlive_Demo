using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CustomOVRCameraRig : OVRCameraRig
{
    public Transform leftEyeCustomAnchor;
    public Transform rightEyeCustomAnchor;

    protected override void Update()
    {
        if (leftEyeAnchor != null && leftEyeCustomAnchor != null)
        {
            leftEyeAnchor.localPosition = leftEyeCustomAnchor.localPosition;
            leftEyeAnchor.localRotation = leftEyeCustomAnchor.localRotation;
        }

        if (rightEyeAnchor != null && rightEyeCustomAnchor != null)
        {
            rightEyeAnchor.localPosition = rightEyeCustomAnchor.localPosition;
            rightEyeAnchor.localRotation = rightEyeCustomAnchor.localRotation;
        }

        base.Update();
    }
}
