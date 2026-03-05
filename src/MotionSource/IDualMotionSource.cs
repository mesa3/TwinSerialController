using ToySerialController.Device;

namespace ToySerialController.MotionSource
{
    public interface IDualMotionSource : IMotionSource
    {
        void UpdateDualDevice(TCodeDevice deviceA, TCodeDevice deviceB);
    }
}
