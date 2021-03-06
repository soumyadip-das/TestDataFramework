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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using log4net;
using TestDataFramework.AttributeDecorator;
using TestDataFramework.HandledTypeGenerator;
using TestDataFramework.ListOperations;
using TestDataFramework.Persistence.Interfaces;
using TestDataFramework.Populator.Interfaces;
using TestDataFramework.TypeGenerator.Interfaces;
using TestDataFramework.ValueGenerator.Interfaces;

namespace TestDataFramework.Populator.Concrete
{
    public class StandardPopulator : BasePopulator, IPopulator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (StandardPopulator));

        private readonly ITypeGenerator typeGenerator;
        private readonly IPersistence persistence;
        private readonly IHandledTypeGenerator handledTypeGenerator;
        private readonly ValueGuaranteePopulator valueGuaranteePopulator;

        public IValueGenerator ValueGenerator { get; }

        private readonly List<RecordReference> recordReferences = new List<RecordReference>();
        private readonly List<OperableList> setOfLists = new List<OperableList>();

        public StandardPopulator(ITypeGenerator typeGenerator, IPersistence persistence,
            IAttributeDecorator attributeDecorator, IHandledTypeGenerator handledTypeGenerator, 
            IValueGenerator valueGenerator, ValueGuaranteePopulator valueGuaranteePopulator)
            : base(attributeDecorator)
        {
            StandardPopulator.Logger.Debug("Entering constructor");

            this.typeGenerator = typeGenerator;
            this.persistence = persistence;
            this.handledTypeGenerator = handledTypeGenerator;
            this.ValueGenerator = valueGenerator;
            this.valueGuaranteePopulator = valueGuaranteePopulator;

            StandardPopulator.Logger.Debug("Entering constructor");
        }

        #region Public Methods

        public virtual void Extend(Type type, HandledTypeValueGetter valueGetter)
        {
            this.handledTypeGenerator.HandledTypeValueGetterDictionary.Add(type, valueGetter);
        }

        public virtual OperableList<T> Add<T>(int copies, params RecordReference[] primaryRecordReferences)
        {
            StandardPopulator.Logger.Debug($"Entering Add. T: {typeof(T)}, copies: {copies}, primaryRecordReference: {primaryRecordReferences}");

            var result = new OperableList<T>(this.valueGuaranteePopulator);
            this.setOfLists.Add(result);

            for (int i = 0; i < copies; i++)
            {
                result.Add(this.Add<T>(primaryRecordReferences));
            }

            StandardPopulator.Logger.Debug("Exiting Add");
            return result;
        }

        public virtual RecordReference<T> Add<T>(params RecordReference[] primaryRecordReferences)
        {
            StandardPopulator.Logger.Debug($"Entering Add. T: {typeof(T)}, primaryRecordReference: {primaryRecordReferences}");

            var recordReference = new RecordReference<T>(this.typeGenerator, this.AttributeDecorator);

            this.recordReferences.Add(recordReference);
            recordReference.AddPrimaryRecordReference(primaryRecordReferences);

            StandardPopulator.Logger.Debug("Exiting Add<T>(primaryRecordReference, propertyExpressionDictionary)");

            StandardPopulator.Logger.Debug($"Exiting Add. record object: {recordReference.RecordObject}");
            return recordReference;
        }

        public virtual void Bind()
        {
            StandardPopulator.Logger.Debug("Entering Populate");

            foreach (OperableList list in this.setOfLists)
            {
                list.Bind();
            }

            foreach (RecordReference recordReference in this.recordReferences)
            {
                if (recordReference.PreBoundObject != null)
                {
                    recordReference.RecordObject = recordReference.PreBoundObject();
                    continue;
                }

                recordReference.Populate();
            }

            this.persistence.Persist(this.recordReferences);
            this.recordReferences.Clear();

            StandardPopulator.Logger.Debug("Exiting Populate");
        }

        #endregion Public Methods
    }
}