using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FIR
{
	class Program
	{
		public static void Main()
		{
			const int headerColumns = 1;
			var weight = GetWeights(128 + 1).ToArray();
			//var weight = System.IO.File.ReadAllLines("weight.txt").Select(x => float.Parse(x)).ToArray();
			var lines = System.IO.File.ReadAllLines("input.txt")
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x => x.Replace(',', '.'))
				.Select(x => x.Split('\t'))
				.ToArray();

			bool hasHeader = lines[0].Any(x => Regex.IsMatch(x, @"[^0-9..\s]"));
			var header = hasHeader ? lines[0] : new string[0];
			var rows = lines.Skip(1).ToArray();

			var input = rows
				.Select(x => x.Skip(headerColumns).Select(y => (y == string.Empty || y == "#N/A") ? 0.0f : float.Parse(y)).ToArray()).ToArray();

			float[][] output = new float[rows.Length].Select(x => new float[input[0].Length]).ToArray();
			for (int j = 0; j < input.Length; j++)
			{
				for (int i = 0; i < input[0].Length; i++)
				{
					float result = 0.0f;
					for (int n = 0; n < weight.Length; n++)
					{
						int index = j + n - (weight.Length - 1) / 2;
						float value = input[Math.Max(0, Math.Min(index, input.Length - 1))][i];
						result += value * weight[n];
					}

					output[j][i] = result;
					//output[j][i] = Math.Max(result, input[j][i]);
				}
			}

			//Normalize(output, 0.95f);
			var firstRow = new[] { string.Join("\t", header) };
			var cols = output.Select((x, i) => string.Join("\t", Enumerable.Range(0, headerColumns)
														   .Select(y => rows[i][y])
														   .Concat(x.Select(y => y.ToString()))));

			System.IO.File.WriteAllText("output.txt", string.Join("\r\n", firstRow.Concat(cols)));
		}

		static void Normalize(float[][] values, float cap)
		{
			for (int i = 0; i < values[0].Length; i++)
			{
				float min = float.MaxValue, max = float.MinValue;
				for (int j = 0; j < values.Length; j++)
				{
					float value = values[j][i];
					min = Math.Min(min, value);
					max = Math.Max(max, value);
				}

				if ((max - min) > 0.0f)
				{
					for (int j = 0; j < values.Length; j++)
					{
						values[j][i] = (values[j][i] - min) / (max - min) * cap;
					}
				}
			}
		}

		static IEnumerable<float> GetWeights(int taps)
		{
			var weights = Gaussian(taps).ToArray();
			var sum = weights.Sum();
			return weights.Select(x => x / sum).ToArray();
		}

		static IEnumerable<float> Gaussian(int taps)
		{
			for (int t = 0; t < taps; t++)
			{
				var x = (t - (taps - 1) / 2.0f) / ((taps - 1) / 5.0f);
				var weight = (float)(Math.Exp(-0.5f * x * x) / taps);
				yield return weight;
			}
		}
	}
}