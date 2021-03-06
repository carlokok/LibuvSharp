using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class LoopFixture
	{
		[Test]
		public void NoAdditionalResources()
		{
			Loop.Default.Run();
		}

		[Test]
		public void RunAsync()
		{
			var timer = new Timer();
			Loop.Default.RunAsync();
			timer.Close();
		}

		[Test]
		public void Handles()
		{
			Assert.Greater(Loop.Default.Handles.Length, 0);
		}
	}
}

