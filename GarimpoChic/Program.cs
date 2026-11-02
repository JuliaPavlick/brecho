using GarimpoChic.Data;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// A Render informa em qual porta o site deve escutar
var porta = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(porta))
    builder.WebHost.UseUrls($"http://0.0.0.0:{porta}");

builder.Services.AddDbContext<BrechoContext>(o => Banco.Configurar(o, builder.Configuration, builder.Environment));

builder.Services.AddDataProtection()
    .SetApplicationName("GarimpoChic")
    .PersistKeysToDbContext<BrechoContext>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/";
        o.AccessDeniedPath = "/perfis";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.Cookie.Name = "garimpo.login";
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<Email>();
builder.Services.AddSingleton<RedefinicaoSenha>();

// As telas que usam fetch() mandam o token neste cabeçalho
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Mantém acentos e emojis legíveis no HTML gerado (sem virar "&#xE9;")
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// A Render recebe o HTTPS e repassa como HTTP; isto faz o site saber que a conexão original era segura
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BrechoContext>();
    await Atualizacao.PrepararBancoAsync(db);
    await DadosIniciais.PopularAsync(db, app.Configuration);
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/erro");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
