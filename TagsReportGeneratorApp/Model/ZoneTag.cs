using System;
using LiteDB;

namespace TagsReportGeneratorApp.Model
{
    public class ZoneTag
    {
        [BsonId(true)]
        public int Id { get; set; }

        public Guid ZoneUuid { get; set; }
        public Guid TagUuid { get; set; }
    }
}
