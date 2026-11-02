# Etapa 1: compila o site
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GarimpoChic/GarimpoChic.csproj GarimpoChic/
RUN dotnet restore GarimpoChic/GarimpoChic.csproj

COPY GarimpoChic/ GarimpoChic/
RUN dotnet publish GarimpoChic/GarimpoChic.csproj -c Release -o /app --no-restore

# Etapa 2: imagem final, só com o necessário para rodar
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GarimpoChic.dll"]
