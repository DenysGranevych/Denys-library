using Mongo.Data.Entities.Abstraction.Interfaces;

namespace Mongo.Data.Services.Interfaces
{
    public interface IEntityService<T> where T : IMongoEntity
    {
        void Create(T entity);

        void Delete(string id);

        T GetById(string id);

        void Update(T entity);
    }
}