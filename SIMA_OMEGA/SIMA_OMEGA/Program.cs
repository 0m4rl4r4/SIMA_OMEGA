using SIMA_OMEGA;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using SIMA_OMEGA.Profile;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



//Configurar la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();


// Agrega servicios de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//Configurar JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // No valida el emisor (útil para desarrollo)
            ValidateAudience = false, // No valida la audiencia
            ValidateLifetime = true, // Valida si el token ha expirado
            ValidateIssuerSigningKey = true, // Valida la firma del token
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["LlaveJWT"]!)),
            ClockSkew = TimeSpan.Zero // Sin tolerancia en la expiración del token
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Permite recibir tokens directamente sin necesidad de prefijo "Bearer"
                var authorizationHeader = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    context.Token = authorizationHeader; // Extrae el token sin prefijo
                }

                return Task.CompletedTask;
            }
        };
    });


//Configurar Swagger para JWT (Bearrer)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Agregar AutoMapper para los DTOs
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//builder.Services.AddAutoMapper(typeof(MappingProfile));



builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        //c.RoutePrefix = string.Empty; // Para que Swagger sea accesible en la raíz
    });
}

app.MapPost("/api/plant/predict", async (IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("No valid image file provided");

    // Validar tipo de archivo
    var allowedTypes = new[] { "image/jpeg", "image/png" };
    if (!allowedTypes.Contains(file.ContentType))
        return Results.BadRequest("Only JPEG or PNG images are allowed");

    // Validar tamaño (ej. 5MB)
    if (file.Length > 5 * 1024 * 1024)
        return Results.BadRequest("Image too large (max 5MB)");

    try
    {
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();
        using var streamContent = new StreamContent(fileStream);

        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "image", file.FileName);

        var response = await client.PostAsync("http://127.0.0.1:3000/predict", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Results.Problem($"Flask API error: {error}",
                                  statusCode: (int)response.StatusCode);
        }

        var result = await response.Content.ReadAsStringAsync();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Internal server error: {ex.Message}",
                              statusCode: 500);
    }
})
    .DisableAntiforgery()
.WithName("PredictPlant")
.Produces<string>(StatusCodes.Status200OK, contentType: "application/json")
.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

app.UseCors("AllowAllOrigins");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();


app.UseAuthorization();

app.MapControllers();

app.Run();
