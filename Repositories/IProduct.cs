using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Repositories;

public interface IProduct
{
    Task<ServiceResponse> AddProduct(Product model);

    Task<List<Product>> GetAllProducts(bool featureProducts);
}

