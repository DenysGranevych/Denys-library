using System.Collections.Generic;
using Mongo.Data.Entities.Concrete;
using Mongo.Data.Services.AbstractClasses;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Mongo.Data.Services.Concrete
{
    public class UserService : EntityService<TestUser>
    {
        public IEnumerable<TestUser> GetGamesDetails(int limit, int skip)
        {
            var gamesCursor = this.MongoConnectionHandler.MongoCollection.FindAllAs<TestUser>()
                .SetSortOrder(SortBy<TestUser>.Descending(g => g.FirstName))
                .SetLimit(limit)
                .SetSkip(skip)
                .SetFields(Fields<TestUser>.Include(g => g.Id, g => g.FirstName, g => g.Age, g => g.Roles));//, g => g.LastName
            return gamesCursor;
        }


        public override void Update(TestUser entity)
        {
            var updateResult = this.MongoConnectionHandler.MongoCollection.Update(
                   Query<TestUser>.EQ(p => p.Id, entity.Id),
                   Update<TestUser>.Set(p => p.FirstName, entity.FirstName)
                       .Set(p => p.Age, entity.Age).Set(p => p.LastName, entity.LastName),
                   new MongoUpdateOptions
                   {
                       WriteConcern = WriteConcern.Acknowledged
                   });

            if (updateResult.DocumentsAffected == 0)
            {
                //// Something went wrong

            }
        }
    }
}