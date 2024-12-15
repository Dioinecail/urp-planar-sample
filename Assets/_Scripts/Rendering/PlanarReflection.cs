namespace Custom.Rendering
{
    using UnityEngine;

    public class PlanarReflection : MonoBehaviour
    {
        // referenses
        [Tooltip("Main camera, generally can just replace it with Camera.main on start")]
        public Camera mainCamera;
        [Tooltip("Reflection render camera, can also just create the camera at runtime")]
        public Camera reflectionCamera;

        [Tooltip("The referense plane from which camera will reflect")]
        public Transform reflectionPlane;
        [Tooltip("Set your materials that need a planar reflection here")]
        public Material[] reflectiveMaterials;

        // parameters
        [Tooltip("Additional vertical camera offset in case needed")]
        public float verticalOffset;
        [Tooltip("Copy parameters from the main camera")]
        public bool isCopyParameters;

        // cache
        private Transform mainCamTransform;
        private Transform reflectionCamTransform;
        private RenderTexture reflectionTexture;



        private void OnEnable() 
        {
            reflectionTexture = new RenderTexture(Screen.width, Screen.height, 24);

            reflectionCamera.targetTexture = reflectionTexture;
            mainCamTransform = mainCamera.transform;
            reflectionCamTransform = reflectionCamera.transform;

            if (isCopyParameters) 
            {
                reflectionCamera.CopyFrom(mainCamera);
            }

            foreach (var mat in reflectiveMaterials) 
            {
                mat.SetTexture("_ReflectionTex", reflectionTexture);
            }
        }

        private void OnDisable() 
        {
            reflectionTexture = null;
        }

        private void Update()
        {
            RenderReflection();
        }

        private void RenderReflection()
        {
            // take main camera directions and position world space
            Vector3 cameraDirectionWorldSpace = mainCamTransform.forward;
            Vector3 cameraUpWorldSpace = mainCamTransform.up;
            Vector3 cameraPositionWorldSpace = mainCamTransform.position;

            cameraPositionWorldSpace.y += verticalOffset;

            // transform direction and position by reflection plane
            Vector3 cameraDirectionPlaneSpace = reflectionPlane.InverseTransformDirection(cameraDirectionWorldSpace);
            Vector3 cameraUpPlaneSpace = reflectionPlane.InverseTransformDirection(cameraUpWorldSpace);
            Vector3 cameraPositionPlaneSpace = reflectionPlane.InverseTransformPoint(cameraPositionWorldSpace);

            // invert direction and position by reflection plane
            cameraDirectionPlaneSpace.y *= -1;
            cameraUpPlaneSpace.y *= -1;
            cameraPositionPlaneSpace.y *= -1;

            // transform direction and position from reflection plane local space to world space
            cameraDirectionWorldSpace = reflectionPlane.TransformDirection(cameraDirectionPlaneSpace);
            cameraUpWorldSpace = reflectionPlane.TransformDirection(cameraUpPlaneSpace);
            cameraPositionWorldSpace = reflectionPlane.TransformPoint(cameraPositionPlaneSpace);

            // apply direction and position to reflection camera
            reflectionCamTransform.position = cameraPositionWorldSpace;
            reflectionCamTransform.LookAt(cameraPositionWorldSpace + cameraDirectionWorldSpace, cameraUpWorldSpace);

            reflectionCamera.Render();
        }
    }
}