/*
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

using System.Diagnostics.Tracing;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Perf;

[Config(typeof(Config))]
public partial class CommandBenchmark
{
	class Config : ManualConfig
	{
		private const ClrTraceEventParser.Keywords EventParserKeywords =
			ClrTraceEventParser.Keywords.Exception
			| ClrTraceEventParser.Keywords.GC
			| ClrTraceEventParser.Keywords.Jit
			| ClrTraceEventParser.Keywords.JitTracing // for the inlining events
			| ClrTraceEventParser.Keywords.Loader
			| ClrTraceEventParser.Keywords.NGen;

		public Config()
		{
 			AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core70));

			AddDiagnoser(MemoryDiagnoser.Default);

			AddDiagnoser(new EventPipeProfiler(providers: new[] {
				new EventPipeProvider(
					ClrTraceEventParser.ProviderName,
					EventLevel.Verbose,
					(long)EventParserKeywords
				)
			}));
		}
	}

	protected readonly string ConnectionString = (new FbConnectionStringBuilder()
	{
		Database = Path.Join(Path.GetTempPath(), "FirebirdSql.Data.FirebirdClient.Benchmark.fb50.fdb"),
		UserID = "SYSDBA",
		Password = "masterkey",
		ServerType = FbServerType.Embedded,
		ClientLibrary = Path.Join(Path.GetTempPath(), @"firebird-binaries\fb50\fbclient.dll"),
	}).ConnectionString;

	[Params(
		"BIGINT",
		"VARCHAR(10) CHARACTER SET UTF8")]
	public string DataType { get; set; }

	[Params(100, 10000)]
	public int Count { get; set; }

	void GlobalSetupBase()
	{
		FbConnection.CreateDatabase(ConnectionString, 16 * 1024, false, true);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		FbConnection.ClearAllPools();
		FbConnection.DropDatabase(ConnectionString);
	}
}
