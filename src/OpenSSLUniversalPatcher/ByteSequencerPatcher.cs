using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenSSLUniversalPatcher {

	class ByteSequencePatcher {
		public static bool Apply(string inputPath, string outputPath, string patchFile) {
			Debug.Assert(!string.IsNullOrEmpty(inputPath));
			Debug.Assert(!string.IsNullOrEmpty(outputPath));
			Debug.Assert(!string.IsNullOrEmpty(patchFile));

			byte[] originalBytes = File.ReadAllBytes(inputPath);
			List<SequencePair> sequencePairs = LoadSequencePairs(patchFile);

			if (sequencePairs.Count != 2) {
				return false;
			}

			var first = sequencePairs[0];
			var second = sequencePairs[1];

			List<PatternOffsets> offsets = FindSequencePattern(originalBytes, first, second);
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
			Array.Copy(first.Patched, 0, originalBytes, firstOffset, first.Patched.Length);

			// Apply the second sequence patch
			Array.Copy(second.Patched, 0, originalBytes, secondOffset, second.Patched.Length);

			// Write the patched bytes to the output file
			File.WriteAllBytes(outputPath, originalBytes);

			Console.WriteLine("Patched file written to " + outputPath);
			return true;
		}

		struct PatternOffsets {
			public int FirstOffset;
			public int SecondOffset;
		}

		private static List<PatternOffsets> FindSequencePattern(byte[] originalBytes, SequencePair first, SequencePair second) {
			List<PatternOffsets> offsets = new List<PatternOffsets>();
			int currentOffset = 0;
			int originalLength = originalBytes.Length;
			while (currentOffset < originalLength) {
				int firstSequenceIndex = FindSequence(originalBytes, first.Original, currentOffset, originalLength);
				if (firstSequenceIndex == -1) {
					break;
				}
				int secondOffset = firstSequenceIndex + first.Original.Length + second.Offset;
				int secondSequenceIndex = FindSequence(originalBytes, second.Original, secondOffset, secondOffset + second.Original.Length);
				if (secondSequenceIndex == -1) {
					currentOffset = secondOffset + second.Original.Length;
					continue;
				}
				
				PatternOffsets offsetPair = new PatternOffsets { FirstOffset = firstSequenceIndex, SecondOffset = secondSequenceIndex };
				offsets.Add(offsetPair);

				currentOffset = secondSequenceIndex + second.Original.Length;
			}
			return offsets;
		}

		static List<SequencePair> LoadSequencePairs(string sequenceFile) {
			string sequenceRegex = @"^(?:#.*\r?\n)?(?:offset: (?<offset>[\-]?[0-9]+)\r?\n)?(?:#.*\r?\n)original: (?<original>[0-9A-Fa-f]+(?:\s[0-9A-Fa-f]+)*)\r?\n(?:#.*\r?\n)?patched: (?<patched>[0-9A-Fa-f]+(?:\s[0-9A-Fa-f]+)*)";
			string sequenceText = sequenceFile;
			List<SequencePair> sequencePairs = new List<SequencePair>();

			MatchCollection matches = Regex.Matches(sequenceText, sequenceRegex, RegexOptions.Multiline);
			if (matches.Count == 0) {
				Console.WriteLine("No valid sequence found...");
				return sequencePairs;
			}

			if (matches.Count % 2 != 0) {
				Console.WriteLine("Invalid sequence count...");
				return sequencePairs;
			}

			foreach (Match match in matches) {
				SequencePair sequencePair = new SequencePair();
				string offset = match.Groups["offset"].Value;
				int.TryParse(offset, out sequencePair.Offset);

				string originalSequence = match.Groups["original"].Value;
				string patchedSequence = match.Groups["patched"].Value;
				
				byte[] original = ParseByteSequence(originalSequence);
				byte[] patched = ParseByteSequence(patchedSequence);

				sequencePair.Original = original;
				sequencePair.Patched = patched;

				sequencePairs.Add(sequencePair);
			}

			return sequencePairs;
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

	class SequencePair {
		public int Offset;
		public byte[] Original { get; set; }
		public byte[] Patched { get; set; }
	}
}
