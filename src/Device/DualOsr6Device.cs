using DebugUtils;
using System.Linq;
using System.Text;
using ToySerialController.MotionSource;
using ToySerialController.Device.OutputTarget;
using UnityEngine;
using ToySerialController.Utils;
using ToySerialController.UI;
using SimpleJSON;
using System.Collections.Generic;
using System;

namespace ToySerialController.Device
{
    public class DualOsr6Device : IDevice
    {
        private TCodeDevice DeviceA;
        private TCodeDevice DeviceB;

        public DualOsr6Device()
        {
            DeviceA = new TCodeDevice();
            DeviceB = new TCodeDevice();
        }

        public void CreateUI(IUIBuilder builder)
        {
            DeviceA.CreateUI(builder);
            // Ideally DeviceB should have its own UI components with unique names
            // Due to TCodeDevice.UI.cs having fixed component IDs, creating B's UI here would
            // overwrite or conflict with A's. For this PoC, we let them share the UI settings
            // by syncing B's parameters from A's, or ignoring B's config entirely.
        }

        public void DestroyUI(IUIBuilder builder)
        {
            DeviceA.DestroyUI(builder);
        }

        public void StoreConfig(JSONNode config)
        {
            DeviceA.StoreConfig(config);
            // B could be serialized to a different key, but for now we sync them
        }

        public void RestoreConfig(JSONNode config)
        {
            DeviceA.RestoreConfig(config);
            // Apply A's config to B directly as a workaround for shared UI
            DeviceB.RestoreConfig(config);
        }

        public void Dispose()
        {
            DeviceA.Dispose();
            DeviceB.Dispose();
        }


        private StringOutputTarget dummyA = new StringOutputTarget();
        private StringOutputTarget dummyB = new StringOutputTarget();

        public bool Update(IMotionSource motionSource, IOutputTarget outputTarget)
        {
            if (motionSource is IDualMotionSource dualSource)
            {
                dualSource.UpdateDualDevice(DeviceA, DeviceB);
            }
            else
            {
                DeviceA.Update(motionSource, null);
                DeviceB.Update(motionSource, null);
            }

            // Must call UpdateValues to process targets to cmds
            dummyA.Clear();
            dummyB.Clear();
            DeviceA.UpdateValues(dummyA);
            DeviceB.UpdateValues(dummyB);

            if (outputTarget is DualUdpOutputTarget dualTarget)
            {
                dualTarget.Write($"{dummyA.Value}|{dummyB.Value}");
            }
            else
            {
                if (!string.IsNullOrEmpty(dummyA.Value))
                    outputTarget?.Write(dummyA.Value);
            }

            return true;
        }

        public void RecordValues(IDeviceRecorder deviceRecorder)
        {
            DeviceA.RecordValues(deviceRecorder);
        }

        public string GetDeviceReport()
        {
            return "A: " + DeviceA.GetDeviceReport() + "\nB: " + DeviceB.GetDeviceReport();
        }

        public void OnSceneChanging()
        {
            DeviceA.OnSceneChanging();
            DeviceB.OnSceneChanging();
        }

        public void OnSceneChanged()
        {
            DeviceA.OnSceneChanged();
            DeviceB.OnSceneChanged();
        }

        private class StringOutputTarget : IOutputTarget
        {
            public string Value { get; private set; } = "";
            public void Write(string data) => Value += data;
            public void Clear() => Value = "";
            public void CreateUI(IUIBuilder builder) {}
            public void DestroyUI(IUIBuilder builder) {}
            public void StoreConfig(JSONNode config) {}
            public void RestoreConfig(JSONNode config) {}
            public void Dispose() {}
        }
    }
}
