using System;
using System.ComponentModel;
using System.Reflection;

namespace Scada.Extend
{
    //public static class EnumHelper
    //{
    //    public static string GetDescription(this Enum value)
    //    {
    //        var customAttribute = GetAttribute<DescriptionAttribute>(value);
    //        if (customAttribute != null && !string.IsNullOrEmpty(customAttribute.Description))
    //        {
    //            return customAttribute.Description;
    //        }

    //        return null;
    //    }

    //    public static T GetAttribute<T>(this Enum value) where T : Attribute
    //    {
    //        if (value == null)
    //        {
    //            return null;
    //        }

    //        FieldInfo field = value.GetType().GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
    //        if (field == null)
    //        {
    //            return null;
    //        }

    //        T customAttribute = field.GetCustomAttribute<T>(inherit: false);
            
    //        return customAttribute;
    //    }

    //}
}
