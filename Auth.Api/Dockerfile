FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Auth.Api/Auth.Api.csproj", "Auth.Api/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]

RUN dotnet restore "Auth.Api/Auth.Api.csproj"

COPY . .
WORKDIR "/src/Auth.Api"
RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]