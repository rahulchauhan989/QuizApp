using Microsoft.AspNetCore.Http;
using quiz.Domain.DataModels;
using quiz.Domain.Dto;
using quiz.Repo.Interface;
using Quiz.Services.Interface;

namespace Quiz.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILoginService _loginService;

    private readonly IQuizService _quizService;

    private readonly IUserQuizAttemptRepository _userQuizAttemptRepository;

    public CategoryService(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor, ILoginService loginService, IQuizService quizService, IUserQuizAttemptRepository userQuizAttemptRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _categoryRepository = categoryRepository;
        _loginService = loginService;
        _quizService = quizService;
        _userQuizAttemptRepository = userQuizAttemptRepository;
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

        bool isNameChanged = !string.IsNullOrWhiteSpace(dto.Name) && dto.Name != category.Name;
        if (isNameChanged)
        {
            bool iaNameExists = await _categoryRepository.CheckDuplicateCategoryAsync(dto.Name!);
            if (iaNameExists)
            {
                throw new InvalidOperationException("Category with this name already exists.");
            }
        }

        category.Name = dto.Name ?? category.Name;
        category.Description = dto.Description ?? category.Description;
        category.Modifiedby = userId;
        category.Modifiedat = DateTime.UtcNow.ToLocalTime(); 

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
        bool isCategoryExists = await _categoryRepository.CheckDuplicateCategoryAsync(name);
        return isCategoryExists;
    }

    public async Task<ValidationResult> validateCategoryAsync(CategoryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ValidationResult.Failure("Category name is required.");

        if (dto.Name.Length < 3 || dto.Name.Length > 50)
            return ValidationResult.Failure("Category name must be between 3 and 50 characters.");

        if (await CheckDuplicateCategoryAsync(dto.Name))
            return ValidationResult.Failure("Category with this name already exists.");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> validateCategoryUpdateAsync(CategoryUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ValidationResult.Failure("Category name is required.");

        if (dto.Name.Length < 3 || dto.Name.Length > 50)
            return ValidationResult.Failure("Category name must be between 3 and 50 characters.");            

        if (await CheckDuplicateCategoryAsync(dto.Name))
            return ValidationResult.Failure("Category with this name already exists.");

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> validateCategoryDeleteAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);  
        if (category == null)
            return ValidationResult.Failure("Category not found.");

        var quizzes = await _categoryRepository.GetQuizzesByCategoryIdAsync(id);
        if (quizzes.Any())
        {
            if (quizzes.Any(q => q.Ispublic == true))
                return ValidationResult.Failure("Cannot delete category as it is associated with published quizzes.");

            return ValidationResult.Failure("Cannot delete category as it is associated with existing quizzes.");
        }

        var userQuizAttempts = await _userQuizAttemptRepository.GetAttemptsByCategoryIdAsync(id);
        if (userQuizAttempts.Any())
            return ValidationResult.Failure("Cannot delete category as it is associated with user quiz attempts.");

        return ValidationResult.Success();
    }
}