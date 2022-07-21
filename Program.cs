WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();


WebApplication? app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run("i hate this");