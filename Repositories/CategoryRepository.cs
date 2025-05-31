using Microsoft.EntityFrameworkCore;
using PhoneShopServer.Data;
using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Repositories;

public class CategoryRepository : ICategory
{
    private readonly AppDbContext _appDbContext;

    public CategoryRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<ServiceResponse> AddCategory(Category model)
    {
        if (model is null)
            return new ServiceResponse(false, "Model is null");

        var (flag, message) = await CheckName(model.Name);
        if (flag)
        {
            _appDbContext.Category.Add(model);
            await Commit();
            return new ServiceResponse(true, "Product Saved");
        }

        return new ServiceResponse(flag, message);
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _appDbContext.Category.ToListAsync();
    }

    private async Task<(bool, string)> CheckName(string name)
    {
        var category = await _appDbContext.Category.FirstOrDefaultAsync(
            x => x.Name.ToLower().Equals(name.ToLower()));

        return category is null
            ? (true, null)
            : (false, "Category already exist");
    }

    private async Task Commit() => await _appDbContext.SaveChangesAsync();
}

