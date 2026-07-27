namespace Keystone.Core;

public interface IQueryableById<T, TId>
{
    Option<T> GetById(TId id);
    bool HasWithId(TId id);
}

public interface IQueryableById<T, TId1, TId2>
{
    Option<T> GetById(TId1 id1, TId2 id2);
    bool HasWithId(TId1 id1, TId2 id2);
}
