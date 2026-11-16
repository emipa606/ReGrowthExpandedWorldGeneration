using System;

namespace RGExpandedWorldGeneration;

public static class AxialTiltUtility
{
    public static int EnumValuesCount
    {
        get
        {
            if (field < 0)
            {
                field = Enum.GetNames(typeof(AxialTilt)).Length;
            }

            return field;
        }
    } = -1;
}