using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Domain.DataModels;
using quiz.Repo.Interface;

namespace quiz.Repo.Implementation;

public class CategoryRepository : ICategoryRepository
{
    private readonly QuiZappDbContext _context;

    public CategoryRepository(QuiZappDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
        .Where(c => c.Isdeleted == false).ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;


        category.Isdeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckDuplicateCategoryAsync(string name)
    {
        return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Isdeleted == false);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByCategoryIdAsync(int categoryId)
    {
        return await _context.Quizzes
            .Where(q => q.Categoryid == categoryId && q.Isdeleted != true)
            .ToListAsync();
    }


}