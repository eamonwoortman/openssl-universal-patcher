using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenSSLUniversalPatcher {

class ByteSequencePatcher {
	public static bool Apply(string inputFile, string outputFile, string patchFile) {
		if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile) || string.IsNullOrEmpty(patchFile)) {
			Console.WriteLine("Usage: ByteSequencePatcher <input_file> <output_file> <target_sequence_file>");
			return false;
		}

		int sequencesPatched = 0;
		byte[] originalBytes = File.ReadAllBytes(inputFile);
		List<SequencePair> sequencePairs = LoadSequencePairs(patchFile);

		foreach (var pair in sequencePairs) {
			int index = FindSequence(originalBytes, pair.Original);

			if (index >= 0) {
				Console.WriteLine("Sequence found at index: " + index);

				// Apply the patch
				Array.Copy(pair.Patched, 0, originalBytes, index, pair.Patched.Length);
				sequencesPatched++;
			} else {
				Console.WriteLine("Original sequence not found in the input file.");
			}
		}

		// Write the patched bytes to the output file
		if (sequencesPatched > 0) {
			File.WriteAllBytes(outputFile, originalBytes);
			Console.WriteLine("Patched file written to " + outputFile);
			return true;
		}
		return false;
	}

	static List<SequencePair> LoadSequencePairs(string sequenceFile) {
		string sequenceRegex = @"^(original|patched):\r?\n(?:#.*\r?\n)([0-9A-Fa-f]+(?:\s[0-9A-Fa-f]+)*)";
		string sequenceText = File.ReadAllText(sequenceFile);
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

		for (int i = 0; i < matches.Count; i += 2) {
			Match originalMatch = matches[i];
			Match patchedMatch = matches[i + 1];

			// Assert it's valid
			if (originalMatch.Groups[1].Value != "original" || patchedMatch.Groups[1].Value != "patched") {
				Console.WriteLine("Couldn't find proper sequence groups");
				continue;
			}

			string originalSequence = originalMatch.Groups[2].Value;
			string patchedSequence = patchedMatch.Groups[2].Value;

			if (string.IsNullOrEmpty(originalSequence) || string.IsNullOrEmpty(patchedSequence)) {
				Console.WriteLine("Original or patched byte sequences are invalid");
				continue;
			}

			byte[] original = ParseByteSequence(originalSequence);
			byte[] patched = ParseByteSequence(originalSequence);

			sequencePairs.Add(new SequencePair { Original = original, Patched = patched });
		}

		return sequencePairs;
	}

	static byte[] ParseByteSequence(string line) {
		string[] parts = line.Split(' ');
		return parts.Select(part => Convert.ToByte(part, 16)).ToArray();
	}

	public static int FindSequence(byte[] data, byte[] sequence)
	{
		int offset = -1;

		for (int i = 0; i < data.Length - sequence.Length + 1; i++)
		{
			bool found = true;
			for (int j = 0; j < sequence.Length; j++)
			{
				if (data[i + j] != sequence[j])
				{
					found = false;
					break;
				}
			}

			if (found)
			{
				offset = i;
				break;
			}
		}

		return offset;
	}
}

class SequencePair {
	public byte[] Original { get; set; }
	public byte[] Patched { get; set; }
}
}
