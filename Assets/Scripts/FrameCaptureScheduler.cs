using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualHuman.Vision
{
    [DisallowMultipleComponent]
    public class FrameCaptureScheduler : MonoBehaviour
    {
        [SerializeField] EyeCameraSensor sensor;
        [SerializeField] float captureHz = 10f;// 1s take 10 photos
        [SerializeField] int maxInflight = 2;//最多同时发起多少次回读请求

    

        bool isRunning = true;
        double nextTime;
        long frameId = 0;

        // debug内容：
        long captured; // 已经发起的采集请求数。即：只要请求发出去就记录
        long droppedLate; // update调用时错过采样的数量
        long droppedInflight; // 请求堆叠主动放弃的数量
        // 这里droppedLate 高说明主线程被阻塞。
        // droppedInflight 高说明回读请求堆叠过多，解码、理解效率低而导致主动丢弃

        void OnDisable () => isRunning = false;

        void Update()
        {
            if (Input.GetButtonDown("ToggleKey"))
            {
                isRunning = !isRunning;
                // Debug.Log($"[FrameCaptureScheduler] Toggle capture: {isRunning}");
            }
            double now = Time.unscaledTimeAsDouble;// unscaledDeltaTime是上一帧的时间间隔，Time.time是从游戏开始到现在的时间, Time.unscaledTime是从游戏开始到现在的时间，不受时间缩放影响
            if(!isRunning) {nextTime = now; return;}// 如果不运行，重置下一次采集时间为当前时间
            if(now < nextTime) return;// 如果当前时间还没到下一次采集就return，即只在需要采集的时候进行采集

            double interval = 1f / captureHz;

            if (now - nextTime > interval) { droppedLate++; nextTime = now + interval; }// 如果现在已经过了下一次采集时间，相当于没采集到，所以记录由于延迟而丢弃的数量，然后更新下一次采集时间
            else { nextTime += interval; } // 否则就更新下一次采集时间

            if (sensor.Inflight >= maxInflight) { droppedInflight++; return; } // 如果排队的数量过多，就不管了，然后记录一下丢弃了几个。

            sensor.CaptureOnce(++frameId);
            captured++;

            // Debug.Log($"[FrameCaptureScheduler] Capture frame={frameId} inflight={sensor.Inflight} " +
            //           $"captured={captured} droppedLate={droppedLate} droppedInflight={droppedInflight}");

        }

           

    }
}