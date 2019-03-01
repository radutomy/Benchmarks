using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
	[ClrJob, CoreJob]
	// [InProcessAttribute]
	public class Benchmark
	{
		[Benchmark]
		public bool AreStringsEqual() =>
			"This reasonably long short but sweet string could actually contain a subtle difference."
			.Equals(
			"This reasonably long short but sweet string could actually contain a subtle difference?");

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
	}

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmark>();
		}
	}
}
