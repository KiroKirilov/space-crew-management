using Microsoft.EntityFrameworkCore;

namespace Space.CrewManagement.Data.Repositories;
public class EfRepository<T>(CrewDbContext _dbContext) : IRepository<T>
    where T : class
{
    public void Add(T entity) => _dbContext.Add(entity);
    public void Update(T entity) => _dbContext.Update(entity);
    public void Delete(T entity) => _dbContext.Remove(entity);
    public IQueryable<T> All() => _dbContext.Set<T>();
    public IQueryable<T> AllAsNoTracking() => _dbContext.Set<T>().AsNoTracking();
    public async ValueTask<T?> FindAsync(params object[] id) => await _dbContext.Set<T>().FindAsync(id);
    public async Task<int> SaveAsync() => await _dbContext.SaveChangesAsync();
}
