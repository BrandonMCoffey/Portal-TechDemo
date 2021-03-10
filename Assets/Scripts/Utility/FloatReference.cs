using System;

namespace Assets.Scripts.Utility {
    [Serializable]
    public class FloatReference {
        public bool UseConstant = true;
        public float ConstantValue;
        public FloatVariable Variable;

        public FloatReference()
        {
        }

        public FloatReference(float value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public float Value => UseConstant ? ConstantValue : Variable.Value;

        public static implicit operator float(FloatReference reference)
        {
            return reference.Value;
        }
    }
}