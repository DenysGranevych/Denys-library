using System;
using System.Collections.Generic;
using System.Linq;
using Mongo.Data.Entities.Concrete;
using Mongo.Data.Properties;
using Mongo.Data.Repositories.Abstraction;
using Mongo.Data.Repositories.Concrete.GenericRepository;
using MongoDB.Driver;

namespace Mongo.Data.Repositories.Concrete
{
    public class MongoTaskRepository : GenericMongoRepository<MongoProject>, IMongoTaskRepository
    {
        public void Insert(string projectName, string newName, string userEmail)
        {
            CheckProject(projectName);

            var filter = Builders<MongoProject>.Filter.Where(x => x.Name == projectName);
            var update = Builders<MongoProject>.Update.Push(x => x.Tasks, new MongoTask
            {
                Name = newName,
                UserEmail = userEmail,
                Slices = new List<MongoSlice>()
            });

            Collection.UpdateOne(filter, update);
        }

        public void Update(string projectName, string oldName, string newName, string userEmail)
        {
            CheckProject(projectName);

            var filter = Builders<MongoProject>.Filter.Where(x => x.Name == projectName && x.Tasks.Any(t => t.Name == oldName));
            var update = Builders<MongoProject>.Update
                .Set(x => x.Tasks[-1].Name, newName)
                .Set(x => x.Tasks[-1].UserEmail, userEmail);

            Collection.UpdateOne(filter, update);
        }

        private void CheckProject(string projectName)
        {
            if (Collection.Find(p => p.Name == projectName).Project(p => p.Name).SingleOrDefault() == null)
            {
                throw new ArgumentNullException(Resources.MongoDbCannotEditTask);
            }
        }
    }
}