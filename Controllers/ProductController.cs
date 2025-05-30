using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopShare.Contracts;
using PhoneShopShare.Models;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProduct productService) : ControllerBase
{
        [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAllProduct(bool featured)
    {
        var products = await productService.GetAllProducts(featured);
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> AddProduct(Product model)
    {
        if(model is null)
            return BadRequest("Model is null");
        var reponse = await productService.AddProduct(model);
        return Ok(reponse);
    }
}

