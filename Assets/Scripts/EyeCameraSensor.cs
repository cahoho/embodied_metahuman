using System;
using System.IO;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine;


namespace MetaHuman.Vision
{
    [DisallowMultipleComponent] // 不允许挂载多个此组件
    public sealed class EyeCameraSensor : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Camera eyeCamera;
        [SerializeField] private RenderTexture targetTexture;

        [Header("配置")]
        [SerializeField, Range(50,95)] int jptQuality = 75;

        long frameID = 0;


        static string CaptureDir()//savethe jpg to a location
        {
            var root = Directory.GetParent(Application.dataPath);
            return Path.Combine(root != null? root.FullName: Application.dataPath, "Logs", "captures");
        }
        
        void Awake()
        {
            eyeCamera.targetTexture = targetTexture!= null? targetTexture: null;
            Debug.Log($"[EyeCameraSensor] 就绪 {targetTexture.width}x{targetTexture.height} " +
                      $"aa={targetTexture.antiAliasing} gfx={targetTexture.graphicsFormat} " +
                      $"depth={targetTexture.depth} srgb={targetTexture.sRGB} | " +
                      $"fovY={eyeCamera.fieldOfView} near={eyeCamera.nearClipPlane} " +
                      $"far={eyeCamera.farClipPlane} | 输出location: {CaptureDir()}");        
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                CaptureOnce(frameID++);
            }
        }

        public void CaptureOnce(long id)
        {
            
        }


    }

}
