using System;

namespace OzricEngine.logic
{
    public sealed class Scalar: Value, IEquatable<Scalar>
    {
        public float value { get; }

        public Scalar()
        {
        }
        
        public Scalar(float value = 0.0f)
        {
            this.value = value;
        }

        public bool Equals(Scalar other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Scalar other && Equals(other);
        }
    }
}