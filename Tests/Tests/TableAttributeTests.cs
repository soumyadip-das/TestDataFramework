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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDataFramework;
using TestDataFramework.Exceptions;

namespace Tests.Tests
{
    [TestClass]
    public class TableAttributeTests
    {
        [TestMethod]
        public void Catalogue_Schema_TableName_Constructor_Test()
        {
            const string catalogue = "catalogueABC";
            const string schema = "schemaABC";
            const string tableName = "tableNameABC";

            var tableAttribute = new TableAttribute(catalogue, schema, tableName);

            Assert.AreEqual(catalogue, tableAttribute.CatalogueName);
            Assert.AreEqual(schema, tableAttribute.Schema);
            Assert.AreEqual(tableName, tableAttribute.Name);

            Assert.AreEqual(catalogue.GetHashCode() ^ schema.GetHashCode() ^ tableName.GetHashCode(),
                tableAttribute.GetHashCode());
        }

        [TestMethod]
        public void Schema_TableName_Constructor_Test()
        {
            const string schema = "schemaABC";
            const string tableName = "tableNameABC";

            var tableAttribute = new TableAttribute(schema, tableName);

            Assert.AreEqual(null, tableAttribute.CatalogueName);
            Assert.AreEqual(schema, tableAttribute.Schema);
            Assert.AreEqual(tableName, tableAttribute.Name);

            Assert.AreEqual(schema.GetHashCode() ^ tableName.GetHashCode(),
                tableAttribute.GetHashCode());
        }

        [TestMethod]
        public void TableName_Constructor_Test()
        {
            const string tableName = "tableNameABC";

            var tableAttribute = new TableAttribute(tableName);

            Assert.AreEqual(null, tableAttribute.CatalogueName);
            Assert.AreEqual("dbo", tableAttribute.Schema);
            Assert.AreEqual(tableName, tableAttribute.Name);

            Assert.AreEqual("dbo".GetHashCode() ^ tableName.GetHashCode(),
                tableAttribute.GetHashCode());
        }

        [TestMethod]
        public void NullSchema_GetHashCode_Test()
        {
            const string tableName = "tableNameABC";

            var tableAttribute = new TableAttribute(null, tableName);

            Assert.AreEqual(null, tableAttribute.Schema);

            Assert.AreEqual(tableName.GetHashCode(), tableAttribute.GetHashCode());
        }

        [TestMethod]
        public void CatalogueAndNoSchemaException_Test()
        {
            const string catalogue = "catalogueABC";
            const string tableName = "tableNameABC";

            Helpers.ExceptionTest(() => new TableAttribute(catalogueName: catalogue, schema: null, name: tableName), typeof (TableAttributeException),
                string.Format(Messages.CatalogueAndNoSchema, catalogue, tableName));

        }

        [TestMethod]
        public void TableNameRequiredException_Test()
        {
            Helpers.ExceptionTest(() => new TableAttribute(null, null, name: null), typeof(ArgumentNullException),
                "Value cannot be null.\r\nParameter name: name");

            Helpers.ExceptionTest(() => new TableAttribute(null, name: null), typeof(ArgumentNullException),
                "Value cannot be null.\r\nParameter name: name");

            Helpers.ExceptionTest(() => new TableAttribute(name: null), typeof(ArgumentNullException),
                "Value cannot be null.\r\nParameter name: name");
        }

        #region Equals tests

        [TestMethod]
        public void CatalogueNull_SchemaNull_Equals_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: null, schema: null, name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: null, schema: null, name: "AA");

            Assert.IsTrue(tableAttribute1.Equals(tableAttribute2));
        }

        [TestMethod]
        public void CatalogueNull_SameSchema_Equals_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: null, schema: "BB", name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: null, schema: "BB", name: "AA");

            Assert.IsTrue(tableAttribute1.Equals(tableAttribute2));
        }

        [TestMethod]
        public void SameCatalogue_SameSchema_Equals_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: "CC", schema: "BB", name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: "CC", schema: "BB", name: "AA");

            Assert.IsTrue(tableAttribute1.Equals(tableAttribute2));
        }

        [TestMethod]
        public void CatalogueNull_SchemaNull_DifferentName_NotEqual_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: null, schema: null, name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: null, schema: null, name: "BB");

            Assert.IsFalse(tableAttribute1.Equals(tableAttribute2));
        }

        [TestMethod]
        public void CatalogueNull_DifferentSchema_SameName_NotEqual_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: null, schema: "KK", name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: null, schema: "LL", name: "AA");

            Assert.IsFalse(tableAttribute1.Equals(tableAttribute2));
        }

        [TestMethod]
        public void DifferentCatalogue_SameSchema_SameName_NotEqual_Test()
        {
            var tableAttribute1 = new TableAttribute(catalogueName: "KK", schema: "BB", name: "AA");
            var tableAttribute2 = new TableAttribute(catalogueName: "LL", schema: "BB", name: "AA");

            Assert.IsFalse(tableAttribute1.Equals(tableAttribute2));
        }

        #endregion Equals tests
    }
}
