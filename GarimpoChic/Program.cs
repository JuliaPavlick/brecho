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

// Chave que protege os CPFs no banco; sem ela o site não sobe (veja Util/Protecao.cs)
Protecao.Configurar(builder.Configuration["CHAVE_DADOS"]);

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
        o.Events.OnValidatePrincipal = Sessao.ValidarAsync;
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(Limites.Configurar);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(365));

builder.Services.AddSingleton<Email>();
builder.Services.AddHttpClient<Gemini>(c => c.Timeout = TimeSpan.FromSeconds(60));
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
{
    app.UseExceptionHandler("/erro");
    // Diz ao navegador para usar sempre HTTPS neste endereço
    app.UseHsts();
}

app.UseCabecalhosDeSeguranca();

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
