using System;
using System.IO;
using System.Reflection;

namespace OpenSSLUniversalPatcher {
	internal class Program {
		static void Main(string[] args) {

			// check for correct usage
			if (args.Length != 2) {
				Console.Error.WriteLine("Usage: OpenSSLUniversalPatcher.exe <oldfile> <newfile>");
				return;
			}

			var inputFilePath = args[0];
			var outputFilePath = args[1];
			var patchFile = GetResource("Resources.openssl.universal.patch");
			if (ByteSequencePatcher.Apply(inputFilePath, outputFilePath, patchFile)) {
				Console.WriteLine("Successfully patched target file");
			} else {
				Console.WriteLine("Failed to patch the target file");
			}

		}

		public static string GetResource(string resourceFilePath) {
			try {
				var assembly = Assembly.GetExecutingAssembly();
				var assemblyName = assembly.GetName().Name;
				var stream = assembly.GetManifestResourceStream(assemblyName + "." + resourceFilePath);
				return new StreamReader(stream).ReadToEnd();
			} catch (Exception e) {
				Console.WriteLine(e);
				return null;
			}
		}

	}


}
