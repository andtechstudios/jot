using System.Diagnostics;

var journalDirectory = Environment.GetEnvironmentVariable("JOT_DIR");
if (string.IsNullOrEmpty(journalDirectory))
{
	Console.WriteLine("Please assign environment variable JOT_DIR");
	Environment.Exit(1);
}
if (!Directory.Exists(journalDirectory))
{
	Console.WriteLine($"JOT_DIR {journalDirectory} doesn't exist. Please create it first.");
	Environment.Exit(1);
}

var today = DateTime.Now.Date;
var firstOfMonth = new DateTime(today.Year, today.Month, 1);
var lastOfMonth = firstOfMonth.AddDays(DateTime.DaysInMonth(today.Year, today.Month));
var previousMonday = GetPreviousMonday(today);
var nextMonday = GetPreviousMonday(today).AddDays(7);
var startOfWeek = firstOfMonth > previousMonday ? firstOfMonth : previousMonday;
var endOfWeek = lastOfMonth < nextMonday ? lastOfMonth : nextMonday;
var weekNumber = GetWeekNumber(today);
var timecode = $"{today.Year:0000}/{today.Month:00}/week{weekNumber}";
var path = Path.Combine(journalDirectory, $"{timecode}.md");

// Create journal page (if it doesn't exist)
if (!File.Exists(path))
{
	var lines = string.Join(Environment.NewLine,
		"---",
		$"title: \"{today:MMMM} Week #{weekNumber}\"",
		$"date: {ToUniversalIso8601(startOfWeek)}",
		$"publishDate: {ToUniversalIso8601(endOfWeek)}",
		"---"
	) + Environment.NewLine;
	Directory.CreateDirectory(Path.GetDirectoryName(path));
	File.WriteAllText(path, lines);
}

// Launch editor process
Process
	.Start(Environment.GetEnvironmentVariable("EDITOR"), $"{path}:2147483647")
	.WaitForExit();

// Helper functions
DateTime GetPreviousMonday(DateTime start)
{
	int daysToAdd = ((int)start.DayOfWeek + 7 - (int)DayOfWeek.Monday) % 7;
	return start.AddDays(-daysToAdd);
}
int GetWeekNumber(DateTime date)
{
	var origin = GetPreviousMonday(firstOfMonth);
	var indexF = (date - origin).TotalDays / 7.0;
	var index = (int)Math.Floor(indexF);
	return index;
}
string ToUniversalIso8601(DateTime dateTime) => dateTime.ToString("u").Replace(" ", "T");
