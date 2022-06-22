namespace IgbExcelDemo.Client;

/// <summary>
/// 指定された日付に発生した地震の回数を表します。
/// </summary>
public record EarthquakeCountParDay(DateTime Date, int Count);
