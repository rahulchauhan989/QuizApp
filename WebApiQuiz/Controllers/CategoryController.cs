
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
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching categories.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Category ID.");
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return category != null ? Ok(category) : NotFound("Category not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching category.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _categoryService.validateCategoryAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var createdCategory = await _categoryService.CreateCategoryAsync(dto);
            return Ok(createdCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var validationResult = await _categoryService.validateCategoryUpdateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto);
            return updatedCategory != null ? Ok(updatedCategory) : NotFound("Category not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating category.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            return deleted ? Ok("Category deleted successfully.") : NotFound("Category not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting category.");
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}