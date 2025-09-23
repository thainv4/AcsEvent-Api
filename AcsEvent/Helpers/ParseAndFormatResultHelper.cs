using System.Text.Json;
using AcsEvent.DTOs.AcsEvent;

namespace AcsEvent.Helpers;

public class ParseAndFormatResultHelper
{
    public static object ParseAndFormatResult(string result)
    {
        try
        {
            var doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("AcsEvent", out var acsEvent) &&
                acsEvent.TryGetProperty("InfoList", out var infoList))
            {
                var records = JsonSerializer.Deserialize<List<InfoRecord>>(infoList.GetRawText());

                var grouped = records
                    .GroupBy(r => new { r.employeeNoString, r.name, Date = DateTime.Parse(r.time).Date })
                    .Select(g =>
                    {
                        var morning = g
                            .Where(x => DateTime.Parse(x.time).Hour < 12)
                            .OrderBy(x => DateTime.Parse(x.time))
                            .FirstOrDefault();

                        var afternoon = g
                            .Where(x => DateTime.Parse(x.time).Hour >= 12)
                            .OrderByDescending(x => DateTime.Parse(x.time))
                            .FirstOrDefault();

                        return new
                        {
                            macc = g.Key.employeeNoString,
                            name = g.Key.name,
                            date = g.Key.Date.ToString("yyyy-MM-dd"),
                            firstin = morning?.time,
                            lastout = afternoon?.time
                        };
                    });

                return grouped.ToList();
            }

            return new List<object>();
        }
        catch (Exception ex)
        {
            return new List<object>();
        }
    }
}