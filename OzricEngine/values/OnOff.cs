using System;

namespace OzricEngine.logic
{
    public sealed class OnOff: Value, IEquatable<OnOff>
    {
        public override ValueType ValueType => ValueType.OnOff;

        public bool value { get; }

        public OnOff()
        {
        }

        public OnOff(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value ? "On" : "Off";
        }

        public bool Equals(OnOff other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OnOff other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}