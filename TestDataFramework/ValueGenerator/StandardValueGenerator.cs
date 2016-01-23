﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using log4net;
using TestDataFramework.ArrayRandomizer;
using TestDataFramework.Exceptions;
using TestDataFramework.Helpers;
using TestDataFramework.Randomizer;
using TestDataFramework.TypeGenerator;
using TestDataFramework.UniqueValueGenerator;
using TestDataFramework.UniqueValueGenerator.Interface;

namespace TestDataFramework.ValueGenerator
{
    public class StandardValueGenerator : IValueGenerator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(StandardValueGenerator));

        private readonly IRandomizer randomizer;
        private readonly ITypeGenerator typeGenerator;
        private readonly IArrayRandomizer arrayRandomizer;
        private readonly IUniqueValueGenerator uniqueValueGenerator;

        private delegate object GetValueForTypeDelegate(PropertyInfo propertyInfo);

        private readonly Dictionary<Type, GetValueForTypeDelegate> typeValueGetterDictionary;

        public StandardValueGenerator(IRandomizer randomizer, ITypeGenerator typeGenerator,
            Func<IValueGenerator, IArrayRandomizer> getArrayRandomizer, IUniqueValueGenerator uniqueValueGenerator)
        {
            StandardValueGenerator.Logger.Debug("Entering constructor");

            this.randomizer = randomizer;
            this.typeGenerator = typeGenerator;
            this.arrayRandomizer = getArrayRandomizer(this);
            this.uniqueValueGenerator = uniqueValueGenerator;

            this.typeValueGetterDictionary = new Dictionary<Type, GetValueForTypeDelegate>
            {
                {typeof (EmailAttribute), x => this.randomizer.RandomizeEmailAddress()},
                {typeof (PrimaryKeyAttribute), this.GetPrimaryKey},
                {typeof (string), this.GetString},
                {typeof (decimal), this.GetDecimal},
                {typeof (int), this.GetInteger},
                {typeof (long), this.GetLong},
                {typeof (short), this.GetShort},
                {typeof (bool), x => this.randomizer.RandomizeBoolean()},
                {typeof (char), x => this.randomizer.RandomizeCharacter()},
                {typeof (DateTime), this.GetDateTime},
                {typeof (byte), x => this.randomizer.RandomizeByte()},
                {typeof (double), this.GetDouble},
                {typeof (Guid), this.NoOp }
            };

            StandardValueGenerator.Logger.Debug("Exiting constructor");
        }

        // This is the general entry point.
        public virtual object GetValue(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetValue(PropertyInfo propertyInfo)");

            propertyInfo.IsNotNull(nameof(propertyInfo));

            GetValueForTypeDelegate getter = null;

            propertyInfo.GetCustomAttributesData()
                .Any(
                    attributeData =>
                        this.typeValueGetterDictionary.TryGetValue(attributeData.AttributeType, out getter));

            object result = getter != null ? getter(propertyInfo) : this.GetValue(propertyInfo, propertyInfo.PropertyType);

            StandardValueGenerator.Logger.Debug("Exiting GetValue(PropertyInfo propertyInfo)");
            return result;
        }

        // This entry point is used only when a different type is requested for a particular PropertyInfo
        public virtual object GetValue(PropertyInfo propertyInfo, Type type)
        {
            StandardValueGenerator.Logger.Debug("Entering GetValue(PropertyInfo propertyInfo, Type type)");

            propertyInfo.IsNotNull(nameof(propertyInfo));
            propertyInfo.IsNotNull(nameof(type));

            if (type.IsArray)
            {
                return this.arrayRandomizer.GetArray(propertyInfo, type);
            }

            Type forType = Nullable.GetUnderlyingType(type) ?? type;

            GetValueForTypeDelegate getter;

            object result = 
                this.typeValueGetterDictionary.TryGetValue(forType, out getter)
                ? getter(propertyInfo)
                : this.typeGenerator.GetObject(forType);

            StandardValueGenerator.Logger.Debug("Exiting GetValue(PropertyInfo propertyInfo, Type type)");
            return result;
        }

        #region Private Methods

        private bool TryGetGetter(PropertyInfo propertyInfo, out GetValueForTypeDelegate getValueForTypeDelegate)
        {
            StandardValueGenerator.Logger.Debug("Entering TryGetGetter");

            GetValueForTypeDelegate tempGetter = null;

            if (
                propertyInfo.GetCustomAttributesData()
                    .Any(
                        attributeData =>
                            this.typeValueGetterDictionary.TryGetValue(attributeData.AttributeType, out tempGetter)))
            {
                getValueForTypeDelegate = tempGetter;
                return true;
            }

            Type forType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            bool result = this.typeValueGetterDictionary.TryGetValue(forType, out getValueForTypeDelegate);

            StandardValueGenerator.Logger.Debug("Exiting TryGetGetter");
            return result;
        }

        private object NoOp(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsValueType
                ? Activator.CreateInstance(propertyInfo.PropertyType)
                : null;
        }

        private object GetString(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetString");

            var lengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
            int? length = lengthAttribute?.Length;

            string result = this.randomizer.RandomizeString(length);

            StandardValueGenerator.Logger.Debug("Exiting GetString");
            return result;
        }

        private object GetDecimal(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetDecimal");

            var precisionAttribute = propertyInfo.GetCustomAttribute<PrecisionAttribute>();
            int? precision = precisionAttribute?.Precision;

            decimal result = this.randomizer.RandomizeDecimal(precision);

            StandardValueGenerator.Logger.Debug("Exiting GetDecimal");
            return result;
        }

        private object GetDouble(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetDouble");

            var precisionAttribute = propertyInfo.GetCustomAttribute<PrecisionAttribute>();
            int? precision = precisionAttribute?.Precision;

            double result = this.randomizer.RandomizeDouble(precision);

            StandardValueGenerator.Logger.Debug("Exiting GetDouble");
            return result;
        }

        private object GetInteger(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetInteger");

            var maxAttribute = propertyInfo.GetCustomAttribute<MaxAttribute>();
            long? max = maxAttribute?.Max;

            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(Messages.MaxAttributeLessThanZero, (Exception) null);
            }

            if (max > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(string.Format(Messages.MaxAttributeOutOfRange, "int"), (Exception) null);
            }

            int result = this.randomizer.RandomizeInteger((int?)max);

            StandardValueGenerator.Logger.Debug("Exiting GetInteger");
            return result;
        }

        private object GetLong(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetLong");

            var maxAttribute = propertyInfo.GetCustomAttribute<MaxAttribute>();
            long? max = maxAttribute?.Max;

            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(Messages.MaxAttributeLessThanZero, (Exception)null);
            }

            long result = this.randomizer.RandomizeLongInteger(max);

            StandardValueGenerator.Logger.Debug("Exiting GetLong");
            return result;
        }

        private object GetShort(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetShort");

            var maxAttribute = propertyInfo.GetCustomAttribute<MaxAttribute>();
            long? max = maxAttribute?.Max;

            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(Messages.MaxAttributeLessThanZero, (Exception)null);
            }

            if (max > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(string.Format(Messages.MaxAttributeOutOfRange, "short"), (Exception)null);
            }

            short result = this.randomizer.RandomizeShortInteger((short?)max);

            StandardValueGenerator.Logger.Debug("Exiting GetShort");
            return result;
        }

        private object GetDateTime(PropertyInfo propertyInfo)
        {
            StandardValueGenerator.Logger.Debug("Entering GetDateTime");

            var pastOrFutureAttribute = propertyInfo.GetCustomAttribute<PastOrFutureAttribute>();
            PastOrFuture? pastOrFuture = pastOrFutureAttribute?.PastOrFuture;

            DateTime result = this.randomizer.RandomizeDateTime((PastOrFuture?)pastOrFuture, this.randomizer.RandomizeLongInteger);

            StandardValueGenerator.Logger.Debug("Exiting GetDateTime");
            return result;
        }

        private object GetPrimaryKey(PropertyInfo propertyInfo)
        {
            object result = this.uniqueValueGenerator.GetValue(propertyInfo);
            return result;
        }

        #endregion Private Methods
    }
}
