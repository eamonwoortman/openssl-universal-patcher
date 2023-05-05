using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSSLUniversalPatcher {
	internal class Program {
		static void Main(string[] args) {

			// check for correct usage
			if (args.Length != 3) {
				Console.Error.WriteLine("bsdiff oldfile newfile patchfile");
				return;
			}

			var inputFile = args[0];
			var outputFile = args[1];
			var patchFile = args[2];

			if (ByteSequencePatcher.Apply(inputFile, outputFile, patchFile)) {
				Console.WriteLine("Successfully patched target file");
			} else {
				Console.WriteLine("Failed to patch the target file");
			}

		}
	}
}
