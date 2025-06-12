
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("RMSDbConnection")));

// AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
builder.Services.AddScoped<IUserQuizAttemptRepository, UserQuizAttemptRepository>();
builder.Services.AddScoped<ILoginRepo, LoginRepo>();
builder.Services.AddScoped<ILoginService, LoginService>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  //Adds authentication services to the application.
    .AddJwtBearer(options =>  //Configures the JWT Bearer authentication scheme
    {
        options.TokenValidationParameters = new TokenValidationParameters  //TokenValidationParameters Specifies the rules for validating the JWT token
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,  //Ensures that the token's signature is valid and was signed using the correct key.
            ValidIssuer = builder.Configuration["Jwt:Issuer"],  //Specifies the expected iss (Issuer) claim value.
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
        };
    });

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(options => //Configures Swagger to support JWT Bearer authentication.
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",  //Specifies the name of the header where the token will be sent.
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,  //Specifies the type of security scheme
        Scheme = "Bearer",  //Specifies the scheme name.
        BearerFormat = "JWT",  //Specifies the format of the token.
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,  //Specifies where the token should be sent in the request.
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
            new string[] {}   //Specifies that no additional scopes are required.
        }
    });   //Configures Swagger to require the "Bearer" scheme for all endpoints
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();