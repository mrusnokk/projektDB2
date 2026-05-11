using Repositories;

namespace projektDB2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string oracleConnString = builder.Configuration.GetConnectionString("OracleDbConnection")
    ?? "User Id=TVOJE_JMENO;Password=TVOJE_HESLO;Data Source=localhost:1521/XEPDB1;";
            builder.Services.AddScoped<InventoryRepository>(provider => new InventoryRepository(oracleConnString));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Inventory}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
