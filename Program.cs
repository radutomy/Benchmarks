using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
	[ClrJob, CoreJob]
	public class Benchmark
	{
		[Benchmark]
		public bool AreStringsEqual() =>
				"This string contains a subtle difference."
				.Equals(
				"This string contains a subtle difference?");
	}

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmark>();
		}
	}
}
