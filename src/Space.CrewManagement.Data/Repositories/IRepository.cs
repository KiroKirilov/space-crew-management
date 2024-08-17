namespace Space.CrewManagement.Data.Repositories;
public interface IRepository<T>
{
    IQueryable<T> All();
    IQueryable<T> AllAsNoTracking();
    ValueTask<T?> FindAsync(params object[] id);
    Task<int> SaveAsync();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
