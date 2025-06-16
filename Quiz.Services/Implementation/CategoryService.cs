using Microsoft.AspNetCore.Http;
using quiz.Domain.DataModels;
using quiz.Domain.ViewModels;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILoginService _loginService;

    public CategoryService(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor, ILoginService loginService)
    {
        _httpContextAccessor = httpContextAccessor;
        _categoryRepository = categoryRepository;
        _loginService = loginService;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.Createdat
        });
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.Createdat
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto)
    {
        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Createdby = userId,
        };

        await _categoryRepository.CreateCategoryAsync(category);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.Createdat,
            CreatedBy = (int)category.Createdby!
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
            return null;

        string token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "")!;
        int userId = _loginService.ExtractUserIdFromToken(token);

        category.Name = dto.Name ?? category.Name;
        category.Description = dto.Description ?? category.Description;
        category.Modifiedby = userId;
        category.Modifiedat = DateTime.UtcNow.ToLocalTime(); //

        await _categoryRepository.UpdateCategoryAsync(category);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.Createdat,
            UpdatedAt = category.Modifiedat,
            CreatedBy = (int)category.Createdby!,
            UpdatedBy = (int)category.Modifiedby!
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _categoryRepository.DeleteCategoryAsync(id);
    }

    public async Task<bool> CheckDuplicateCategoryAsync(string name)
    {
        // var categories = await _categoryRepository.GetAllCategoriesAsync();
        // return categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        bool isCategoryExists = await _categoryRepository.CheckDuplicateCategoryAsync(name);
        return isCategoryExists;
    }

}