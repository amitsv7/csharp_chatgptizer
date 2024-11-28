using Microsoft.AspNetCore.Mvc;
using OpenAI_API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapPost("/api/Data/GetData", async ([FromServices] IConfiguration configuration, [FromBody] string query) =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query cannot be null or empty.");
            }

            var openAIAPI = new OpenAIAPI(configuration["ChatGPT:OpenAISecretKey"]);

            var completions = await openAIAPI.Completions.CreateCompletionAsync(
                prompt: configuration["ChatGPT:Prompt"],
                model: configuration["ChatGPT:Model"],
                max_tokens: int.Parse(configuration["ChatGPT:MaxTokens"]),
                temperature: double.Parse(configuration["ChatGPT:Temperature"])
            );

            var completionsResult = completions.Completions;

            if (completionsResult is not null && completionsResult.Count > 0)
            {
                return Results.Ok(completionsResult[0].Text.Trim());
            }

            return Results.BadRequest("No response from OpenAI.");
        }
        catch(Exception ex)
        {
            return Results.BadRequest("Unable to connect with OpenAI.");
        }
    })
    .WithName("GetData")
    .WithOpenApi();

app.Run();