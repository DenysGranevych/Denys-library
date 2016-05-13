using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDbLibrary
{
    public static class TestMongoOfficial
    {
        static IMongoClient _client;
        static IMongoDatabase _database;

        public async static void Test()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("test");

            var collection = _database.GetCollection<BsonDocument>("user");
            var user1 = new BsonDocument()
            {
                 { "name", "User1" },
                 { "role", new BsonArray { "user", "amdin" } },
                 {"project", new BsonDocument()
                 {
                      { "name", "Project1" },
                      { "date", new DateTime(2014, 10, 1, 0, 0, 0, DateTimeKind.Utc) },
                 }}

            };
            var user2 = new BsonDocument()
            {
                 { "name", "User2" },
                 { "role", new BsonArray { "user"} },
                 {"project", new BsonDocument()
                 {
                      { "name", "Project7" },
                      { "date", new DateTime(2014, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
                 }}

            };
            if (!collection.AsQueryable().Any())
            {
                //TODO insert
                collection.InsertOne(user1);
                collection.InsertOne(user2);
            }
            else
            {
                //TODO other 
                //var filterBuilder = Builders<BsonDocument>.Filter;
                //var filter = filterBuilder.Gt("i", 50) & filterBuilder.Lte("i", 100);
                //var documents = SpeCollection.AsQueryable();
                //var json = Json(documents, JsonRequestBehavior.AllowGet);

                //TODO filter 
                var projection = Builders<BsonDocument>.Projection.Include("name");// Exclude("_id"); //TODO set use field

                var filterTest = Builders<BsonDocument>.Filter.Where(item => item["role"].AsBsonArray.Contains("user"));

                //await collection.Find(filterTest).MongoProject(projection)
                //    .ForEachAsync(x =>  Console.WriteLine(x.ToString()));
                //Console.WriteLine(documentSomeField.ToString());

                //TODO cursor 
                //var filter = Builders<BsonDocument>.Filter.Eq(item => item["name"], "User2");// "name", "User2");//BsonDocument - user
                //using (var cursor = await collection.FindAsync(filter))
                //{
                //    var res = cursor.ToList();
                //    while (await cursor.MoveNextAsync())
                //    {
                //        var batch = cursor.Current;
                //        foreach (var document in batch)
                //        {
                //            // process document
                //            Console.WriteLine(document.ToString());
                //        }
                //    }
                //}
            }

            //var documentIn = new BsonDocument
            //{
            //    { "address" , new BsonDocument
            //        {
            //            { "street", "2 Avenue" },
            //            { "zipcode", "10075" },
            //            { "building", "1480" },
            //            { "coord", new BsonArray { 73.9557413, 40.7720266 } }
            //        }
            //    },
            //    { "borough", "Manhattan" },
            //    { "cuisine", "Italian" },
            //    { "grades", new BsonArray
            //        {
            //            new BsonDocument
            //            {
            //                { "date", new DateTime(2014, 10, 1, 0, 0, 0, DateTimeKind.Utc) },
            //                { "grade", "A" },
            //                { "score", 11 }
            //            },
            //            new BsonDocument
            //            {
            //                { "date", new DateTime(2014, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            //                { "grade", "B" },
            //                { "score", 17 }
            //            }
            //        }
            //    },
            //    { "name", "Vella" },
            //    { "restaurant_id", "41704620" }
            //};

            //var collection = _database.GetCollection<BsonDocument>("restaurants");
            ////collection.InsertOne(document);
            //var test = collection.AsQueryable().Any();
            //using (var cursor = await _client.ListDatabasesAsync())
            //{
            //    await cursor.ForEachAsync(d => Console.WriteLine(d.ToString()));
            //}
            //var documentOut = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
            //Console.WriteLine(documentOut.ToString());
        }
    }
}
