using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Projet_Final.Areas.Identity.Data;
using Projet_Final.Hubs;
using Projet_Final.Models;
using Projet_Final.Services;
using Projet_Final.Settings;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUtilisateur>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//External LogIn
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));

//Google
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration.GetSection("GoogleAuthSettings")
    .GetValue<string>("ClientId");
    googleOptions.ClientSecret = builder.Configuration.GetSection("GoogleAuthSettings")
    .GetValue<string>("ClientSecret");
});

//Facebook
builder.Services.AddAuthentication().AddFacebook(opt =>
{
    opt.ClientId = "2128729867488511";
    opt.ClientSecret = "97a00ea7a960dd8643398adc88f5833b";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddLogging();
builder.Services.AddSignalR();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Dur�e de verrouillage du compte
});

builder.Services.AddSingleton<TestData>();

//SendGrid
builder.Services.AddSendGrid(options => {
    options.ApiKey = builder.Configuration.GetSection("SendGridSettings")
    .GetValue<string>("ApiKey");
});

builder.Services.AddScoped<IEmailSender, EmailSenderService>();
builder.Services.AddTransient<IEmailSender, EmailSenderService>();
builder.Services.AddScoped<ISMSSenderService, SMSSenderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    // Ajouter les rôles pour créer l'administrateur
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Récupérer le service UserManager
    var userManager = services.GetRequiredService<UserManager<ApplicationUtilisateur>>();

    // Vérifier si un administrateur existe déjà
    var utilisateurAdmin = await userManager.FindByEmailAsync("admin@gmail.com");

    // Si aucun administrateur n'existe, créez-en un
    if (utilisateurAdmin == null)
    {
        var admin = new ApplicationUtilisateur
        {
            UserName = "admin@gmail.com",
            Email = "admin@gmail.com",
            FirstName = "Admin", 
            LastName = "Test"
        };

        var creerAdmin = await userManager.CreateAsync(admin, "test1234!");

        if (creerAdmin.Succeeded)
        {
            // Ajoutez le rôle d'administrateur à l'utilisateur
            var ajouterRole = await userManager.AddToRoleAsync(admin, "Admin");

            if (!ajouterRole.Succeeded)
            {
                // Gérez l'échec d'ajout de rôle ici
                // Vous pouvez logger l'erreur ou prendre des mesures appropriées

                // Exemple de journalisation de l'erreur
                logger.LogError("Échec lors de l'ajout du rôle à l'utilisateur : {Error}", ajouterRole.Errors);
            }
        }
        else
        {
            // Gérez l'échec de création d'utilisateur ici
            // Vous pouvez logger l'erreur ou prendre des mesures appropriées

            // Exemple de journalisation de l'erreur
            logger.LogError("Échec lors de la création de l'utilisateur : {Error}", creerAdmin.Errors);
        }
    }

    ApplicationDbContext.SeedData(dbContext);
}

app.Run();
