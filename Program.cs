using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
	[CoreJob, ClrJob]
	public class Benchmark
	{
		[Benchmark]
		public bool AreStringsEqual() =>
			"This reasonably long short but sweet string could actually contain a subtle difference."
			.Equals(
			"This reasonably long short but sweet string could actually contain a subtle difference?");

		// ======================================================================= //

		[Benchmark]
		public void LoopbackStreamSync()
		{
			var server = new TcpListener(IPAddress.Loopback, 3814);
			server.Start();

			Task.Run(() =>
			{
				var buff = new byte[1024];
				using (var stream = server.AcceptTcpClient().GetStream())
				{
					while (true)
					{
						var read = stream.Read(buff, 0, 1024);
						if (read == 0)
							break;
					}
				}
			});

			var client = new TcpClient();
			var sendBuff = new byte[1024];
			new Random().NextBytes(sendBuff);
			client.Connect(new IPEndPoint(IPAddress.Loopback, 3814));
			using (var sendStream = client.GetStream())
			{
				for (int i = 0; i < 1024; i++)
				{
					sendStream.Write(sendBuff, 0, 1024);
				}
			}
		}

		// ======================================================================= //

		[Benchmark]
		public void TestBoxing()
		{
			ISomeEvilInterface foo = new SomeEvilClass(42);
			foo.DoSomething();
		}

		// ======================================================================== // 

		public struct SomeEvilClass : ISomeEvilInterface
		{
			private int _value;
			public SomeEvilClass(int value)
			{
				_value = value;
			}

			private void DoSomethingEvil() { }
			void ISomeEvilInterface.DoSomething() => DoSomethingEvil();
		}

		public interface ISomeEvilInterface
		{
			void DoSomething();
		}
	}

	// ======================================================================= //

	[CoreJob]
	public class SpanVsArray
	{
		string _text;

		[GlobalSetup]
		public void Setup() => _text = new string(Enumerable.Repeat('1', 100000).ToArray());

		[Benchmark]
		public string TestSubstring() => _text.Substring(0, _text.Length / 2);

		[Benchmark]
		public ReadOnlySpan<char> TestSlice() => _text.AsSpan().Slice(0, _text.Length / 2);

		[Benchmark]
		public void ReadArray()
		{
			byte[] _buff = new byte[16];

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_text)))
			{
				while (stream.Read(_buff, 0, _buff.Length) > 0)
				{
					var newArray = new byte[_buff.Length / 2];
					Array.Copy(_buff, newArray, _buff.Length / 2);

					Parse(newArray);
				}
			}
		}

		[Benchmark]
		public void ReadSpan()
		{
			byte[] _buff = new byte[16];

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_text)))
			{
				while (stream.Read(_buff, 0, _buff.Length) > 0)
				{
					Parse(new ReadOnlySpan<byte>(_buff, 0, _buff.Length / 2));
				}
			}
		}

		void Parse(ReadOnlySpan<byte> memory)
		{
		}

		void Parse(byte[] managed)
		{
		}
	}

	// ======================================================================= //

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmark>();
			BenchmarkRunner.Run<SpanVsArray>();
		}
	}
}
