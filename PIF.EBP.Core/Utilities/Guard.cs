using PIF.EBP.Core.Exceptions;
using System;

namespace PIF.EBP.Core.Utilities
{
    public static class Guard
    {
        public static void AssertArgumentNotLessThanOrEqualToZero(int? value, string argumentName = null)
        {
            if (value == null)
                throw new ArgumentNullException(argumentName);

            if (value <= 0)
                throw new ArgumentException("Value cannot be less than or equal to zero.", argumentName);
        }

        public static void AssertArgumentNotLessThanOrEqualToZero(long? value, string argumentName = null)
        {
            if (value == null)
                throw new ArgumentNullException(argumentName);

            if (value <= 0)
                throw new ArgumentException("Value cannot be less than or equal to zero.", argumentName);
        }

        public static void AssertArgumentNotLessThanOrEqualToZero(decimal? value, string argumentName = null)
        {
            if (value == null)
                throw new ArgumentNullException(argumentName);

            if (value <= 0)
                throw new ArgumentException("Value cannot be less than or equal to zero.", argumentName);
        }

        public static void AssertArgumentEqualToZero(int value, string argumentName = null)
        {
            if (value != 0)
                throw new ArgumentException("Value must be equal to zero.", argumentName);
        }

        public static void AssertArgumentEqualToZero(long value, string argumentName = null)
        {
            if (value != 0)
                throw new ArgumentException("Value must be equal to zero.", argumentName);
        }

        public static void AssertArgumentNotNull(object value, string messageKey = null)
        {
            if (value == null)
            {
                throw new UserFriendlyException("NullArgument");
            }
        }

        public static void AssertArgumentBetween<T>(T firstValue, T secondValue, T value, string argumentName = null) where T : IComparable
        {
            if (firstValue.CompareTo(secondValue) > 0 || firstValue.CompareTo(value) > 0 || secondValue.CompareTo(value) < 0)
            {
                throw new ArgumentOutOfRangeException($"Value is not in range {firstValue} - {secondValue} value .", argumentName);
            }
        }


        public static void AssertValueIsCanBeConvertedToEnum<T>(int value, string argumentName = null) where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentOutOfRangeException($"Value is not in range of {typeof(T).Name} enum .", argumentName);
            }
        }

        public static void AssertStringIsNotNullOrEmpty(string value, string argumentName = null)
        {
            AssertArgumentNotNull(value, argumentName);

            if(value == "")
            {
                throw new ArgumentException($"String is Empty: {argumentName}");
            }
        }
    }
}
