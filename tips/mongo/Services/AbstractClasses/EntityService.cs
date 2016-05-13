using System.Collections.Generic;
using Mongo.Data.DbConnection;
using Mongo.Data.Entities.Abstraction.Interfaces;
using Mongo.Data.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Mongo.Data.Services.AbstractClasses
{
    public abstract class EntityService<T> : IEntityService<T> where T : IMongoEntity
    {
        protected readonly MongoConnectionHandler<T> MongoConnectionHandler;

        protected EntityService()
        {
            MongoConnectionHandler = new MongoConnectionHandler<T>();
        }

        public virtual void Create(T entity)
        {
            //// Save the entity with safe mode (WriteConcern.Acknowledged)
            var result = this.MongoConnectionHandler.MongoCollection.Save(
                entity,
                new MongoInsertOptions
                {
                    WriteConcern = WriteConcern.Acknowledged
                });

            if (result.HasLastErrorMessage)//!ok
            {
                //// Something went wrong
            }
        }

        public virtual void Delete(string id)
        {
            var result = this.MongoConnectionHandler.MongoCollection.Remove(
                Query<T>.EQ(e => e.Id,
                new ObjectId(id)),
                RemoveFlags.None,
                WriteConcern.Acknowledged);

            if (result.HasLastErrorMessage)//!ok
            {
                //// Something went wrong
            }
        }

        public virtual T GetById(string id)
        {
            var entityQuery = Query<T>.EQ(e => e.Id, new ObjectId(id));
            return this.MongoConnectionHandler.MongoCollection.FindOne(entityQuery);
        }

        public abstract void Update(T entity);

        public IEnumerable<T> GetAll()
        {
            return MongoConnectionHandler.MongoCollection.FindAll();
        }
    }
}