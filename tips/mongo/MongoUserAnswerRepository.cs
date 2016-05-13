using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mongo.Data.Entities.Concrete;
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
            var projectTaskIndex = Collection
                .Find(p => p.Name == projectName)
                .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName)).Single();

            var sliceKeyIfExist = Collection.Find(p => p.Name == projectName
                                                       && p.Tasks.Any(t => t.Name == taskName))
                .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.FirstOrDefault(s => s.Key == sliceKey)).FirstOrDefault();

            if (sliceKeyIfExist == null)
            {
                TaskAddSlice(projectName, taskName, sliceKey);
            }

            var projectSliceIndex = Collection
         .Find(p => p.Name == projectName)
         .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.ToList().FindIndex(s => s.Key == sliceKey)).Single();


            var updateDefinition = new UpdateDefinitionBuilder<MongoProject>()
                .AddToSet(p => p.Tasks[projectTaskIndex].Slices[projectSliceIndex].Questions, new MongoQuestion()
            {
                Key = questionKey,
                AnswerText = answerText,
                Answers = answerKeys.ToList()
            });

            Collection.UpdateOne(p => p.Name == projectName, updateDefinition);


            //var taskIndex = project.Tasks.IndexOf(project.Tasks.Single(t => t.Name == taskName));

            //if (!project.Tasks.Any(t => t.Name == taskName && t.Slices.Any(s => s.Key == sliceKey)))
            //{
            //    TaskAddSlice(projectName, taskName, sliceKey);
            //}

            //var filter = Builders<MongoProject>.Filter
            //  .Where(x => x.Name == projectName 
            //      && x.Tasks.Any(t => t.Name == taskName 
            //          && t.Slices.Any(s => s.Key == sliceKey)) 
            //        );
            //var update = Builders<MongoProject>.Update.Push(x => x.Tasks[taskIndex].Slices.ElementAt(-1).Questions, new MongoQuestion()
            //{
            //    Key = questionKey,
            //    AnswerText = answerText,
            //    Answers = answerKeys.ToList()
            //});

            //var result = Collection.UpdateOneAsync(filter, update).Result;

            //get all method
            //var project = Collection.Find(p => p.Name == projectName && p.Tasks.Any(t => t.Name == taskName))
            //   .Single();


            //if (!project.Tasks.Single(t => t.Name == taskName).Slices.Any(s => s.Key == sliceKey))
            //{
            //    var newSlices = project.Tasks.Single(t => t.Name == taskName).Slices.ToList();
            //    newSlices.Add(new MongoSlice()
            //    {
            //        Key = sliceKey
            //    });
            //    project.Tasks.Single(t => t.Name == taskName).Slices = newSlices;
            //}

            //var questions = project.Tasks.Single(t => t.Name == taskName).Slices.Single(s => s.Key == sliceKey).Questions.ToList();
            //questions.Add(new MongoQuestion
            //  {
            //      Key = questionKey,
            //      AnswerText = answerText,
            //      Answers = answerKeys.ToList()
            //  });

            //project.Tasks.Single(t => t.Name == taskName).Slices.Single(s => s.Key == sliceKey).Questions = questions;

            //var filter = Builders<MongoProject>.Filter.Where(x => x.Name == projectName);
            //var result = Collection.ReplaceOneAsync(filter, project);
        }

        public void Delete(string projectName, string taskName, string sliceKey, string questionKey)
        {
            var projectTaskIndex = Collection
              .Find(p => p.Name == projectName)
              .Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName)).Single();

            var projectSliceIndex = Collection
             .Find(p => p.Name == projectName)
             .Project(p => p.Tasks.Single(t => t.Name == taskName).Slices.ToList().FindIndex(s => s.Key == sliceKey)).Single();

            var update = Builders<MongoProject>.Update.PullFilter(x => x.Tasks[projectTaskIndex].Slices[projectSliceIndex].Questions,
                question => question.Key == questionKey);

            Collection.UpdateOne(p => p.Name == projectName, update);


            //old2
            //var project = Collection.Find(p => p.Name == projectName && p.Tasks.Any(t => t.Name == taskName))
            //   .Single();

            //var slices = project.Tasks.Single(t => t.Name == taskName).Slices.ToList();
            //var removedSlice = slices.Single(s => s.Key == sliceKey);

            //slices.Remove(removedSlice);
            //project.Tasks.Single(t => t.Name == taskName).Slices = slices;

            //var filter = Builders<MongoProject>.Filter.Where(x => x.Name == projectName);
            //var result = Collection.ReplaceOneAsync(filter, project);


            //old
            //var project = Collection.Find(p => p.Name == projectName && p.Tasks.Any(t => t.Name == taskName))
            //   .Single();
            //var taskIndex = project.Tasks.IndexOf(project.Tasks.Single(t => t.Name == taskName));

            //var filter = Builders<MongoProject>.Filter
            // .Where(x => x.Name == projectName
            //     && x.Tasks.Any(t => t.Name == taskName
            //         && t.Slices.Any(s => s.Key == sliceKey))
            //         && x.Tasks.Any(t => t.Name == taskName
            //             && t.Slices.Any(s => s.Key == sliceKey)));

            //var update = Builders<MongoProject>.Update.PullFilter(x => x.Tasks[taskIndex].Slices.ElementAt(-1).Questions, question => question.Key == questionKey);

            //var result = Collection.UpdateOneAsync(filter, update).Result;
        }

        private void TaskAddSlice(string projectName, string taskName, string sliceKey)
        {
            //var projectTaskIndex = Collection
            //   .Find(p => p.Name == projectName).Project(p => p.Tasks.ToList().FindIndex(t => t.Name == taskName)).Single();

            //var updateDefinition = new UpdateDefinitionBuilder<MongoProject>()
            //    .AddToSet(p => p.Tasks[projectTaskIndex].Slices, new MongoSlice
            //    {
            //        Key = sliceKey,
            //    });

            //var result = Collection.UpdateOneAsync(p => p.Name == projectName, updateDefinition).Result;

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