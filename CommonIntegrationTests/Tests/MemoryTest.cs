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
using CommonIntegrationTests.TestModels;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDataFramework.Exceptions;
using TestDataFramework.Factories;
using TestDataFramework.Populator.Concrete;
using TestDataFramework.Populator.Interfaces;

namespace CommonIntegrationTests.Tests
{
    [Ignore]
    [TestClass]
    public class MemoryTest
    {
        private PopulatorFactory factory;

        [TestInitialize]
        public void Initialize()
        {
            XmlConfigurator.Configure();

            this.factory = new PopulatorFactory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.factory.Dispose();
        }

        [TestMethod]
        public void Test()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            IList<RecordReference<SubjectClass>> subjectReference = populator.Add<SubjectClass>(2);
            RecordReference<ForeignSubjectClass> foreignReference = populator.Add<ForeignSubjectClass>(subjectReference[1]);
            populator.Bind();

            Helpers.Dump(subjectReference[0].RecordObject);
            Helpers.Dump(subjectReference[1].RecordObject);
            Helpers.Dump(foreignReference.RecordObject);
        }

        // This is a test of value types in general.
        [TestMethod]
        public void Multiple_KeyValuePair_Test()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            IList<RecordReference<KeyValuePair<SubjectClass, ForeignSubjectClass>>> kvpRefs =
                populator.Add<KeyValuePair<SubjectClass, ForeignSubjectClass>>(10);

            populator.Bind();

            Helpers.Dump(kvpRefs.First().RecordObject.Key);
            Helpers.Dump(kvpRefs.First().RecordObject.Value);
            kvpRefs.ToList().ForEach(r => Helpers.Dump(r.RecordObject));
        }

        [TestMethod]
        public void Dictionary_UniqueValueTypeKeys_Test()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            RecordReference<ClassWithHandledTypes> recordReference = populator.Add<ClassWithHandledTypes>();
            populator.Bind();

            IDictionary<KeyValuePair<int, string>, object> dictionary = recordReference.RecordObject.ADictionary;

            foreach (KeyValuePair<KeyValuePair<int, string>, object> item in dictionary)
            {
                Console.WriteLine(item.Key);
            }
        }

        [TestMethod]
        public void GauranteedValueTest()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            IList<RecordReference<SubjectClass>> subjectReference = populator.Add<SubjectClass>(2);
            OperableList<ForeignSubjectClass> foreignReference =
                populator.Add<ForeignSubjectClass>(20, subjectReference[1])
                    .GuaranteeByPercentageOfTotal(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 777},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = subjectReference[1].RecordObject.IntegerWithMax}),
                        new ForeignSubjectClass {SecondInteger = 999},
                    }, 50)
                    .GuaranteeByFixedQuantity(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 111},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = subjectReference[0].RecordObject.IntegerWithMax}),
                        new ForeignSubjectClass {SecondInteger = 222},
                    }, 5);

            populator.Bind();

            Console.WriteLine("SubjectClass[1].IntegerWithMax: " + subjectReference[1].RecordObject.IntegerWithMax);
            Console.WriteLine("SubjectClass[0].IntegerWithMax: " + subjectReference[0].RecordObject.IntegerWithMax);
            int i = 1;
            foreignReference.ToList().ForEach(r => Console.WriteLine(i++ + ".\r\n" + r.RecordObject.ToString()));
        }

        [TestMethod]
        public void GauranteedValue_FixedQuantity_Too_Large()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            OperableList<ForeignSubjectClass> foreignReference =
                populator.Add<ForeignSubjectClass>(20)
                    .GuaranteeByFixedQuantity(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 111},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = 222}),
                        new ForeignSubjectClass {SecondInteger = 333},
                    }, 21);

            global::Tests.Helpers.ExceptionTest(() => populator.Bind(), typeof(ValueGuaranteeException),
                Messages.TooFewReferencesForValueGuarantee);
        }

        [TestMethod]
        public void GauranteedValue_Percentage_Too_Large()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

            OperableList<ForeignSubjectClass> foreignReference =
                populator.Add<ForeignSubjectClass>(20)
                    .GuaranteeByPercentageOfTotal(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 111},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = 222}),
                        new ForeignSubjectClass {SecondInteger = 333},
                    }, 105);

            global::Tests.Helpers.ExceptionTest(() => populator.Bind(), typeof(ValueGuaranteeException),
                Messages.TooFewReferencesForValueGuarantee);
        }

        [TestMethod]
        public void GauranteedValue_FixedQuantity_Plus_Percentage_Too_Large()
        {
            IPopulator populator = this.factory.CreateMemoryPopulator();

                populator.Add<ForeignSubjectClass>(20)
                    .GuaranteeByPercentageOfTotal(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 777},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = 888}),
                        new ForeignSubjectClass {SecondInteger = 999},
                    }, 80)
                    .GuaranteeByFixedQuantity(new object[]
                    {
                        new ForeignSubjectClass {SecondInteger = 111},
                        (Func<ForeignSubjectClass>)
                        (() => new ForeignSubjectClass {SecondInteger = 222}),
                        new ForeignSubjectClass {SecondInteger = 333},
                    }, 5);

            global::Tests.Helpers.ExceptionTest(() => populator.Bind(), typeof(ValueGuaranteeException),
                Messages.TooFewReferencesForValueGuarantee);
        }

        private class ClassWithDictionary
        {
            public Dictionary<string, string> ADictionary { get; set; }
        }

        [TestMethod]
        public void Dictionary_Test()
        {
            IPopulator populator = StaticPopulatorFactory.CreateMemoryPopulator();
            populator.Add<ClassWithDictionary>();
            populator.Bind();
        }

        [TestMethod]
        public void GuaranteedValueAndExplicitSetting_Test()
        {
            IPopulator populator = StaticPopulatorFactory.CreateMemoryPopulator();

            OperableList<SubjectClass> subjectReferences = populator.Add<SubjectClass>(4)
                .GuaranteeByPercentageOfTotal(
                    new[] {new SubjectClass {AnEmailAddress = "myemailAddress@here.com", Text = "Guaranteed Text"}}, 100);

            subjectReferences[1].Set(p => p.Text, "Hello");

            populator.Bind();

            subjectReferences.ToList()
                .ForEach(
                    reference =>
                        Console.WriteLine(reference.RecordObject.AnEmailAddress + "\r\n" + reference.RecordObject.Text +
                                          "\r\n"));
        }
    }
}
