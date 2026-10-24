using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;


namespace VirtualHuman.Vision
{
    [DisallowMultipleComponent] // 不允许挂载多个此组件
    public sealed class EyeCameraSensor : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Camera eyeCamera;
        [SerializeField] private RenderTexture targetTexture;

        [Header("配置")]
        [SerializeField, Range(50,95)] int jpegQuality = 75;

        int inflight; // 飞行中的（正在排队的）



/// <summary>
/// 已经发起但是回调尚未完成的回读请求数。用于控制采集频率，避免堆叠过多导致卡顿
/// </summary>
        public int Inflight => inflight;

        static string CaptureDir()//savethe jpg to a location
        {
            var root = Directory.GetParent(Application.dataPath);
            return Path.Combine(root != null? root.FullName: Application.dataPath, "Logs", "perception");
        }
        
        void Awake()
        {
            if (eyeCamera == null || targetTexture == null)
            {
                Debug.LogError("[EyeCameraSensor] 引用缺失");
                enabled = false;
                return;
            }
            eyeCamera.targetTexture = targetTexture!= null? targetTexture: null;
            Debug.Log($"[EyeCameraSensor] 就绪 {targetTexture.width}x{targetTexture.height} " +
                      $"aa={targetTexture.antiAliasing} gfx={targetTexture.graphicsFormat} " +
                      $"depth={targetTexture.depth} srgb={targetTexture.sRGB} | " +
                      $"fovY={eyeCamera.fieldOfView} near={eyeCamera.nearClipPlane} " +
                      $"far={eyeCamera.farClipPlane} | 输出location: {CaptureDir()}");        
        }

        public void CaptureOnce(long id)
        {
            //快照：
            CameraPose framePose = CameraPose.From(eyeCamera); //时机：先获取pose。
            //必须在发起回读之前存储好，防止后期转头等导致拿到的时机出问题
            long captureTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int quality = jpegQuality;


            // rendertexture的内容在显存里，CPU不能直接读。传统做法：
            // 把RT设为rendertexture.active，然后用Texture2D.ReadPixels()读到CPU内存里。弊端：会阻塞GPU，导致卡顿。

            // 这里采用：
            // AsyncGPUReadback.Request()，异步读回显存数据到CPU内存里。弊端：需要等一帧才能拿到数据，不能立即使用。


            inflight++;
            // request是lambda闭包回调函数，
            AsyncGPUReadback.Request(targetTexture, 0, TextureFormat.RGBA32, request => // 时机：获取到pose之后进行异步回读（GPU--传输数据到-->CPU）
            {
                // 它会减少cpu对gpu的等待和阻塞。虽然代价是延迟几帧，但由于项目是实时感知，阻塞造成的成本远大于延迟几帧的成本
                inflight--;
                if (request.hasError)
                {
                    Debug.LogWarning($"[EyeSensor] TRANSPORT_READBACK_ERROR frame={id}");
                    return;
                    //核心：不重试。因为这个项目是在线感知——下一秒就有新的一帧来了，重试旧帧只会造成积压
                }

                //NativeArray：非托管内存数组
                // 托管内存：代码泡在mono或li2cpp的脚本环境（这就是托管环境），无需手动申请、释放内存。https://docs.unity3d.com/cn/6000.0/Manual/performance-managed-memory.html



                NativeArray<byte> rgba = request.GetData<byte>();

                using NativeArray<byte> jpg = ImageConversion.EncodeNativeArrayToJPG(
                    rgba, GraphicsFormat.R8G8B8A8_UNorm, (uint)targetTexture.width, (uint)targetTexture.height,0 ,quality
                );


                try
                {
                    string dir = CaptureDir();
                    Directory.CreateDirectory(dir);
                    string path = Path.Combine(dir, $"frame_{id:D6}_{captureTs}.jpg");
                    File.WriteAllBytes(path, jpg.ToArray());

                    Debug.Log($"[EyeSensor] just saved {Path.GetFileName(path)}" + $"bytes={jpg.Length} pos={framePose.position}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EyeSensor] failed to save frame {id}: {e.Message}");
                }

            });
        }

        


    }

}
