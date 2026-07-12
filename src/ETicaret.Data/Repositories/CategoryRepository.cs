using ETicaret.Data.Context;
using ETicaret.Data.Entities;

namespace ETicaret.Data.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }
}
