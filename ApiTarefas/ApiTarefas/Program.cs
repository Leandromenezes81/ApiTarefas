using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Endpoints
//app.MapGet("/", () => "Olá Mundo!");

//app.MapGet("frases", async () => await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

// Get all tasks
app.MapGet("tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

// Get tasks by id
app.MapGet("tarefas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

// Get tasks completed
app.MapGet("tarefas/concluida", async (AppDbContext db) =>
                                await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

// Create new task
app.MapPost("tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa); // Results: acessa retornos HTTP.
});

// Edit task
app.MapPut("tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    // Get tarefa by id
    var tarefa = await db.Tarefas.FindAsync(id);

    // Validate if the task exists
    if (tarefa is null) return Results.NotFound();

    // Input data
    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    // Save changes
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete task
app.MapDelete("tarefas/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});
#endregion

app.Run();

#region Model
class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}
#endregion

#region DbContextClass
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}
#endregion