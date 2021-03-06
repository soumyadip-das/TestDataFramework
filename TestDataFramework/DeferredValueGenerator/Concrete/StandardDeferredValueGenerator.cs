﻿/*
    Copyright 2016 Alexander Kuperman

    This file is part of TestDataFramework.

    TestDataFramework is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    TestDataFramework is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with TestDataFramework.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using TestDataFramework.DeferredValueGenerator.Interfaces;
using TestDataFramework.Helpers;
using TestDataFramework.Populator;

namespace TestDataFramework.DeferredValueGenerator.Concrete
{
    public class StandardDeferredValueGenerator<T> : IDeferredValueGenerator<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (StandardDeferredValueGenerator<T>));

        private readonly IPropertyDataGenerator<T> dataSource;
        private readonly Dictionary<PropertyInfo, Data<T>> propertyDataDictionary = new Dictionary<PropertyInfo, Data<T>>();

        public StandardDeferredValueGenerator(IPropertyDataGenerator<T> dataSource)
        {
            StandardDeferredValueGenerator<T>.Logger.Debug($"Entering constructor. T: {typeof(T)}");

            this.dataSource = dataSource;

            StandardDeferredValueGenerator<T>.Logger.Debug("Exiting constructor");
        }

        public void AddDelegate(PropertyInfo targetPropertyInfo, DeferredValueGetterDelegate<T> valueGetter)
        {
            StandardDeferredValueGenerator<T>.Logger.Debug(
                $"Entering AddDelegate. targetPropertyInfo: {targetPropertyInfo.GetExtendedMemberInfoString()}");

            if (this.propertyDataDictionary.ContainsKey(targetPropertyInfo))
            {
                StandardDeferredValueGenerator<T>.Logger.Debug("AddDelegate. Duplicate property. Exiting");

                return;
            }

            this.propertyDataDictionary.Add(targetPropertyInfo, new Data<T>(valueGetter));

            StandardDeferredValueGenerator<T>.Logger.Debug("Exiting AddDelegate");
        }

        public void Execute(IEnumerable<RecordReference> targets)
        {
            StandardDeferredValueGenerator<T>.Logger.Debug("Entering Execute");

            this.dataSource.FillData(this.propertyDataDictionary);

            IEnumerable<RecordReference> uniqueTargets = StandardDeferredValueGenerator<T>.GetUniqueTargets(targets);

            uniqueTargets.ToList().ForEach(targetRecordReference =>
            {
                StandardDeferredValueGenerator<T>.Logger.Debug("Target object type: " + targetRecordReference.RecordType);

                targetRecordReference.RecordType.GetPropertiesHelper().ToList().ForEach(propertyInfo =>
                {
                    StandardDeferredValueGenerator<T>.Logger.Debug("Property: " + propertyInfo.GetExtendedMemberInfoString());

                    if (targetRecordReference.IsExplicitlySet(propertyInfo))
                    {
                        StandardDeferredValueGenerator<T>.Logger.Debug(
                            "Property explicitly set. Continuing to next iteration.");

                        return;
                    }

                    Data<T> data;
                    if (!this.propertyDataDictionary.TryGetValue(propertyInfo, out data))
                    {
                        StandardDeferredValueGenerator<T>.Logger.Debug(
                            "Property not in deferred properties dictionary. Continuing to next iteration.");

                        return;
                    }

                    StandardDeferredValueGenerator<T>.Logger.Debug($"Property found in deferred properties dictionary. Data: {data}");

                    object value = data.ValueGetter(data.Item);

                    value = value ?? Helper.GetDefaultValue(propertyInfo.PropertyType);

                    propertyInfo.SetValue(targetRecordReference.RecordObject, value);
                });
            });

            this.propertyDataDictionary.Clear();

            StandardDeferredValueGenerator<T>.Logger.Debug("Exiting Execute");
        }

        private static IEnumerable<RecordReference> GetUniqueTargets(IEnumerable<RecordReference> targets)
        {
            targets = targets.ToList();

            IEnumerable<RecordReference> distinctReferenceTypes =
                targets.Where(t => !t.RecordObject.GetType().IsValueType)
                    .Distinct(StandardDeferredValueGenerator<T>.ReferenceRecordObjectEqualityComparerObject);

            IEnumerable<RecordReference> valueTypes = targets.Where(t => t.GetType().IsValueType);

            IEnumerable<RecordReference> result = distinctReferenceTypes.Concat(valueTypes);

            return result;
        }

        private static readonly ReferenceRecordObjectEqualityComparer ReferenceRecordObjectEqualityComparerObject =
            new ReferenceRecordObjectEqualityComparer();

        private class ReferenceRecordObjectEqualityComparer : IEqualityComparer<RecordReference>
        {
            public bool Equals(RecordReference x, RecordReference y)
            {
                return x.RecordObject == y.RecordObject;
            }

            public int GetHashCode(RecordReference obj)
            {
                return obj.RecordObject.GetHashCode();
            }
        }

    }
}
