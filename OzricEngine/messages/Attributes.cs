using System;
using System.Collections;
using System.Collections.Generic;

namespace OzricEngine
{
    public class Attributes: Dictionary<string, object>, IEquatable<Attributes>
    {
        public static bool operator ==(Attributes lhs, Attributes rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(Attributes lhs, Attributes rhs) => !(lhs == rhs);

        public bool Equals(Attributes other)
        {
            if (Count != other.Count) return false;

            foreach (var kvp in this)
            {
                var key = kvp.Key;
                if (!other.TryGetValue(key, out var otherValue))
                    return false;

                var value = kvp.Value;
                if (!AreSameValue(value, otherValue))
                    return false;
            }
            return true;
        }

        private bool AreSameValue(object value, object otherValue)
        {
            if (value == null)
                return otherValue == null;

            if (value.Equals(otherValue))
                return true;

            return AreSameList(value, otherValue);
        }
        
        private bool AreSameList(object value1, object value2)
        {
            if (!(value1 is IEnumerable list1))
                return false;

            if (!(value2 is IEnumerable list2))
                return false;

            var values1 = list1.GetEnumerator();
            var values2 = list2.GetEnumerator();

            var next1 = values1.MoveNext();
            var next2 = values2.MoveNext();

            while (next1 && next2)
            {
                if (!AreSameValue(values1.Current, values2.Current))
                    return false;
                
                next1 = values1.MoveNext();
                next2 = values2.MoveNext();
            }

            return (next1 == next2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Attributes)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}