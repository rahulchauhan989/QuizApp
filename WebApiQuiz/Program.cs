using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataContext;
using quiz.Repo.Implementation;
using quiz.Repo.Interface;
using Quiz.Services.Implementation;
using Quiz.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register the QuizDbContext with a connection string
builder.Services.AddDbContext<QuiZappDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RMSDbConnection")));

    // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    builder.Services.AddScoped<IQuizRepository, QuizRepository>();
    builder.Services.AddScoped<IQuizService, QuizService>();
    builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
    builder.Services.AddScoped<IUserQuizAttemptRepository, UserQuizAttemptRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
