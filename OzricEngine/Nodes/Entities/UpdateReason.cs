using System;
using System.Runtime.CompilerServices;

namespace OzricEngine.Nodes;

/// <summary>
/// Unify "checking values" to make debugging the reason an update is happening easier. 
/// </summary>

class UpdateReason
{
    public bool update;
    public string reason;

    public void CheckApprox(float v0, float v1, float epsilon, [CallerArgumentExpression("v0")] string? v0s = null, [CallerArgumentExpression("v1")] string? v1s = null)
    {
        if (!update && Math.Abs(v0 - v1) > epsilon)
        {
            update = true;
            reason = $"{v0s} ({v0:F2}) !~= {v1s} ({v1:F2}), ε={epsilon:F2}";
        }
    }

    public void CheckApprox(int v0, int v1, int epsilon, [CallerArgumentExpression("v0")] string? v0s = null, [CallerArgumentExpression("v1")] string? v1s = null)
    {
        if (!update && Math.Abs(v0 - v1) > epsilon)
        {
            update = true;
            reason = $"{v0s} ({v0}) !~= {v1s} ({v1}), ε={epsilon}";
        }
    }

    public void CheckEquals<T>(T v0, T v1, [CallerArgumentExpression("v0")] string? v0s = null, [CallerArgumentExpression("v1")] string? v1s = null)
    {
        if (!update && !Equals(v0, v1))
        {
            update = true;
            reason = $"{v0s} ({v0:F2}) != {v1s} ({v1:F2})";
        }
    }

    public bool Check(bool condition, [CallerArgumentExpression("condition")] string? conditionString = null)
    {
        if (!update && condition)
        {
            update = true;
            reason = conditionString ?? "?";
        }

        return update;
    }

    public void Set(string reason)
    {
        Check(true, reason);
    }
}