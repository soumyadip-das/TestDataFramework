﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Windsor.Diagnostics.Inspectors;
using log4net;
using TestDataFramework.Exceptions;
using TestDataFramework.Persistence;
using TestDataFramework.TypeGenerator;
using TestDataFramework.ValueGenerator;

namespace TestDataFramework.Populator
{
    public class SetExpression<T>
    {
        public Expression<Func<T, object>> FieldExpression { get; set; }
        public object Value { get; set; }
    }

    public class StandardPopulator : IPopulator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (StandardPopulator));

        private readonly ITypeGenerator typeGenerator;
        private readonly IPersistence persistence;

        private readonly List<RecordReference> recordReferences = new List<RecordReference>();

        public StandardPopulator(ITypeGenerator typeGenerator, IPersistence persistence)
        {
            StandardPopulator.Logger.Debug("Entering constructor");

            this.typeGenerator = typeGenerator;
            this.persistence = persistence;

            StandardPopulator.Logger.Debug("Entering constructor");
        }

        #region Public Methods

        public virtual IList<RecordReference<T>> Add<T>(int copies, RecordReference primaryRecordReference = null) where T : new()
        {
            var result = new List<RecordReference<T>>();

            for (int i = 0; i < copies; i++)
            {
                result.Add(this.Add<T>(primaryRecordReference));
            }

            return result;
        }

        public virtual RecordReference<T> Add<T>(RecordReference primaryRecordReference = null) where T : new()
        {
            StandardPopulator.Logger.Debug("Entering Add<T>(primaryRecordReference, propertyExpressionDictionary)");

            StandardPopulator.Logger.Debug("Adding type " + typeof(T));

            var recordReference = new RecordReference<T>(this.typeGenerator);

            this.recordReferences.Add(recordReference);

            if (primaryRecordReference != null)
            {
                recordReference.AddPrimaryRecordReference(primaryRecordReference);
            }

            StandardPopulator.Logger.Debug("Exiting Add<T>(primaryRecordReference, propertyExpressionDictionary)");

            return recordReference;
        }

        public virtual void Bind()
        {
            StandardPopulator.Logger.Debug("Entering Populate");

            foreach (RecordReference recordReference in this.recordReferences)
            {
                this.typeGenerator.ResetRecursionGuard();
                recordReference.Populate();
            }

            this.persistence.Persist(this.recordReferences);
            this.recordReferences.Clear();

            StandardPopulator.Logger.Debug("Exiting Populate");
        }

        #endregion Public Methods

        #region Helpers

        private static void UpdateSetterDictionary<T>(
            ConcurrentDictionary<PropertyInfo, Action<T>> propertyExpressionDictionary,
            SetExpression<T> setExpression)
        {
            var memberExpression = setExpression.FieldExpression.Body as MemberExpression;

            if (memberExpression == null)
            {
                var unaryExpression = setExpression.FieldExpression.Body as UnaryExpression;

                if (unaryExpression == null)
                {
                    throw new SetExpressionException(Messages.MustBePropertyAccess);
                }

                memberExpression = unaryExpression.Operand as MemberExpression;

                if (memberExpression == null)
                {
                    throw new SetExpressionException(Messages.MustBePropertyAccess);
                }
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new SetExpressionException(Messages.MustBePropertyAccess);
            }

            if (propertyInfo.GetSetMethod() == null)
            {
                throw new SetExpressionException(Messages.NoSetter);
            }

            Action<T> setter = @object => propertyInfo.SetValue(@object, setExpression.Value);

            propertyExpressionDictionary.AddOrUpdate(propertyInfo, setter, (pi, lambda) => setter);
        }

        #endregion Helpers

    }
}