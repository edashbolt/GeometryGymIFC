using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeometryGym.Ifc;

namespace CoreTestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
			ProcessPropertyLines();
		}

		static void ProcessPropertyLines()
		{
			Console.WriteLine("Loading IFC file...");

			//string fileContents = File.ReadAllText("C:/Users/eaaz3620/OneDrive - ARCADIS/Desktop/Testing/IFC/ORD/30089580-ACD-RG100-MOD-D300.ifc");
			string fileContents = File.ReadAllText("TestIFC.ifc");
			//string fileContents = File.ReadAllText("C:\\Temp\\RevitIFC\\TEST - EASTERN PORTAL.ifc");

			Stopwatch sw = Stopwatch.StartNew();

			int dataIndex = fileContents.IndexOf("DATA;");
			string header = fileContents.Substring(0, dataIndex);

			//Load ifc
			DatabaseIfc db = new DatabaseIfc(new StringReader(header));
			if (db.Format != FormatIfcSerialization.STEP)
				throw new FormatException($"Error: {db.Format} is not supported by this processor");
			SerializationIfcSTEP step = new SerializationIfcSTEP(db);

			//Get property lines
			string[] lines = fileContents.Split(new string[] { ";\r\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			string[] properties = lines.Where(x => x.Contains("IFCPROPERTY")).ToArray();
			
			//Process properties
			ParallelOptions parallelOptions = new ParallelOptions();
			//parallelOptions.CancellationToken = mCancellationTokenSource.Token;
			parallelOptions.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

			bool aborted = false;
			try
			{/*
				Parallel.ForEach(properties, parallelOptions, x =>
				{
					step.processDataLine(x);
					parallelOptions.CancellationToken.ThrowIfCancellationRequested();
				});
				*/
				foreach (var property in properties) { 
					step.processDataLine(property + ";");
				}
			}
			catch (OperationCanceledException)
			{
				aborted = true;
			}
			if (aborted)
			{
				return;
			}


			step.processObjects();
			
			//Console.WriteLine(sw.Elapsed);
			//File.WriteAllLines("C:/temp/customParser.txt", properties);
			var propsets = db.Where(x => x.GetType() == typeof(IfcPropertySet) && (x as IfcPropertySet).Name == "General");
			foreach(IfcPropertySet propset in propsets)
			{
				var getProps = propset.HasProperties;
			}

			Console.WriteLine(sw.Elapsed);

			Console.ReadKey();
		}

		static void ProcessLines()
		{
			Console.WriteLine("Loading IFC file...");

			Stopwatch sw = Stopwatch.StartNew();

			string fileContents = File.ReadAllText("C:/Users/eaaz3620/OneDrive - ARCADIS/Desktop/Testing/IFC/ORD/30089580-ACD-RG100-MOD-D300.ifc");
			//string fileContents = File.ReadAllText("TestIFC.ifc");

			//Stopwatch sw = Stopwatch.StartNew();

			string[] lines = fileContents.Split(new string[] { ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			string[] properties = lines.Where(x => x.Contains("IFCPROPERTY")).ToArray();

			DatabaseIfc db = new DatabaseIfc(ModelView.Ifc4X1NotAssigned);
			SerializationIfc sifc = new SerializationIfc(db);
			SerializationIfcSTEP step = new SerializationIfcSTEP(db);

			for (int i = 0; i < properties.Length; i++)
			{
				step.processDataLine(properties[i]);
			}

			step.processObjects();

			//Console.WriteLine(sw.Elapsed);

			//File.WriteAllLines("C:/temp/customParser.txt", properties);

			Console.WriteLine(sw.Elapsed);

			Console.ReadKey();
		}

		static void ProcessDatabase()
		{
			Console.WriteLine("Loading IFC file...");

			Stopwatch sw = Stopwatch.StartNew();

			string fileContents = File.ReadAllText("C:/Users/eaaz3620/OneDrive - ARCADIS/Desktop/Testing/IFC/ORD/30089580-ACD-RG100-MOD-D300.ifc");
			//string fileContents = File.ReadAllText("TestIFC.ifc");

			string[] content = fileContents.Split(new string[] { "ENDSEC;\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (content.Length > 3)
				throw new Exception("Unexpected error occurred splitting content");

			//Stopwatch sw = Stopwatch.StartNew();

			string[] lines = content[1].Split(new string[] { ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			string[] properties = lines.Where(x => x.Contains("IFCPROPERTY")).ToArray();

			string allContent = content[0] + "ENDSEC;\r\nDATA;\r\n" + string.Join(";\r\n", properties) + "\r\nENDSEC;\r\n" + content[2];
			TextReader reader = new StringReader(allContent);
			DatabaseIfc db = new DatabaseIfc(reader);

			Console.WriteLine(sw.Elapsed);

			Console.ReadKey();
		}
	}
}
