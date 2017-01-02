namespace NewOlympicsBidder.Migrations
{
    using NewOlympicsBidder.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal sealed class Configuration : DbMigrationsConfiguration<NewOlympicsBidder.Models.OlympicContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(NewOlympicsBidder.Models.OlympicContext context)
        {
            context.Team.AddOrUpdate(x => x.Name,
                new Team { Name = "Team A" },
                new Team { Name = "Team B" },
                new Team { Name = "Team C" },
                new Team { Name = "Team D" }
            );

            var csvString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Participants.csv"));

            var jsonString = csvString.CsvToJson();

            var jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonString);
            
            context.Participant.AddOrUpdate(x => x.Name,
                jsonArray.ToList().Select(y => 
                {
                    var participant = new Participant { Name = y["Name"].ToString(), IsCurrent = false, Expertise = y.ToString() };
                    
                    return participant;
                })
                .ToArray()
            );

            context.SaveChanges();
            
            var randomParticipant = context.Participant.OrderBy(x => Guid.NewGuid()).First();

            randomParticipant.IsCurrent = true;

            context.SaveChanges();
        }
    }

    public static class Utility
    {
        public static string[] SplitQuotedLine(this string value, char separator)
        {
            // Use the "quotes" bool if you need to keep/strip the quotes or something...
            var s = new List<string>();
            var regex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
            foreach (Match m in regex.Matches(value))
            {
                s.Add(m.Value);
            }
            return s.ToArray();
        }

        public static string CsvToJson(this string value)
        {
            // Get lines.
            if (value == null) return null;
            string[] lines = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) throw new InvalidDataException("Must have header line.");

            // Get headers.
            string[] headers = lines.First().SplitQuotedLine(',');

            // Build JSON array.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields = lines[i].SplitQuotedLine(',');
                if (fields.Length != headers.Length) throw new InvalidDataException("Field count must match header count.");
                var jsonElements = headers.Zip(fields, (header, field) => string.Format("\"{0}\": \"{1}\"", header, field)).ToArray();
                string jsonObject = "{" + string.Format("{0}", string.Join(",", jsonElements)) + "}";
                if (i < lines.Length - 1)
                    jsonObject += ",";
                sb.AppendLine(jsonObject);
            }
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}
