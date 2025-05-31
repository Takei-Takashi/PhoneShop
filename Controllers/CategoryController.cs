using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopServer.Repositories;
using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategory categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAllCategories()
    {
        var categories = await categoryService.GetAllCategories();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> AddCategory(Category model)
    {
        if(model is null)
            return BadRequest("Model is null");
        var reponse = await categoryService.AddCategory(model);
        return Ok(reponse);
    }
}

