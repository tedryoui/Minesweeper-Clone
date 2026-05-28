using System;
using NUnit.Framework;

namespace _.Scripts.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TimeFieldAttribute : PropertyAttribute
    {
        public enum TimeFieldType { Hours, Minutes, Seconds }

        private TimeFieldType[] _timeFieldTypes;

        public TimeFieldAttribute(params TimeFieldType[] timeFieldTypes)
        {
            _timeFieldTypes = timeFieldTypes;
        }
    }
}