﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestDataFramework.DeferredValueGenerator.Concrete;
using TestDataFramework.DeferredValueGenerator.Interfaces;
using TestDataFramework.Exceptions;
using Tests.TestModels;

namespace Tests.Tests
{
    [TestClass]
    public class StandardDeferredValueGeneratorTests
    {
        [TestMethod]
        public void DeferredValueGeneratorExecutedException_Test()
        {
            // Arrange

            var generator = new StandardDeferredValueGenerator<ulong>(new Mock<IPropertyDataGenerator<ulong>>().Object);

            // Act. Assert.

            generator.Execute(Enumerable.Empty<object>());

            Helpers.ExceptionTest(() => generator.Execute(Enumerable.Empty<object>()),
                typeof(DeferredValueGeneratorExecutedException), Messages.DeferredValueGeneratorExecuted);

            Helpers.ExceptionTest(() => generator.Execute(Enumerable.Empty<object>()),
                typeof(DeferredValueGeneratorExecutedException), Messages.DeferredValueGeneratorExecuted);
        }

        [TestMethod]
        public void DeferredValueGenerator_Test()
        {
            // Arrange

            var object1 = new PrimaryTable();
            var object2 = new ForeignTable();

            var dbProvider = new Mock<IPropertyDataGenerator<ulong>>();
            var generator = new StandardDeferredValueGenerator<ulong>(dbProvider.Object);

            // Act

            generator.AddDelegate(object1.GetType().GetProperty("Text"), ul => "A");
            generator.AddDelegate(object1.GetType().GetProperty("Integer"), ul => 1);
            generator.AddDelegate(object2.GetType().GetProperty("Text"), ul => "B");
            generator.AddDelegate(object2.GetType().GetProperty("Integer"), ul => 2);

            generator.Execute(new object[] {object2, object1});

            // Assert

            Assert.AreEqual("A", object1.Text);
            Assert.AreEqual(1, object1.Integer);
            Assert.AreEqual("B", object2.Text);
            Assert.AreEqual(2, object2.Integer);
        }
    }
}