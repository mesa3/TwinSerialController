using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class FemaleFeetTarget : AbstractPersonTarget
    {
        protected override DAZCharacterSelector.Gender TargetGender => DAZCharacterSelector.Gender.Female;
        protected override string DefaultTarget => "Left Foot";

        public FemaleFeetTarget(string footName)
        {
            RegisterTarget(footName, r => UpdateFreeControllerTarget(footName == "Left Foot" ? "lFootControl" : "rFootControl", r));
            RegisterAutoTarget(footName);
        }
    }
}
