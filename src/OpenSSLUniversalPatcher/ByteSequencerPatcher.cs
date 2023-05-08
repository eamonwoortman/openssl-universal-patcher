using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenSSLUniversalPatcher {

	class ByteSequencePatcher {
		public static bool Apply(string inputPath, string outputPath, string patchFile) {
			Debug.Assert(!string.IsNullOrEmpty(inputPath));
			Debug.Assert(!string.IsNullOrEmpty(outputPath));
			Debug.Assert(!string.IsNullOrEmpty(patchFile));

			byte[] originalBytes = File.ReadAllBytes(inputPath);
			Patch patch = ParsePatchFile(patchFile);

			List<PatternOffsets> offsets = FindSequencePattern(originalBytes, patch);
			if (offsets.Count == 0) {
				Console.Error.WriteLine("Error, pattern not found in target.");
				return false;
			}
			if (offsets.Count > 1) {
				Console.Error.WriteLine("Error, pattern found multiple times in target.");
				return false;
			}

			Console.WriteLine("Sequence pattern found in target, patching...");

			var patternOffsets = offsets[0];
			var firstOffset = patternOffsets.FirstOffset;
			var secondOffset = patternOffsets.SecondOffset;

			// Apply the first sequence patch
			Array.Copy(patch.FirstSequence.Patched, 0, originalBytes, firstOffset, patch.FirstSequence.Patched.Length);

			// Apply the second sequence patch
			Array.Copy(patch.SecondSequence.Patched, 0, originalBytes, secondOffset, patch.SecondSequence.Patched.Length);

			// Write the patched bytes to the output file
			File.WriteAllBytes(outputPath, originalBytes);

			Console.WriteLine("Patched file written to " + outputPath);
			return true;
		}

		struct PatternOffsets {
			public int FirstOffset;
			public int SecondOffset;
		}

		private static List<PatternOffsets> FindSequencePattern(byte[] originalBytes, Patch patch) {
			List<PatternOffsets> offsets = new List<PatternOffsets>();
			int currentOffset = 0;
			int originalLength = originalBytes.Length;
			while (currentOffset < originalLength) {
				int firstSequenceIndex = FindSequence(originalBytes, patch.FirstSequence.Original, currentOffset, originalLength);
				if (firstSequenceIndex == -1) {
					break;
				}

				int secondOffset = firstSequenceIndex + patch.FirstSequence.Original.Length + patch.RelativeOffset;
				int secondSequenceIndex = FindSequence(originalBytes, patch.SecondSequence.Original, secondOffset, secondOffset + patch.SecondSequence.Original.Length);
				if (secondSequenceIndex == -1) {
					currentOffset = secondOffset + patch.SecondSequence.Original.Length;
					continue;
				}
				
				PatternOffsets offsetPair = new PatternOffsets { FirstOffset = firstSequenceIndex, SecondOffset = secondSequenceIndex };
				offsets.Add(offsetPair);

				currentOffset = secondSequenceIndex + patch.SecondSequence.Original.Length;
			}
			return offsets;
		}

		static string TrimVariablePart(string inputLine) {
			int index = inputLine.IndexOf(':') + 2; // include character and space
			return inputLine.Substring(index); 
		}

		static Patch ParsePatchFile(string sequenceFile) {
			string[] patchLines = sequenceFile.Replace("\r\n", "\n").Split('\n');

			SequencePair firstPair = new SequencePair();
			firstPair.Original = ParseByteSequence(TrimVariablePart(patchLines[1]));
			firstPair.Patched = ParseByteSequence(TrimVariablePart(patchLines[3]));

			SequencePair secondPair = new SequencePair();
			secondPair.Original = ParseByteSequence(TrimVariablePart(patchLines[7]));
			secondPair.Patched = ParseByteSequence(TrimVariablePart(patchLines[9]));
			
			int offset;
			int.TryParse(TrimVariablePart(patchLines[5]), out offset);

			Patch patch = new Patch();
			patch.RelativeOffset = offset;
			patch.FirstSequence = firstPair;
			patch.SecondSequence = secondPair;
			return patch;
		}

		static byte[] ParseByteSequence(string line) {
			string[] parts = line.Split(' ');
			return parts.Select(part => Convert.ToByte(part, 16)).ToArray();
		}

		public static int FindSequence(byte[] data, byte[] sequence, int startOffset, int maxLookupLength) {
			int offset = -1;

			for (int i = startOffset; i < maxLookupLength - sequence.Length + 1; i++) {
				bool found = true;
				for (int j = 0; j < sequence.Length; j++) {
					if (data[i + j] != sequence[j]) {
						found = false;
						break;
					}
				}

				if (found) {
					offset = i;
					break;
				}
			}

			return offset;
		}

	}

	class Patch {
		public SequencePair FirstSequence;
		public SequencePair SecondSequence;
		public int RelativeOffset;
	}

	class SequencePair {
		public byte[] Original { get; set; }
		public byte[] Patched { get; set; }
	}

}
