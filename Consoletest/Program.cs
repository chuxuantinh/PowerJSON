﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace consoletest
{
    public class Program
    {
        static int count = 1000;
        static int tcount = 5;
        static DataSet ds = new DataSet();
        static bool exotic = false;
        static bool dsser = false;

		public static void Main (string[] args)
        {
			Console.WriteLine (".net version = " + Environment.Version);
			START:
			Console.WriteLine ("==== fastJSON console test program ====");
			Console.WriteLine ("\t1, Serialization Benchmark");
			Console.WriteLine ("\t2, Deserialization Benchmark");
			Console.WriteLine ("\t3, Misc. Tests");
			Console.WriteLine ("\t4, Exotic Serialization Benchmark");
			Console.WriteLine ("\t5, Exotic Deserialization Benchmark");
			Console.WriteLine ("\t6, Exotic Misc. Tests");
			Console.WriteLine ("\t7, Serialization Tests");
			Console.WriteLine ("\t8, Deserialization Tests");
			Console.WriteLine ("\tOther key: Exit");
			Console.WriteLine ("Please select an option: ");
			var k = Console.ReadKey ().KeyChar;
			Console.WriteLine();
			switch (k) {
				case '4':
					exotic = true;
					SerializationTest ();
					break;
				case '1':
					exotic = false;
					SerializationTest ();
					break;
				case '7':
					exotic = false;
					SerializationTest ();
					exotic = true;
					SerializationTest ();
					break;
				case '5':
					exotic = true;
					DeserializationTest ();
					break;
				case '2':
					exotic = false;
					DeserializationTest ();
					break;
				case '8':
					exotic = false;
					DeserializationTest ();
					exotic = true;
					DeserializationTest ();
					break;
				case '6':
					exotic = true;
					WriteTestObject (CreateObject ());
					WriteTestObject (CreateNVCollection ());
					NullValueTest ();
					TestCustomConverterType ();
					break;
				case '3':
					exotic = false;
					WriteTestObject (CreateObject ());
					WriteTestObject (CreateNVCollection ());
					NullValueTest ();
					TestCustomConverterType ();
					break;
				default: return;
			}

			goto START;
			#region [ other tests]

            //			litjson_serialize();
            //			jsonnet_serialize();
            //			jsonnet4_serialize();
            //stack_serialize();

            //systemweb_deserialize();
            //bin_deserialize();
            //fastjson_deserialize();

            //			litjson_deserialize();
            //			jsonnet_deserialize();
            //			jsonnet4_deserialize();
            //			stack_deserialize();
            #endregion
        }

		private static void DeserializationTest () {
			ds = CreateDataset ();
			Console.WriteLine ("-dataset");
			dsser = false;
			//bin_deserialize();
			fastjson_deserialize ();
			dsser = true;
			Console.WriteLine ("+dataset");
			//bin_deserialize();
			fastjson_deserialize ();
		}

		private static void SerializationTest () {
			ds = CreateDataset ();
			Console.WriteLine ("-dataset");
			dsser = false;
			//bin_serialize();
			fastjson_serialize ();
			dsser = true;
			Console.WriteLine ("+dataset");
			//bin_serialize();
			fastjson_serialize ();
		}

		private static System.Collections.Specialized.NameValueCollection CreateNVCollection () {
			var n = new System.Collections.Specialized.NameValueCollection ();
			n.Add ("new1", "value1");
			n.Add ("item2", null);
			n.Add ("item3", "value3");
			n.Add ("item3", "value3");
			return n;
		}

		private static void WriteTestObject<T> (T obj) {
			var t = fastJSON.JSON.ToJSON (obj);
			Console.WriteLine ("serialized " + typeof (T).FullName + ": ");
			Console.WriteLine (t);
			Console.WriteLine ("deserialized object: ");
			var o = fastJSON.JSON.ToObject<T> (t);
			Console.WriteLine (fastJSON.JSON.ToJSON (o));
			Console.ReadKey ();
		}

		private static void NullValueTest () {
			Console.WriteLine ("Null value test");
			var dv = new NullValueTest ();
			Console.WriteLine (fastJSON.JSON.ToJSON (dv));
			Console.WriteLine ((dv = fastJSON.JSON.ToObject<NullValueTest> (@"{ ""Text"": null, ""Number"": null, ""Array"": null, ""Guid"": null, ""NullableNumber"": null }")));
			Console.WriteLine ((dv = fastJSON.JSON.ToObject<NullValueTest> (@"{}")));
			Console.WriteLine ();
		}

		private static void TestCustomConverterType () {
			Console.WriteLine ("Custom converter test");
			var c = new Test () {
				CustomConverter = new CustomConverterType () {
					Array = new int[] { 1, 2, 3 },
					NormalArray = new int[] { 2, 3, 4 },
					Variable1 = new int[] { 3, 4 },
					Variable2 = new List<int> { 5, 6 }
				},
				Multiple1 = new FreeTypeTest () { FreeType = new class1 ("a", "b", Guid.NewGuid ()) },
				Multiple2 = new FreeTypeTest () { FreeType = new class2 ("a", "b", "c") },
				Multiple3 = new FreeTypeTest () { FreeType = DateTime.Now }
			};
			var t = fastJSON.JSON.ToJSON (c, new fastJSON.JSONParameters () { UseExtensions = false });
			Console.WriteLine ("serialized Test instance: ");
			Console.WriteLine (t);
			Console.WriteLine ("deserialized Test instance: ");
			var o = fastJSON.JSON.ToObject<Test> (t);
			Console.WriteLine (fastJSON.JSON.ToJSON (o, new fastJSON.JSONParameters () { UseExtensions = false }));
			Console.WriteLine ();
			Console.ReadKey (true);
		}

        //private static string pser(object data)
        //{
        //    System.Drawing.Point p = (System.Drawing.Point)data;
        //    return p.X.ToString() + "," + p.Y.ToString();
        //}

        //private static object pdes(string data)
        //{
        //    string[] ss = data.Split(',');

        //    return new System.Drawing.Point(
        //        int.Parse(ss[0]),
        //        int.Parse(ss[1])
        //        );
        //}

        //private static string tsser(object data)
        //{
        //    return ((TimeSpan)data).Ticks.ToString();
        //}

        //private static object tsdes(string data)
        //{
        //    return new TimeSpan(long.Parse(data));
        //}

        public static colclass CreateObject()
        {
            var c = new colclass();

            c.booleanValue = true;
            c.ordinaryDecimal = 3;

            if (exotic)
            {
                c.nullableGuid = Guid.NewGuid();
                c.hash = new Hashtable();
                c.bytes = new byte[1024];
                c.stringDictionary = new Dictionary<string, baseclass>();
                c.objectDictionary = new Dictionary<baseclass, baseclass>();
                c.intDictionary = new Dictionary<int, baseclass>();
                c.nullableDouble = 100.003;

                if (dsser)
                    c.dataset = ds;
                c.nullableDecimal = 3.14M;

                c.hash.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
                c.hash.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));

                c.stringDictionary.Add("name1", new class2("1", "code", "desc"));
                c.stringDictionary.Add("name2", new class1("1", "code", Guid.NewGuid()));

                c.intDictionary.Add(1, new class2("1", "code", "desc"));
                c.intDictionary.Add(2, new class1("1", "code", Guid.NewGuid()));

                c.objectDictionary.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
                c.objectDictionary.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));

                c.arrayType = new baseclass[2];
                c.arrayType[0] = new class1();
                c.arrayType[1] = new class2();
            }


            c.items.Add(new class1("1", "1", Guid.NewGuid()));
            c.items.Add(new class2("2", "2", "desc1"));
            c.items.Add(new class1("3", "3", Guid.NewGuid()));
            c.items.Add(new class2("4", "4", "desc2"));

            c.laststring = "" + DateTime.Now;

            return c;
        }

        public static DataSet CreateDataset()
        {
            DataSet ds = new DataSet();
            for (int j = 1; j < 3; j++)
            {
                DataTable dt = new DataTable();
                dt.TableName = "Table" + j;
                dt.Columns.Add("col1", typeof(int));
                dt.Columns.Add("col2", typeof(string));
                dt.Columns.Add("col3", typeof(Guid));
                dt.Columns.Add("col4", typeof(string));
                dt.Columns.Add("col5", typeof(bool));
                dt.Columns.Add("col6", typeof(string));
                dt.Columns.Add("col7", typeof(string));
                ds.Tables.Add(dt);
                Random rrr = new Random();
                for (int i = 0; i < 100; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = rrr.Next(int.MaxValue);
                    dr[1] = "" + rrr.Next(int.MaxValue);
                    dr[2] = Guid.NewGuid();
                    dr[3] = "" + rrr.Next(int.MaxValue);
                    dr[4] = true;
                    dr[5] = "" + rrr.Next(int.MaxValue);
                    dr[6] = "" + rrr.Next(int.MaxValue);

                    dt.Rows.Add(dr);
                }
            }
            return ds;
        }

        private static void fastjson_deserialize()
        {
            Console.Write("fastjson deserialize");
            colclass c = CreateObject();

			var stopwatch = new Stopwatch();
            for (int pp = 0; pp < tcount; pp++)
            {
                colclass deserializedStore;
                string jsonText = null;

				stopwatch.Restart();
                jsonText = fastJSON.JSON.ToJSON(c, new fastJSON.JSONParameters () { SerializeNullValues = false });
                //Console.WriteLine(" size = " + jsonText.Length);
                for (int i = 0; i < count; i++)
                {
                    deserializedStore = fastJSON.JSON.ToObject<colclass>(jsonText);
                }
				stopwatch.Stop();
				Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
			Console.WriteLine ();
		}

		private static void fastjson_serialize () {
			Console.Write ("fastjson serialize");
			colclass c = CreateObject ();
			fastJSON.JSON.ToJSON (c);
			var stopwatch = new Stopwatch();
            for (int pp = 0; pp < tcount; pp++)
            {
                string jsonText = null;
				stopwatch.Restart();
                for (int i = 0; i < count; i++)
                {
                    jsonText = fastJSON.JSON.ToJSON(c);
                }
				stopwatch.Stop();
				Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
			Console.WriteLine ();
        }


		private static void bin_deserialize()
        {
            Console.Write("bin deserialize");
            colclass c = CreateObject();
			var stopwatch = new Stopwatch();
            for (int pp = 0; pp < tcount; pp++)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
				colclass deserializedStore = null;
				stopwatch.Restart();
                bf.Serialize(ms, c);
                //Console.WriteLine(" size = " +ms.Length);
                for (int i = 0; i < count; i++)
                {
					stopwatch.Stop(); // we stop then resume the stopwatch here so we don't factor in Seek()'s execution
                    ms.Seek(0L, SeekOrigin.Begin);
					stopwatch.Start();
                    deserializedStore = (colclass)bf.Deserialize(ms);
                }
				stopwatch.Stop();
				Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
			Console.WriteLine ();
        }

        private static void bin_serialize()
        {
            Console.Write("bin serialize");
            colclass c = CreateObject();
			var stopwatch = new Stopwatch();
            for (int pp = 0; pp < tcount; pp++)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
				stopwatch.Restart();
                for (int i = 0; i < count; i++)
                {
					stopwatch.Stop(); // we stop then resume the stop watch here so we don't factor in the MemoryStream()'s execution
                    ms = new MemoryStream();
					stopwatch.Start();
                    bf.Serialize(ms, c);
                }
				stopwatch.Stop();
				Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

		#region [   other tests  ]
		/*
		private static void systemweb_serialize()
		{
			Console.WriteLine();
			Console.Write("msjson serialize");
			colclass c = CreateObject();
			var sws = new System.Web.Script.Serialization.JavaScriptSerializer();
			for (int pp = 0; pp < tcount; pp++)
			{
				DateTime st = DateTime.Now;
				colclass deserializedStore = null;
				string jsonText = null;

				//jsonText =sws.Serialize(c);
				//Console.WriteLine(" size = " + jsonText.Length);
				for (int i = 0; i < count; i++)
				{
					jsonText =sws.Serialize(c);
					//deserializedStore = (colclass)sws.DeserializeObject(jsonText);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void stack_serialize () {
			Console.Write ("servicestack serialize");
			colclass c = CreateObject ();
			ServiceStack.Text.JsConfig.Reset ();
			ServiceStack.Text.JsConfig<baseclass>.IncludeTypeInfo = true;
			Console.WriteLine (ServiceStack.Text.JsonSerializer.SerializeToString (c));
			var stopwatch = new Stopwatch ();
			for (int pp = 0; pp < tcount; pp++) {
				string jsonText = null;
				stopwatch.Restart ();
				for (int i = 0; i < count; i++) {
					jsonText = ServiceStack.Text.JsonSerializer.SerializeToString (c);
				}
				stopwatch.Stop ();
				Console.Write ("\t" + stopwatch.ElapsedMilliseconds);
			}
			Console.WriteLine ();
		}

		private static void systemweb_deserialize()
//		{
//			Console.WriteLine();
//			Console.Write("fastjson deserialize");
//			colclass c = CreateObject();
//			var sws = new System.Web.Script.Serialization.JavaScriptSerializer();
//			for (int pp = 0; pp < tcount; pp++)
//			{
//				DateTime st = DateTime.Now;
//				colclass deserializedStore = null;
//				string jsonText = null;
//
//				jsonText =sws.Serialize(c);
//				//Console.WriteLine(" size = " + jsonText.Length);
//				for (int i = 0; i < count; i++)
//				{
//					deserializedStore = (colclass)sws.DeserializeObject(jsonText);
//				}
//				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
//			}
//		}

		private static void jsonnet4_deserialize()
		{
			Console.WriteLine();
			Console.Write("json.net4 deserialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c;
				colclass deserializedStore = null;
				string jsonText = null;
				c = Tests.mytests.CreateObject();
				var s = new Newtonsoft.Json.JsonSerializerSettings();
				s.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
				jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(c, Newtonsoft.Json.Formatting.Indented, s);
				for (int i = 0; i < count; i++)
				{
					deserializedStore = (colclass)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText, typeof(colclass), s);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void jsonnet4_serialize()
		{
			Console.WriteLine();
			Console.Write("json.net4 serialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c = Tests.mytests.CreateObject();
				Newtonsoft.Json.JsonSerializerSettings s = null;
				string jsonText = null;
				s = new Newtonsoft.Json.JsonSerializerSettings();
				s.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;

				for (int i = 0; i < count; i++)
				{
					jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(c, Newtonsoft.Json.Formatting.Indented, s);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void stack_deserialize () {
			Console.Write ("servicestack deserialize");
			colclass c = CreateObject ();

			var stopwatch = new Stopwatch ();
			for (int pp = 0; pp < tcount; pp++) {
				colclass deserializedStore;
				string jsonText = null;

				stopwatch.Restart ();
				jsonText = ServiceStack.Text.JsonSerializer.SerializeToString (c);
				//Console.WriteLine(" size = " + jsonText.Length);
				for (int i = 0; i < count; i++) {
					deserializedStore = ServiceStack.Text.JsonSerializer.DeserializeFromString<colclass> (jsonText);
				}
				stopwatch.Stop ();
				Console.Write ("\t" + stopwatch.ElapsedMilliseconds);
			}
			Console.WriteLine ();
		}

		private static void jsonnet_deserialize()
		{
			Console.WriteLine();
			Console.Write("json.net deserialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c;
				colclass deserializedStore = null;
				string jsonText = null;
				c = Tests.mytests.CreateObject();
				var s = new json.net.JsonSerializerSettings();
				s.TypeNameHandling = json.net.TypeNameHandling.All;
				jsonText = json.net.JsonConvert.SerializeObject(c, json.net.Formatting.Indented, s);
				for (int i = 0; i < count; i++)
				{
					deserializedStore = (colclass)json.net.JsonConvert.DeserializeObject(jsonText, typeof(colclass), s);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void jsonnet_serialize()
		{
			Console.WriteLine();
			Console.Write("json.net serialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c = Tests.mytests.CreateObject();
				json.net.JsonSerializerSettings s = null;
				string jsonText = null;
				s = new json.net.JsonSerializerSettings();
				s.TypeNameHandling = json.net.TypeNameHandling.All;

				for (int i = 0; i < count; i++)
				{
					jsonText = json.net.JsonConvert.SerializeObject(c, json.net.Formatting.Indented, s);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void litjson_deserialize()
		{
			Console.WriteLine();
			Console.Write("litjson deserialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c;
				colclass deserializedStore = null;
				string jsonText = null;
				c = Tests.mytests.CreateObject();
				jsonText = BizFX.Common.JSON.JsonMapper.ToJson(c);
				for (int i = 0; i < count; i++)
				{
					deserializedStore = (colclass)BizFX.Common.JSON.JsonMapper.ToObject(jsonText);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		private static void litjson_serialize()
		{
			Console.WriteLine();
			Console.Write("litjson serialize");
			for (int pp = 0; pp < 5; pp++)
			{
				DateTime st = DateTime.Now;
				colclass c;
				string jsonText = null;
				c = Tests.mytests.CreateObject();
				for (int i = 0; i < count; i++)
				{
					jsonText = BizFX.Common.JSON.JsonMapper.ToJson(c);
				}
				Console.Write("\t" + DateTime.Now.Subtract(st).TotalMilliseconds );
			}
		}

		
		 */
		#endregion
	}
}