using UnityEngine;

public class EnumFlagAttribute : PropertyAttribute
{
    public int max;
    public EnumFlagAttribute(int max = 3) { this.max = max; }
}