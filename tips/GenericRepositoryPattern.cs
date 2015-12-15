
(for BaseContext)
public interface IGenericRepository<T> where T : class {
    
    IQueryable<T> GetAll();
    IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void Delete(T entity);
    void Edit(T entity);
    void Save();
}

 public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly BaseContext _entities;
        protected readonly IDbSet<T> _dbset;

        public GenericRepository(BaseContext context)
        {
            _entities = context;
            _dbset = context.Set<T>();
        }
        public virtual IEnumerable<T> GetAll()
        {
            IQueryable<T> query = _dbset;
            return _dbset;
            //return _dbset.AsEnumerable();
        }

        public T GetById(Guid id)
        {
            var query = this.GetAll().FirstOrDefault(x => x.Id == id);
            return query;
            //return _dbset.Find(id);
        }

        public IEnumerable<T> FindBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return _dbset.Where(predicate).AsEnumerable();
        }

        public virtual void Add(T entity)
        {
            _dbset.Add(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public virtual void Edit(T entity)
        {
            _entities.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Save()
        {
            _entities.SaveChanges();
        }
    }

    (for ALL)
    public class GenericRepository<T> : IGenericRepository<T>
          where T : class
    {
        private readonly BaseDbContext _entities;
        private readonly IDbSet<T> _dbset;

        public GenericRepository(BaseDbContext context)
        {
            _entities = context;
            _dbset = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbset.AsEnumerable();
        }

        public T GetById(Guid id)
        {
            return _dbset.Find(id);
        }

        public IEnumerable<T> FindBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            IEnumerable<T> query = _dbset.Where(predicate).AsEnumerable();
            return query;
        }

        public virtual T Add(T entity)
        {
            return _dbset.Add(entity);
        }

        public virtual T Delete(T entity)
        {
            return _dbset.Remove(entity);
        }

        public virtual void Edit(T entity)
        {
            _entities.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Save()
        {
            _entities.SaveChanges();
        }
    }

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Inoxoft.SecondaryScreen.DAL.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);
        T Add(T entity);
        T Delete(T entity);
        void Edit(T entity);
        void Save();
    }
}
