using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSSLUniversalPatcher {

	class ByteSequencePatcher {
		public static bool Apply(string inputPath, string outputPath, string patchFileText) {
			byte[] originalBytes = File.ReadAllBytes(inputPath);
			Patch patch = ParsePatchFile(patchFileText);

			List<PatternOffsets> offsets = FindSequencePattern(originalBytes, patch.FirstSequence, patch.SecondSequence, patch.RelativeOffset);
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
			var patchedBytes = patch.FirstSequence.Patched;
			Array.Copy(patchedBytes, 0, originalBytes, firstOffset, patchedBytes.Length);

			// Apply the second sequence patch
			patchedBytes = patch.SecondSequence.Patched;
			Array.Copy(patch.SecondSequence.Patched, 0, originalBytes, secondOffset, patchedBytes.Length);

			// Write the patched bytes to the output file
			File.WriteAllBytes(outputPath, originalBytes);

			Console.WriteLine("Patched file written to " + outputPath);
			return true;
		}

		static Patch ParsePatchFile(string sequenceFile) {
			string[] patchLines = sequenceFile.Replace("\r\n", "\n").Split('\n');

			SequencePair firstPair = new SequencePair();
			firstPair.Original = ParseHexByteSequence(TrimVariablePart(patchLines[1]));
			firstPair.Patched = ParseHexByteSequence(TrimVariablePart(patchLines[3]));

			SequencePair secondPair = new SequencePair();
			secondPair.Original = ParseHexByteSequence(TrimVariablePart(patchLines[7]));
			secondPair.Patched = ParseHexByteSequence(TrimVariablePart(patchLines[9]));

			int offset;
			int.TryParse(TrimVariablePart(patchLines[5]), out offset);

			Patch patch = new Patch();
			patch.RelativeOffset = offset;
			patch.FirstSequence = firstPair;
			patch.SecondSequence = secondPair;
			return patch;
		}
		
		private static List<PatternOffsets> FindSequencePattern(byte[] originalBytes, SequencePair firstSequence, SequencePair secondSequence, int relativeOffset) {
			var offsets = new List<PatternOffsets>();
			var currentOffset = 0;
			var originalLength = originalBytes.Length;
			while (currentOffset < originalLength) {
				int firstSequenceIndex = FindSequence(originalBytes, firstSequence.Original, currentOffset, originalLength);
				if (firstSequenceIndex == -1) {
					break;
				}
				int secondOffset = firstSequenceIndex + firstSequence.Original.Length + relativeOffset; // Start from where found the first sequence, offseted by some bytes
				int maxLookupLength = secondOffset + secondSequence.Original.Length; // Only find the next sequence within the acceptable range
				int secondSequenceIndex = FindSequence(originalBytes, secondSequence.Original, secondOffset, maxLookupLength);
				if (secondSequenceIndex == -1) {
					currentOffset = secondOffset + secondSequence.Original.Length;
					continue;
				}
				offsets.Add(new PatternOffsets { FirstOffset = firstSequenceIndex, SecondOffset = secondSequenceIndex });
				currentOffset = secondSequenceIndex + secondSequence.Original.Length; // Continue looking for our pattern from this offset, until none is found
			}
			return offsets;
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
				
		static string TrimVariablePart(string inputLine) {
			int index = inputLine.IndexOf(':') + 2; // include character and space
			return inputLine.Substring(index); 
		}

		static byte[] ParseHexByteSequence(string line) {
			string[] parts = line.Split(' ');
			return parts.Select(part => Convert.ToByte(part, 16)).ToArray();
		}
	}
	
	class Patch {
		public SequencePair FirstSequence;
		public SequencePair SecondSequence;
		public int RelativeOffset;
	}
	
	struct PatternOffsets {
		public int FirstOffset;
		public int SecondOffset;
	}

	struct SequencePair {
		public byte[] Original { get; set; }
		public byte[] Patched { get; set; }
	}

}
