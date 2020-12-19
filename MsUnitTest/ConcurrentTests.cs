﻿using System;
using System.Threading;
using fastJSON;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MsUnitTest
{
	[TestClass]
	public class ConcurrentTests
	{
		#region Sample Types
		public class ConcurrentClassA
		{
			public ConcurrentClassA () {
				Console.WriteLine ("ctor ConcurrentClassA. I will sleep for 2 seconds.");
				Thread.Sleep (2000);
				Thread.MemoryBarrier (); // just to be sure the caches on multi-core processors do not hide the bug. For me, the bug is present without the memory barrier, too.
				Console.WriteLine ("ctor ConcurrentClassA. I am done sleeping.");
			}

			public PayloadA PayloadA { get; set; }
		}

		public class ConcurrentClassB
		{
			public ConcurrentClassB () {
				Console.WriteLine ("ctor ConcurrentClassB.");
			}

			public PayloadB PayloadB { get; set; }
		}

		public class PayloadA
		{
			public PayloadA () {
				Console.WriteLine ("ctor PayLoadA.");
			}
		}

		public class PayloadB
		{
			public PayloadB () {
				Console.WriteLine ("ctor PayLoadB.");
			}
		} 
		#endregion

		void GenerateJsonForAandB (out string jsonA, out string jsonB) {
			Console.WriteLine ("Begin constructing the original objects. Please ignore trace information until I'm done.");

			// set all parameters to false to produce pure JSON
			JSON.Parameters = new JSONParameters { EnableAnonymousTypes = false, SerializeNullValues = false, ShowReadOnlyProperties = false, UseExtensions = false, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = false };

			var a = new ConcurrentClassA { PayloadA = new PayloadA () };
			var b = new ConcurrentClassB { PayloadB = new PayloadB () };

			// A is serialized with extensions and global types
			jsonA = JSON.ToJSON (a, new JSONParameters { EnableAnonymousTypes = false, SerializeNullValues = false, ShowReadOnlyProperties = false, UseExtensions = true, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = true });
			// B is serialized using the above defaults
			jsonB = JSON.ToJSON (b);

			Console.WriteLine ("Ok, I'm done constructing the objects. Below is the generated json. Trace messages that follow below are the result of deserialization and critical for understanding the timing.");
			Console.WriteLine (jsonA);
			Console.WriteLine (jsonB);
		}

		[TestMethod]
		public void UsingGlobalsBug_singlethread () {
			var p = JSON.Parameters;
			string jsonA;
			string jsonB;
			GenerateJsonForAandB (out jsonA, out jsonB);

			var ax = JSON.ToObject (jsonA); // A has type information in JSON-extended
			var bx = JSON.ToObject<ConcurrentClassB> (jsonB); // B needs external type info

			Assert.IsNotNull (ax);
			Assert.IsInstanceOfType (ax, typeof (ConcurrentClassA));
			Assert.IsNotNull (bx);
			Assert.IsInstanceOfType (bx, typeof (ConcurrentClassB));
			JSON.Parameters = p;
		}

		[TestMethod]
		public void UsingGlobalsBug_multithread () {
			var p = JSON.Parameters;
			string jsonA;
			string jsonB;
			GenerateJsonForAandB (out jsonA, out jsonB);

			object ax = null;
			object bx = null;

			/*
             * Intended timing to force CannotGetType bug in 2.0.5:
             * the outer class ConcurrentClassA is deserialized first from json with extensions+global types. It reads the global types and sets _usingglobals to true.
             * The constructor contains a sleep to force parallel deserialization of ConcurrentClassB while in A's constructor.
             * The deserialization of B sets _usingglobals back to false.
             * After B is done, A continues to deserialize its PayloadA. It finds type "2" but since _usingglobals is false now, it fails with "Cannot get type".
             */

			Exception exception = null;

			var thread = new Thread (() =>
			{
				try {
					Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " A begins deserialization");
					ax = JSON.ToObject (jsonA); // A has type information in JSON-extended
					Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " A is done");
				}
				catch (Exception ex) {
					exception = ex;
				}
			});

			thread.Start ();

			Thread.Sleep (500); // wait to allow A to begin deserialization first

			Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " B begins deserialization");
			bx = JSON.ToObject<ConcurrentClassB> (jsonB); // B needs external type info
			Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " B is done");

			Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " waiting for A to continue");
			thread.Join (); // wait for completion of A due to Sleep in A's constructor
			Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " threads joined.");

			Assert.IsNull (exception, exception == null ? "" : exception.Message + " " + exception.StackTrace);

			Assert.IsNotNull (ax);
			Assert.IsInstanceOfType (ax, typeof (ConcurrentClassA));
			Assert.IsNotNull (bx);
			Assert.IsInstanceOfType (bx, typeof (ConcurrentClassB));
			JSON.Parameters = p;
		}

		[TestMethod]
		public void NullOutput () {
			var c = new ConcurrentClassA ();
			var s = JSON.ToJSON (c, new JSONParameters { UseExtensions = false, SerializeNullValues = false });
			Console.WriteLine (JSON.Beautify (s));
			Assert.AreEqual (false, s.Contains (",")); // should not have a comma
		}

	}
}
