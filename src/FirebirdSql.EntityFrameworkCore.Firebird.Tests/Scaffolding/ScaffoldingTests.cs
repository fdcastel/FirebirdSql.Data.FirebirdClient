﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Scaffolding;
#pragma warning disable EF1001
public class ScaffoldingTests : EntityFrameworkCoreTestsBase
{
	private DatabaseModel _databaseModel;

	public override async Task SetUp()
	{
		await base.SetUp();

		var modelFactory = GetModelFactory();
		_databaseModel = modelFactory.Create(Connection, new DatabaseModelFactoryOptions());
	}

	[Test]
	public void CanScaffoldPrimaryKey()
	{
		var testTable = _databaseModel.Tables.Where(t => t.Name.Equals("TEST")).First();

		Assert.NotNull(testTable.PrimaryKey);
		Assert.AreEqual("INT_FIELD", testTable.PrimaryKey.Columns[0].Name);
	}

	[Test]
	public void CanScaffoldColumns()
	{
		var testTable = _databaseModel.Tables.Where(t => t.Name == "TEST").First();	
		Assert.NotNull(testTable);

		var intColumn = testTable.Columns.Where(c => c.Name == "INT_FIELD").First();
		Assert.AreEqual("INTEGER", intColumn.StoreType);
		Assert.AreEqual("0", intColumn.DefaultValueSql);

		var charColumn = testTable.Columns.Where(c => c.Name == "CHAR_FIELD").First();
		Assert.AreEqual("CHAR(30)", charColumn.StoreType);

		var varcharColumn = testTable.Columns.Where(c => c.Name == "VARCHAR_FIELD").First();
		Assert.AreEqual("VARCHAR(100)", varcharColumn.StoreType);

		var numericColumn = testTable.Columns.Where(c => c.Name == "NUMERIC_FIELD").First();
		Assert.AreEqual("NUMERIC(15,2)", numericColumn.StoreType);

		var decimalColumn = testTable.Columns.Where(c => c.Name == "DECIMAL_FIELD").First();
		Assert.AreEqual("DECIMAL(15,2)", decimalColumn.StoreType);

		var blobColumn = testTable.Columns.Where(c => c.Name == "BLOB_FIELD").First();
		Assert.AreEqual("BLOB SUB_TYPE BINARY", blobColumn.StoreType);
		Assert.AreEqual(80, Convert.ToInt32(blobColumn.GetAnnotation(FbAnnotationNames.BlobSegmentSize).Value));

		var clobColumn = testTable.Columns.Where(c => c.Name == "CLOB_FIELD").First();
		Assert.AreEqual("BLOB SUB_TYPE TEXT", clobColumn.StoreType);
		Assert.AreEqual(80, Convert.ToInt32(clobColumn.GetAnnotation(FbAnnotationNames.BlobSegmentSize).Value));

		var exprColumn = testTable.Columns.Where(c => c.Name == "EXPR_FIELD").First();
		Assert.AreEqual("(smallint_field * 1000)", exprColumn.ComputedColumnSql);

		var csColumn = testTable.Columns.Where(c => c.Name == "CS_FIELD").First();
		Assert.AreEqual("CHAR(1)", csColumn.StoreType);
		Assert.AreEqual("UNICODE_FSS", csColumn.Collation);
		Assert.AreEqual("UNICODE_FSS", csColumn.GetAnnotation(FbAnnotationNames.CharacterSet).Value.ToString());
	}

	private static IDatabaseModelFactory GetModelFactory()
	{
		return new FbDatabaseModelFactory();
	}
}
