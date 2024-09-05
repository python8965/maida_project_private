#if !UNITY_WEBGL
#if UNITY_EDITOR
#define EDITOR_JOB
#endif
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace OnlyNew.BodyProportions
{
    public partial class ScalableBoneJobManager : MonoBehaviour
    {
        private CopyParentDataAll[] copyParentDataAll;
        private CalculateAllJob[] calculateAllJobs;
        private ScalableBoneUpdateJob[] scalableBoneUpdateJobs;
        private ScalableBoneUpdateSimpleJob[] scalableBoneUpdateSimpleJobs;
        private WriteTransformJob writeTransformJob;
        private ScalableBoneJobData scalableBoneJobData;


        private int totalCount;
        private const int innerLoopBatchCount = -1;
        public TransformAccessArray transformAccessArray;
        public TransformAccessArray[] transformAccessArrays;
        private readonly ProfilerMarker editorJobMarker = new ProfilerMarker("ScalableBoneJobForEditor");

        private readonly ProfilerMarker runtimeJobFastModeMarker = new ProfilerMarker("ScalableBoneJobForFastMode");
        private readonly ProfilerMarker runtimeJobMarker = new ProfilerMarker("ScalableBoneJobForRuntime");
        public void UpdateEachFrame()
        {
            SyncScalableBoneData();
            JobHandle jobHandle = new JobHandle();

#if EDITOR_JOB
            if (Application.isPlaying && fastMode)
            {
                runtimeJobFastModeMarker.Begin();
                jobHandle = ScalableBoneUpdateSimpleSchedule(jobHandle);
                jobHandle.Complete();
                runtimeJobFastModeMarker.End();
            }
            else
            {
                editorJobMarker.Begin();
                jobHandle = ScalableBoneScheduleForEditor(jobHandle);
                jobHandle.Complete();
                UndoSupport();
                jobHandle = WriteTransformScheduleForEditor(jobHandle);
                jobHandle.Complete();
                editorJobMarker.End();
            }
#else
            JobsForRuntime(jobHandle);
#endif
            //For Debug
            //SyncTypeOfChange();
            //SyncRelativeRotation();
        }


        private void JobsForRuntime(JobHandle jobHandle)
        {
            if (fastMode)
            {
                runtimeJobFastModeMarker.Begin();
                jobHandle = ScalableBoneUpdateSimpleSchedule(jobHandle);
                jobHandle.Complete();
                runtimeJobFastModeMarker.End();
            }
            else
            {
                runtimeJobMarker.Begin();
                jobHandle = ScalableBoneUpdateSchedule(jobHandle);
                jobHandle.Complete();
                runtimeJobMarker.End();
            }
        }
#if UNITY_EDITOR
        private void UndoSupport()
        {
            Profiler.BeginSample("EditorUndoSupport");
            var undoArray = scalableBoneJobData.undoArray.GetTotalNativeArray();
            for (int i = 0; i < scalableBones.Count; i++)
            {
                if (undoArray[i])
                {
                    UnityEditor.Undo.RecordObject(scalableBones[i].transform, "childbone rotation");
                    undoArray[i] = false;
                }
            }
            Profiler.EndSample();
        }
#endif
        private void SyncTypeOfChange()
        {
            var typeOfChange = scalableBoneJobData.typeOfChangeArray.GetTotalNativeArray();
            var transformed = scalableBoneJobData.transformedArray.GetTotalNativeArray();
            for (var i = 0; i < scalableBones.Count; i++)
            {
                scalableBones[i].typeOfChange = typeOfChange[i];
                scalableBones[i].Transformed = transformed[i];
            }
        }
        private void SyncRelativeRotation()
        {
            var relativeRotationArray = scalableBoneJobData.relativeRotationArray.GetTotalNativeArray();
            for (var i = 0; i < scalableBones.Count; i++)
            {
                scalableBones[i].relativeRotation = relativeRotationArray[i];
            }
        }
        private JobHandle WriteTransformScheduleForEditor(JobHandle jobHandle)
        {
            Profiler.BeginSample("WriteTransformScheduleForEditor");
            jobHandle = writeTransformJob.Schedule(transformAccessArray, jobHandle);
            Profiler.EndSample();
            return jobHandle;
        }

        #region SyncData

        private unsafe void SyncScalableBoneData()
        {
            Profiler.BeginSample("SyncScalableBoneData");
            //for (var i = 0; i < totalCount; i++) scalableBoneJobData.SyncScalableBoneDynamicData(scalableBones[i], i);
            if (scalableBoneJobData.Length == scalableBones.Count)
            {
                Parallel.For(0, scalableBones.Count, (i) =>
                {
                    scalableBoneJobData.SyncScalableBoneDynamicData(scalableBones[i], i);
                });
            }
            else
            {
                Debug.LogError("scalableBoneJobData.Length != scalableBones.Count");
            }
            Profiler.EndSample();
        }

        private void InitScalableBoneData()
        {
            Parallel.For(0, totalCount, i =>
            {
                scalableBoneJobData.InitScalableBoneData(scalableBones[i], i);
                scalableBoneJobData.GetParentIndex(i, scalableBones[i].parentIndex);
            });
        }

        #endregion


        JobHandle ScalableBoneScheduleForEditor(JobHandle jobHandle)
        {
            Profiler.BeginSample("ScalableBoneScheduleForEditor");
            for (int i = 0; i < scalableBoneDepthLists.Count; i++)
            {
                var length = scalableBoneDepthLists[i].Count;
                if (i > 0)
                {
                    jobHandle = copyParentDataAll[i].Schedule(length, innerLoopBatchCount, jobHandle);
                }
                jobHandle = calculateAllJobs[i].Schedule(transformAccessArrays[i], jobHandle);// (length, innerLoopBatchCount, jobHandle);
            }

            Profiler.EndSample();
            return jobHandle;
        }
        JobHandle ScalableBoneUpdateSchedule(JobHandle jobHandle)
        {
            for (int i = 0; i < scalableBoneDepthLists.Count; i++)
            {
                var length = scalableBoneDepthLists[i].Count;
                if (i > 0)
                {
                    jobHandle = copyParentDataAll[i].Schedule(length, innerLoopBatchCount, jobHandle);
                }
                jobHandle = scalableBoneUpdateJobs[i].Schedule(transformAccessArrays[i], jobHandle);// (length, innerLoopBatchCount, jobHandle);
            }
            return jobHandle;
        }
        JobHandle ScalableBoneUpdateSimpleSchedule(JobHandle jobHandle)
        {
            for (int i = 0; i < scalableBoneDepthLists.Count; i++)
            {
                var length = scalableBoneDepthLists[i].Count;
                if (i > 0)
                {
                    jobHandle = copyParentDataAll[i].Schedule(length, innerLoopBatchCount, jobHandle);
                }
                jobHandle = scalableBoneUpdateSimpleJobs[i].Schedule(transformAccessArrays[i], jobHandle);// (length, innerLoopBatchCount, jobHandle);
            }
            return jobHandle;
        }
        private void RemapParentIndex()
        {
            for (var i = 0; i < scalableBoneDepthLists.Count; i++)
            {
                var list = scalableBoneDepthLists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var scalableBone = list[j];
                    scalableBone.SetIndex(j);
                    scalableBone.UpdateParentIndex();
                }
            }
        }

    }
}
#endif