using UnityEngine;

namespace MetaHuman.Vision
{
    [System.Serializable]
    public struct CameraPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fovYDeg;
        public float near;
        public float far;
        public int width;
        public int height;

        public static CameraPose From(Camera cam)
        {
            RenderTexture rt = cam.targetTexture;
            return new CameraPose
            {
                position = cam.transform.position,
                rotation = cam.transform.rotation,
                fovYDeg  = cam.fieldOfView,
                near     = cam.nearClipPlane,
                far      = cam.farClipPlane,
                width    = rt != null ? rt.width  : Screen.width,
                height   = rt != null ? rt.height : Screen.height,
            };
        }
    }

   
}