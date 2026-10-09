# ---- Stage 1: publish the .NET API ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src

# Copy just the project files first so `dotnet restore` is cached across builds that only
# change source, not dependencies.
COPY Carpool.Core/Carpool.Core.csproj Carpool.Core/
COPY Carpool.Data/Carpool.Data.csproj Carpool.Data/
COPY Carpool.Service/Carpool.Service.csproj Carpool.Service/
COPY Carpool.API/Carpool.API.csproj Carpool.API/
RUN dotnet restore Carpool.API/Carpool.API.csproj

COPY Carpool.Core/ Carpool.Core/
COPY Carpool.Data/ Carpool.Data/
COPY Carpool.Service/ Carpool.Service/
COPY Carpool.API/ Carpool.API/
RUN dotnet publish Carpool.API/Carpool.API.csproj -c Release -o /app/publish --no-restore

# ---- Final stage: ASP.NET Core runtime, serving the API only ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=api-build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render injects PORT at runtime; ASPNETCORE_URLS has no built-in way to read it, so this
# resolves it via the shell (falling back to 8080 for `docker run` without Render's env).
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Carpool.API.dll"]