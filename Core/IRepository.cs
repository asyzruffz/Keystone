using System.Linq.Expressions;

namespace Keystone.Core;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();
    void LoadSingle<TResult>(T obj, Expression<Func<T, TResult?>> expression) where TResult : class;
    void LoadCollection<TResult>(T obj, Expression<Func<T, IEnumerable<TResult>>> expression) where TResult : class;
    void Create(T obj);
    void Create(IEnumerable<T> objs);
    void Update(T obj);
    void Delete(T obj);
    void Attach(T obj);
    void Attach(IEnumerable<T> objs);
}
