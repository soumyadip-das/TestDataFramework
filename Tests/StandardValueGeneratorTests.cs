﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestDataFramework;
using TestDataFramework.Exceptions;
using TestDataFramework.Randomizer;
using TestDataFramework.TypeGenerator;
using TestDataFramework.ValueGenerator;

namespace Tests
{
    [TestClass]
    public class StandardValueGeneratorTests
    {
        private Mock<IRandomizer> randomizerMock;
        private Mock<ITypeGenerator> typeGeneratorMock;
        private StandardValueGenerator valueGenerator;

        private const int IntegerResult = 5;
        private const long LongResult = 6;
        private const short ShortResult = 7;
        private static readonly string StringResult = Guid.NewGuid().ToString("N");
        private const char CharacterResult = 'A';
        private const decimal DecimalResult = 48576.412587m;
        private const bool BooleanResult = true;
        private static readonly DateTime DateTimeResult = DateTime.Now;
        private const byte ByteResult = 8;
        private const double DoubleResult = 574.1575d;
        private const string EmailAddress = "address@domain.com";

        [TestInitialize]
        public void Initialize()
        {
            this.randomizerMock = new Mock<IRandomizer>();
            this.typeGeneratorMock = new Mock<ITypeGenerator>();

            this.randomizerMock.Setup(m => m.RandomizeInteger()).Returns(StandardValueGeneratorTests.IntegerResult);
            this.randomizerMock.Setup(m => m.RandomizeLongInteger()).Returns(StandardValueGeneratorTests.LongResult);
            this.randomizerMock.Setup(m => m.RandomizeShortInteger()).Returns(StandardValueGeneratorTests.ShortResult);
            this.randomizerMock.Setup(m => m.RandomizeString(It.Is<int?>(length => length == null))).Returns(StandardValueGeneratorTests.StringResult);
            this.randomizerMock.Setup(m => m.RandomizeCharacter()).Returns(StandardValueGeneratorTests.CharacterResult);
            this.randomizerMock.Setup(m => m.RandomizeDecimal(It.Is<int?>(precision => precision == null))).Returns(StandardValueGeneratorTests.DecimalResult);
            this.randomizerMock.Setup(m => m.RandomizeBoolean()).Returns(StandardValueGeneratorTests.BooleanResult);
            this.randomizerMock.Setup(m => m.RandomizeDateTime()).Returns(StandardValueGeneratorTests.DateTimeResult);
            this.randomizerMock.Setup(m => m.RandomizeByte()).Returns(StandardValueGeneratorTests.ByteResult);
            this.randomizerMock.Setup(m => m.RandomizeDouble(It.Is<int?>(precision => precision == null))).Returns(StandardValueGeneratorTests.DoubleResult);
            this.randomizerMock.Setup(m => m.RandomizeEmailAddress()).Returns(StandardValueGeneratorTests.EmailAddress);

            this.valueGenerator = new StandardValueGenerator(this.randomizerMock.Object, this.typeGeneratorMock.Object);
        }

        [TestMethod]
        public void AllTypeTests()
        {
            var list = new List<Tuple<string, object>>
            {
                new Tuple<string, object>("Integer", StandardValueGeneratorTests.IntegerResult),
                new Tuple<string, object>("LongInteger", StandardValueGeneratorTests.LongResult),
                new Tuple<string, object>("ShortInteger", StandardValueGeneratorTests.ShortResult),
                new Tuple<string, object>("Text", StandardValueGeneratorTests.StringResult),
                new Tuple<string, object>("Character", StandardValueGeneratorTests.CharacterResult),
                new Tuple<string, object>("Decimal", StandardValueGeneratorTests.DecimalResult),
                new Tuple<string, object>("Boolean", StandardValueGeneratorTests.BooleanResult),
                new Tuple<string, object>("DateTime", StandardValueGeneratorTests.DateTimeResult),
                new Tuple<string, object>("Byte", StandardValueGeneratorTests.ByteResult),
                new Tuple<string, object>("Double", StandardValueGeneratorTests.DoubleResult),
                new Tuple<string, object>("NullableInteger", StandardValueGeneratorTests.IntegerResult),
                new Tuple<string, object>("AnEmailAddress", StandardValueGeneratorTests.EmailAddress),
            };

            list.ForEach(type => this.TypeTest(type.Item1, type.Item2));
        }

        private void TypeTest(string propertyName, object expectedResult)
        {
            Console.WriteLine("Executing for " + propertyName);

            // Arrange

            PropertyInfo propertyInfo = typeof (SubjectClass).GetProperty(propertyName);

            // Act

            object result = this.valueGenerator.GetValue(propertyInfo);

            // Assert

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void GetValue_StringLengthAttribute_Test()
        {
            // Arrange

            PropertyInfo propertyInfo = typeof(SubjectClass).GetProperty("TextWithLength");

            this.randomizerMock.Setup(m => m.RandomizeString(It.Is<int?>(length => length == SubjectClass.StringLength))).Verifiable();

            // Act

            object result = this.valueGenerator.GetValue(propertyInfo);

            // Assert

            this.randomizerMock.Verify();
        }

        [TestMethod]
        public void GetValue_PrecisionAttribute_Test()
        {
            // Arrange

            var propertyNameAnVerifierList = new List<Tuple<string, Action>>
            {
                new Tuple<string, Action>(
                    "DecimalWithPrecision",
                    () =>
                        this.randomizerMock.Verify(
                            m => m.RandomizeDecimal(It.Is<int?>(precision => precision == SubjectClass.Precision)),
                            Times.Once())
                ),
                new Tuple<string, Action>(
                    "DoubleWithPrecision",
                    () =>
                        this.randomizerMock.Verify(
                            m => m.RandomizeDouble(It.Is<int?>(precision => precision == SubjectClass.Precision)),
                            Times.Once())
                ),
            };

            // Act and Assert

            propertyNameAnVerifierList.ForEach(propertyNameVerifier =>
            {
                PropertyInfo propertyInfo = typeof (SubjectClass).GetProperty(propertyNameVerifier.Item1);
                this.valueGenerator.GetValue(propertyInfo);
                propertyNameVerifier.Item2();
            });            
        }

        [TestMethod]
        public void GetValue_ComplexObject()
        {
            // Arrange

            var secondClass = new SecondClass();

            this.typeGeneratorMock.Setup(m => m.GetObject(It.Is<Type>(t => t == typeof (SecondClass)))).Returns(secondClass);
            PropertyInfo propertyInfo = typeof (SubjectClass).GetProperty("SecondObject");

            // Act

            object result = this.valueGenerator.GetValue(propertyInfo);
            this.typeGeneratorMock.Verify();

            // Assert

            var secondObject = result as SecondClass;
            Assert.IsNotNull(secondObject);
        }
    }
}
