using quiz.Domain.Dto;

namespace Quiz.Services.Interface;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto);
    Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> CheckDuplicateCategoryAsync(string name);
    Task<ValidationResult> validateCategoryAsync(CategoryCreateDto dto);
    Task<ValidationResult> validateCategoryUpdateAsync(CategoryUpdateDto dto);
    Task<ValidationResult> validateCategoryDeleteAsync(int id);
}