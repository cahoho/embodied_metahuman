using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetaHuman.Vision
{
    [DisallowMultipleComponent]
    public class FrameCaptureScheduler : MonoBehaviour
    {
        [SerializeField] EyeCameraSensor sensor;
        [SerializeField] float captureHz = 10f;// 1s take 10 photos
        [SerializeField] int maxInflight = 2;//最多同时发起多少次回读请求

    

        bool isRunning = true;
        float nextTime;
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
                Debug.Log($"[FrameCaptureScheduler] Toggle capture: {isRunning}");
            }
            float now = Time.unscaledTime;// unscaledDeltaTime是上一帧的时间间隔，Time.time是从游戏开始到现在的时间, Time.unscaledTime是从游戏开始到现在的时间，不受时间缩放影响
            if(!isRunning) {nextTime = now; return;}// 如果不运行，重置下一次采集时间为当前时间
            if(now < nextTime) return;// 如果当前时间还没到下一次采集

            float interval = 1f / captureHz;

            if (now - nextTime > interval) { droppedLate++; nextTime = now + interval; }
            else { nextTime += interval; }

            if (sensor.Inflight >= maxInflight) { droppedInflight++; return; }

            sensor.CaptureOnce(++frameId);
            captured++;

            Debug.Log($"[FrameCaptureScheduler] Capture frame={frameId} inflight={sensor.Inflight} " +
                      $"captured={captured} droppedLate={droppedLate} droppedInflight={droppedInflight}");

        }

           

    }
}