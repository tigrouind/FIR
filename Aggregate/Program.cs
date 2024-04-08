using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aggregate
{
	class Program
	{
		public static void Main()
		{
			TimeSpan average = TimeSpan.FromMinutes(1);
			Func<DateTime, DateTime> round = x => new DateTime((x.Ticks / average.Ticks) * average.Ticks);

			var lines = System.IO.File.ReadAllLines("input.txt")
				.Select(x => x.Replace(",", "."))
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();

			var hasHeader = lines.Any() && Regex.IsMatch(lines[0], @"[^0-9\.\s]");
			var header = lines.Take(hasHeader ? 1 : 0);
			var rows = lines.Skip(1).ToArray();

			var result = rows
							.Select(x => x.Split('\t'))
							.Select(x => new Tuple<DateTime, float[]>(round(DateTime.Parse(x[0])), x.Skip(1).Select(y => float.Parse(y)).ToArray()))
							.GroupBy(x => x.Item1, x => x.Item2)
							.Select(x => (new[] { (float)x.Key.TimeOfDay.TotalMinutes }).Concat(Transpose(x.ToArray()).Select(y => y.Max())).ToArray())
							.ToArray();

			result = Transpose(Process(Transpose(result)));

			System.IO.File.WriteAllLines(@"output.txt",
										header.Concat(result.Select(x => new DateTime((long)x[0] * TimeSpan.TicksPerMinute).ToString("HH:mm")
																	+ "\t" + string.Join("\t", x.Skip(1)))));
		}

		static float[][] Process(float[][] input)
		{
			var largeValue = input.Select(row => row.Any(x => x >= 10000)).ToArray();

			input = input.Select(x => x.Select(y => y >= 1000 * 1000 * 1000 ? 0.0f : y).ToArray()).ToArray();

			return Enumerable.Range(0, input.Length)
				.Select(row => largeValue[row] ? input[row].Select(x => x / (1024 * 1024)).ToArray() : input[row])
				.ToArray();
		}

		static float[][] Transpose(float[][] input)
		{
			int columns = input.Min(row => row.Length);
			return Enumerable.Range(0, columns)
				.Select(col => input.Select(row => row[col]).ToArray())
				.ToArray();
		}
	}
}