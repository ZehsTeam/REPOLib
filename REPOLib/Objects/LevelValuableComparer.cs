using System.Collections.Generic;

namespace REPOLib.Objects;

public class LevelValuableComparer : IEqualityComparer<LevelValuables>
{
    public bool Equals(LevelValuables x, LevelValuables y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.name == y.name;
    }

    public int GetHashCode(LevelValuables obj)
    {
        return obj.name?.GetHashCode() ?? 0;
    }
}
