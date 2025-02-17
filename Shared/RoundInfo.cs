using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RoundInfo
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    //        [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement]
    public string CourseName { get; set; } = "Unknown";
    [BsonElement]
    public DateOnly DateOfPlay { get; set; } = DateOnly.FromDateTime(DateTime.Now);

}
