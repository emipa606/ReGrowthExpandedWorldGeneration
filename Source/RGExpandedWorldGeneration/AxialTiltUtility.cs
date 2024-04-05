using System;

namespace RGExpandedWorldGeneration;

public static class AxialTiltUtility
{
    private static int cachedEnumValuesCount = -1;

    public static int EnumValuesCount
    {
        get
        {
            if (cachedEnumValuesCount < 0)
            {
                cachedEnumValuesCount = Enum.GetNames(typeof(AxialTilt)).Length;
            }

            return cachedEnumValuesCount;
        }
    }
}