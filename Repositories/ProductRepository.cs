using Microsoft.EntityFrameworkCore;
using PhoneShopServer.Data;
using PhoneShopShare.Contracts;
using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Repositories;

public class ProductRepository : IProduct
{
    private readonly AppDbContext _appDbContext;

    public ProductRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<ServiceResponse> AddProduct(Product model)
    {
        if (model is null)
            return new ServiceResponse(false, "Model is null");

        var (flag, message) = await CheckName(model.Name);
        if (flag)
        {
            _appDbContext.Products.Add(model);
            await Commit();
            return new ServiceResponse(true, "Product Saved");
        }

        return new ServiceResponse(flag, message);
    }

    public async Task<List<Product>> GetAllProducts(bool featureProducts)
    {
        if (featureProducts)
            return await _appDbContext.Products.Where(_ => _.Featured).ToListAsync();
        else
            return await _appDbContext.Products.ToListAsync();
    }

    private async Task<(bool, string)> CheckName(string name)
    {
        var product = await _appDbContext.Products.FirstOrDefaultAsync(
            x => x.Name.ToLower().Equals(name.ToLower()));

        return product is null
            ? (true, null)
            : (false, "Product already exist");
    }

    private async Task Commit() => await _appDbContext.SaveChangesAsync();
}

