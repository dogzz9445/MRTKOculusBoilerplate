using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardSystem
{
    public enum MagicType
    {
        Circle,
        Square,
        Star,
        Triangle
    }

    public static class MagicTypeConverter
    {
        public static MagicType GetMagicType(int iType)
        {
            switch (iType)
            {
                default:
                case 0:
                    return MagicType.Circle;
                case 1:
                    return MagicType.Square;
                case 2:
                    return MagicType.Star;
                case 3:
                    return MagicType.Triangle;
            }
        }

        public static string GetMagicName(MagicType type)
        {
            switch (type)
            {
                default:
                case MagicType.Circle:
                    return "Circle";
                case MagicType.Square:
                    return "Square";
                case MagicType.Star:
                    return "Star";
                case MagicType.Triangle:
                    return "Triangle";
            }
        }

    }
}
