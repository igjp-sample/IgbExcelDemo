using System.Text.RegularExpressions;
using System.Xml.Linq;
using Zipangu;

namespace IgbExcelDemo.Server;

/// <summary>
/// 気象庁防災情報 XML から取得したデータに基づき、直近数日間に発生した地震の発生日時を取得するサービスです。
/// </summary>
public class EarthquakeDataService
{
    private readonly IHttpClientFactory _HttpClientFactory;

    public EarthquakeDataService(IHttpClientFactory httpClientFactory)
    {
        this._HttpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 気象庁防災情報 XML から取得したデータに基づき、直近数日間に発生した地震の発生日時の集合を返します。
    /// </summary>
    public async ValueTask<IEnumerable<DateTime>> GetDateTimesAsync()
    {
        var httpClient = this._HttpClientFactory.CreateClient();
        var xmlText = await httpClient.GetStringAsync("https://www.data.jma.go.jp/developer/xml/feed/eqvol_l.xml");

        var xdoc = XDocument.Parse(xmlText);
        var ns = "http://www.w3.org/2005/Atom";
        var dateTimes = xdoc.Descendants(XName.Get("entry", ns))
            .Select(e => new
            {
                Title = e.Element(XName.Get("title", ns))?.Value,
                Updated = DateTime.TryParse(e.Element(XName.Get("updated", ns))?.Value, out var d) ? d : DateTime.MinValue,
                Content = e.Element(XName.Get("content", ns))?.Value.AsciiToNarrow() ?? "",
            })
            .Where(entry => "震源・震度に関する情報|震源に関する情報|震度速報".Split('|').Contains(entry.Title))
            .Select(entry =>
            {
                var match = Regex.Match(entry.Content, @"(?<day>\d+)日(?<hour>\d+)時(?<minute>\d+)分");
                if (!match.Success) return DateTime.MinValue;
                var day = int.Parse(match.Groups["day"].Value);
                var hour = int.Parse(match.Groups["hour"].Value);
                var minute = int.Parse(match.Groups["minute"].Value);
                return new DateTime(entry.Updated.Year, entry.Updated.Month, day, hour, minute, second: 0);
            })
            .Where(dateTime => dateTime.Year != DateTime.MinValue.Year)
            .OrderBy(dateTime => dateTime)
            .Distinct()
            .ToArray();

        return dateTimes;
    }
}
