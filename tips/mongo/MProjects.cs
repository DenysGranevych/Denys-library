using System.Collections.Generic;
using System.Linq;
using Mongo.Data.Entities.Concrete;
using Mongo.Data.Properties;
using MongoDB.Driver;
using System;

namespace Mongo.Data
{

    public static class MProjects
    {
        private static readonly IMongoDatabase Database;

        static MProjects()
        {
            var connectionString = Settings.Default.MongoConnectionString;
            var mongoClient = new MongoClient(connectionString);

            Database = mongoClient.GetDatabase(Settings.Default.MongoDBName);
        }

        public static async void TestFilter()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("projects");

            var findFluent = collectionProjects.Find(Builders<MongoProject>.Filter.ElemMatch(
                foo => foo.Tasks,
                task => task.Name == "Task2"));

            Console.WriteLine(collectionProjects.Find(f => f.Tasks.Any(fb => fb.Name == "Task23")).Any());

            var res = findFluent;
        }
        public static async void TestUpdateNestedField()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("projects");

            var filter = Builders<MongoProject>.Filter.Where(x => x.Tasks.Any(t => t.Name == "Task2"));
            var update = Builders<MongoProject>.Update.Set(x => x.Tasks[-1].Name, "TaskNewName");
            var result = collectionProjects.UpdateOneAsync(filter, update).Result;
            Console.WriteLine(result);
        }

        public static void NestedArrrayAddItem()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("projects");

            var filter = Builders<MongoProject>.Filter.Where(x => x.Name == "TestProject");
            //var update = Builders<MessageDto>.Update.Push(x => x.NestedArray.ElementAt(-1).Users, new User { UserID = userID });
            //TODO for nested in nested
            var update = Builders<MongoProject>.Update.Push("Tasks", new MongoTask
            {
                Name = "inserted task",
                Slices = new List<MongoSlice>()
            });
            var result = collectionProjects.UpdateOneAsync(filter, update).Result;
            Console.WriteLine(result);
        }

        public static void TestAddSlice()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("mongoprojects");

            var filter = Builders<MongoProject>.Filter
                .Where(x => x.Name == "fixMongoEmptyArrayTest" && x.Tasks.Any(i => i.Name == "tesdafsd"));
            var update = Builders<MongoProject>.Update
                .Push(x => x.Tasks.ElementAt(-1).Slices, new MongoSlice() { Key = "testSlice"});
            ////TODO for nested in nested
            //var update = Builders<MongoProject>.Update.Push("Tasks", new MongoTask
            //{
            //    Name = "inserted task",
            //    Slices = new List<MongoSlice>()
            //});
            var result = collectionProjects.UpdateOneAsync(filter, update).Result;
            Console.WriteLine(result);
        }

        public static void NestedArrrayRemoveItem()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("projects");

            var filter = Builders<MongoProject>.Filter.Where(x => x.Name == "TestProject");

            //var pull = Update<MongoProject>.Pull(x => x.Tasks, builder =>
            //    builder.Where(q => q.Name == "TestProject"));

            var update = Builders<MongoProject>.Update.PullFilter(x => x.Tasks, task => task.Name == "inserted task");

            var result = collectionProjects.UpdateOneAsync(filter, update).Result;
            Console.WriteLine(result);
        }

        public static void InsertDefaul()
        {
            var collectionProjects = Database.GetCollection<MongoProject>("projects");

            //collectionProjects.InsertOne(new MongoProject
            //{
            //    Name = "TestProjectNewStucture",
            //    Tasks = new List<MongoTask>
            //    {
            //        new MongoTask 
            //        {
            //            Name = "Task1",
            //            UserEmail = "user@com.ua",
            //            Slices = new List<MongoSlice>
            //            {
            //                new MongoSlice
            //                {
            //                    Key = "sliceKey",
            //                    Questions = new List<MongoQuestion>{
            //                        new MongoQuestion
            //                        {
            //                            Key = "qoestion Key1",
            //                            Answers = new List<MongoAnswer>
            //                            {
            //                                new MongoAnswer
            //                                {
            //                                    Key = "answer 1",
            //                                },
            //                                new MongoAnswer
            //                                {
            //                                    Text = "someText2"
            //                                }
            //                            }
            //                        },
            //                         new MongoQuestion
            //                        {
            //                            Key = "qoestion Key2",
            //                            Answers = new List<MongoAnswer>
            //                            {

            //                                new MongoAnswer
            //                                {
            //                                    Key = "answer 1 q2",
            //                                },
            //                                new MongoAnswer
            //                                {
            //                                    Text = "someText q2",
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }, new MongoTask
            //        {
            //            Name = "Task2",
            //            UserEmail = "user3@com.ua",
            //            Slices = new List<MongoSlice>()
            //        }
            //    }
            //});
        }
    }
}
