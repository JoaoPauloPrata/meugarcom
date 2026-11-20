# Build context: apps/api
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/MeuGarcom.Domain/MeuGarcom.Domain.csproj src/MeuGarcom.Domain/
COPY src/MeuGarcom.Application/MeuGarcom.Application.csproj src/MeuGarcom.Application/
COPY src/MeuGarcom.Infrastructure/MeuGarcom.Infrastructure.csproj src/MeuGarcom.Infrastructure/
COPY src/MeuGarcom.Api/MeuGarcom.Api.csproj src/MeuGarcom.Api/
RUN dotnet restore src/MeuGarcom.Api/MeuGarcom.Api.csproj

COPY src/ src/
RUN dotnet publish src/MeuGarcom.Api/MeuGarcom.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "MeuGarcom.Api.dll"]
