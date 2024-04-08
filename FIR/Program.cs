using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FIR
{
	class Program
	{
		public static void Main()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

			float[] weight;
			if (File.Exists("weights.txt") && new FileInfo("weights.txt").Length > 0)
			{
				weight = File.ReadAllLines("weights.txt")
					.Select(x => float.Parse(x.Replace(",", "."), CultureInfo.InvariantCulture))
					.ToArray();
			}
			else
			{
				weight = GetWeights(21).ToArray();
			}

			var lines = File.ReadAllLines("input.txt").Select(x => x.Split('\t', ' ')).ToArray();
			Thread.CurrentThread.CurrentCulture = new CultureInfo(lines.SelectMany(x => x).Any(x => x.Contains(",")) ? "fr-FR" : "en-US");

			var input = lines.Skip(lines[0].Any(x => Regex.IsMatch(x, @"[^0-9,.\s]")) ? 1 : 0)
				.Select(x => x.Select(y => (y == string.Empty || y == "#N/A") ? 0.0f : float.Parse(y)).ToArray()).ToArray();

			float[][] output = new float[lines.Length].Select(x => new float[input[0].Length]).ToArray();
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
				}
			}

			//Normalize(output, 0.95f);
			File.WriteAllText("output.txt", string.Join("\r\n", output.Select(x => string.Join("\t", x))));
		}

		/*static void Normalize(float[][] values, float cap)
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
		}*/

		static IEnumerable<float> GetWeights(int taps)
		{
			var weights = Gaussian().ToArray();
			var sum = weights.Sum();
			return weights.Select(x => x / sum).ToArray();

			IEnumerable<float> Gaussian()
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
}