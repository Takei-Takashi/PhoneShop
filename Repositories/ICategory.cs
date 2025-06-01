using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Repositories;

public interface ICategory
{
    Task<ServiceResponse> AddCategory(Category model);

    Task<List<Category>> GetAllCategories();
}
