using SimpleJSON;
using ToySerialController.Device;
using ToySerialController.UI;
using UnityEngine;
using System.Reflection;

namespace ToySerialController.MotionSource
{
    public class DualFeetMotionSource : IDualMotionSource
    {
        private IMotionSourceReference Reference;
        private FemaleFeetTarget LeftFootTarget;
        private FemaleFeetTarget RightFootTarget;

        private UIDynamicButton Title;

        public Vector3 ReferencePosition => Reference?.Position ?? Vector3.zero;
        public Vector3 ReferenceUp => Reference?.Up ?? Vector3.up;
        public Vector3 ReferenceRight => Reference?.Right ?? Vector3.right;
        public Vector3 ReferenceForward => Reference?.Forward ?? Vector3.forward;
        public float ReferenceLength => Reference?.Length ?? 1.0f;
        public float ReferenceRadius => Reference?.Radius ?? 0.1f;
        public Vector3 ReferencePlaneNormal => Reference?.PlaneNormal ?? Vector3.up;
        public Vector3 ReferencePlaneTangent => Reference?.PlaneTangent ?? Vector3.right;

        public Vector3 TargetPosition => LeftFootTarget != null && RightFootTarget != null ? (LeftFootTarget.Position + RightFootTarget.Position) / 2 : Vector3.zero;
        public Vector3 TargetUp => Vector3.up;
        public Vector3 TargetRight => Vector3.right;
        public Vector3 TargetForward => Vector3.forward;

        public DualFeetMotionSource(IMotionSourceReference reference, FemaleFeetTarget leftFoot, FemaleFeetTarget rightFoot)
        {
            Reference = reference;
            LeftFootTarget = leftFoot;
            RightFootTarget = rightFoot;
        }

        public void CreateUI(IUIBuilder builder)
        {
            Title = builder.CreateDisabledButton("Dual Feet Motion Source", new Color(0.3f, 0.3f, 0.3f), Color.white);
            Reference?.CreateUI(builder);
            LeftFootTarget?.CreateUI(builder);
            RightFootTarget?.CreateUI(builder);
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(Title);
            Reference?.DestroyUI(builder);
            LeftFootTarget?.DestroyUI(builder);
            RightFootTarget?.DestroyUI(builder);
        }

        public void OnSceneChanged()
        {
            Reference?.OnSceneChanged();
            LeftFootTarget?.OnSceneChanged();
            RightFootTarget?.OnSceneChanged();
        }

        public void OnSceneChanging()
        {
            Reference?.OnSceneChanging();
            LeftFootTarget?.OnSceneChanging();
            RightFootTarget?.OnSceneChanging();
        }

        public void RestoreConfig(JSONNode config)
        {
            Reference?.RestoreConfig(config);
            LeftFootTarget?.RestoreConfig(config);
            RightFootTarget?.RestoreConfig(config);
        }

        public void StoreConfig(JSONNode config)
        {
            Reference?.StoreConfig(config);
            LeftFootTarget?.StoreConfig(config);
            RightFootTarget?.StoreConfig(config);
        }

        public bool Update()
        {
            var refValid = Reference?.Update() ?? false;
            var leftValid = LeftFootTarget?.Update() ?? false;
            var rightValid = RightFootTarget?.Update() ?? false;

            return refValid && leftValid && rightValid;
        }

        public void UpdateDualDevice(TCodeDevice deviceA, TCodeDevice deviceB)
        {
            if (Reference == null || LeftFootTarget == null || RightFootTarget == null) return;

            // Z-axis stroke (L0)
            var leftVec = LeftFootTarget.Position - Reference.Position;
            var rightVec = RightFootTarget.Position - Reference.Position;

            var zMotionA = Vector3.Dot(leftVec, Reference.Up) / Reference.Length;
            var zMotionB = Vector3.Dot(rightVec, Reference.Up) / Reference.Length;

            float L0_A = Mathf.Clamp01(zMotionA + 0.5f);
            float L0_B = Mathf.Clamp01(zMotionB + 0.5f);

            SetTargets(deviceA, L0_A, 0.5f, CalculateL2(L0_A), 0.5f, 0.5f, 0.5f);
            SetTargets(deviceB, L0_B, 0.5f, CalculateL2(L0_B), 0.5f, 1.0f - 0.5f, 0.5f);
        }

        private float CalculateL2(float L0)
        {
            return Mathf.Clamp01(0.5f - (L0 - 0.5f));
        }

        private void SetTargets(TCodeDevice device, float l0, float l1, float l2, float r0, float r1, float r2)
        {
            device.SetTargets(l0, l1, l2, r0, r1, r2);
        }
    }
}
