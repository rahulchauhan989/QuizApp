
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using Quiz.Services.Interface;

namespace WebApiQuiz.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    public async Task<ActionResult<ResponseDto>> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return categories != null && categories.Any() 
                ? Ok(new ResponseDto(true, "Categories fetched successfully.", categories))
                : new ResponseDto(false, "No categories found.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching categories.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<ActionResult<ResponseDto>> GetCategoryById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Category ID.");
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return category != null 
                ? Ok(new ResponseDto(true, "Category fetched successfully.", category)) 
                : new ResponseDto(false, "Category not found.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching category.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> CreateCategory([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _categoryService.validateCategoryAsync(dto);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var createdCategory = await _categoryService.CreateCategoryAsync(dto);
            return createdCategory != null 
                ? new ResponseDto(true, "Category created successfully.", createdCategory)
                : new ResponseDto(false, "Failed to create category.", null, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _categoryService.validateCategoryUpdateAsync(dto);
            if (!validationResult.IsValid)
                return new ResponseDto(false, validationResult.ErrorMessage, null, 400);

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto);
            return updatedCategory != null 
                ? new ResponseDto(true, "Category updated successfully.", updatedCategory) 
                : new ResponseDto(false, "Failed to update category or category not found.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating category.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseDto>> DeleteCategory(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            return deleted 
                ? new ResponseDto(true, "Category deleted successfully.", null)
                : new ResponseDto(false, "Failed to delete category or category not found.", null, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting category.");
            return new ResponseDto(false, "An internal server error occurred.", null, 500);
        }
    }
}