
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("QuiZAppDb")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<QuizSubmissionScheduler>();   
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
builder.Services.AddScoped<IUserQuizAttemptRepository, UserQuizAttemptRepository>();
builder.Services.AddScoped<ILoginRepo, LoginRepo>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IQuestionServices, QuestionServices>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuizesSubmission,QuizesSubmission>();
builder.Services.AddScoped<IUserHistory,UserHistory>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) 
    .AddJwtBearer(options =>  
    {
        options.TokenValidationParameters = new TokenValidationParameters  
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,  
            ValidIssuer = builder.Configuration["Jwt:Issuer"], 
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<QuizSubmissionService>();

builder.Services.AddSwaggerGen(options => 
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",  
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,  
        Scheme = "Bearer",  
        BearerFormat = "JWT", 
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,  
        Description = "Enter  your valid JWT token in the text input below"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}  
        }
    });   
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();