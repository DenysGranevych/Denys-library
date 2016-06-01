using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mongo.Data.Entities.Concrete;
using Mongo.Data.Properties;
using Mongo.Data.Repositories.Abstraction;
using Mongo.Data.Repositories.Concrete.GenericRepository;
using MongoDB.Driver;

namespace Mongo.Data.Repositories.Concrete
{
    public class MongoUserAnswerRepository : GenericMongoRepository<MongoProject>, IMongoUserAnswerRepository
    {
        public void Insert(string projectName, string taskName, string sliceKey, string questionKey, IEnumerable<string> answerKeys,
            string answerText)
        {
            var projectTaskIndex = -1;
            try
            {
                projectTaskIndex = Collection
              .Find(p => p.Name == projectName)
              .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName)).Single();
            }
            catch (Exception)
            {
                throw new ArgumentNullException(Resources.MongoDbCannotInsertUserAnswer);
            }

            var sliceKeyIfExist = Collection.Find(p => p.Name == projectName
                                                       && p.Tasks.Any(t => t.Name == taskName))
                .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.FirstOrDefault(s => s.Key == sliceKey)).FirstOrDefault();

            if (sliceKeyIfExist == null)
            {
                TaskAddSlice(projectName, taskName, sliceKey);
            }

            var projectSliceIndex = Collection
                 .Find(p => p.Name == projectName)
                 .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.ToList().FindIndex(s => s.Key == sliceKey))
                 .Single();


            var updateDefinition = new UpdateDefinitionBuilder<MongoProject>()
                .AddToSet(p => p.Tasks[projectTaskIndex].Slices[projectSliceIndex].Questions, new MongoQuestion()
            {
                Key = questionKey,
                AnswerText = answerText,
                Answers = answerKeys.ToList()
            });

            Collection.UpdateOneAsync(p => p.Name == projectName, updateDefinition);
        }

        public async void Delete(string projectName, string taskName, string sliceKey, string questionKey)
        {
            var projectTaskIndex = await Collection
              .Find(p => p.Name == projectName)
              .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName))
              .SingleAsync();

            var projectSliceIndex = await Collection
             .Find(p => p.Name == projectName)
             .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.ToList().FindIndex(s => s.Key == sliceKey))
             .SingleAsync();

            var update = Builders<MongoProject>.Update.PullFilter(x => x.Tasks[projectTaskIndex].Slices[projectSliceIndex].Questions,
                question => question.Key == questionKey);

            await Collection.UpdateOneAsync(p => p.Name == projectName, update);
        }

        public void DeleteAllInSlice(string projectName, string taskName, string sliceKey)
        {
            var projectTaskIndex = Collection
              .Find(p => p.Name == projectName)
              .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName))
              .Single();

            var update = Builders<MongoProject>.Update.PullFilter(x => x.Tasks[projectTaskIndex].Slices,
                slice => slice.Key == sliceKey);

            Collection.UpdateOne(p => p.Name == projectName, update);
        }

        public async void InsertMany(string projectName, string taskName, string sliceKey, IEnumerable<MongoQuestion> questions)
        {
            var projectTaskIndex = -1;
            try
            {
                projectTaskIndex = await Collection
              .Find(p => p.Name == projectName)
              .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName)).SingleAsync();
            }
            catch (Exception)
            {
                throw new ArgumentNullException(Resources.MongoDbCannotInsertUserAnswer);
            }

            var sliceKeyIfExist = await Collection.Find(p => p.Name == projectName
                                                       && p.Tasks.Any(t => t.Name == taskName))
                .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.FirstOrDefault(s => s.Key == sliceKey)).FirstOrDefaultAsync();

            if (sliceKeyIfExist == null)
            {
                TaskAddSlice(projectName, taskName, sliceKey);
            }

            var projectSliceIndex = await Collection
                 .Find(p => p.Name == projectName)
                 .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.ToList().FindIndex(s => s.Key == sliceKey))
                 .SingleAsync();

            var updateDefinition = new UpdateDefinitionBuilder<MongoProject>()
                .AddToSetEach(p => p.Tasks[projectTaskIndex].Slices[projectSliceIndex].Questions, questions);

            Collection.UpdateOne(p => p.Name == projectName, updateDefinition);
        }

        private void TaskAddSlice(string projectName, string taskName, string sliceKey)
        {
            var filter = Builders<MongoProject>.Filter
               .Where(x => x.Name == projectName && x.Tasks.Any(i => i.Name == taskName));
            var update = Builders<MongoProject>.Update.Push(x => x.Tasks.ElementAt(-1).Slices, new MongoSlice()
            {
                Key = sliceKey
            });

            Collection.UpdateOne(filter, update);
        }
    }
}