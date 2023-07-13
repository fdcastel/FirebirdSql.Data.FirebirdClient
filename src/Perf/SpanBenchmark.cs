using System;
using System.Diagnostics.Tracing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Perf
{
	[Config(typeof(Config))]
	public class SpanBenchmark
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

		[Params(100, 1_000)]
		public int N;

		string countries = null;
		int index, numberOfCharactersToExtract;

		[GlobalSetup]
		public void GlobalSetup()
		{
			countries = "India, USA, UK, Australia, Netherlands, Belgium";
			index = countries.LastIndexOf(",", StringComparison.Ordinal);
			numberOfCharactersToExtract = countries.Length - index;
		}

		[Benchmark]
		public void Substring()
		{
			for (int i = 0; i < N; i++)
			{
				_ = countries.Substring(index + 1, numberOfCharactersToExtract - 1);
			}
		}

		[Benchmark(Baseline = true)]
		public void Span()
		{
			for (int i = 0; i < N; i++)
			{
				_ = countries.AsSpan().Slice(index + 1, numberOfCharactersToExtract - 1);
			}
		}
	}
}
