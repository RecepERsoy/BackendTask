FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Product.Api/Product.Api.csproj", "Product.Api/"]
COPY ["Product.Domain/Product.Domain.csproj", "Product.Domain/"]
COPY ["Product.Application/Product.Application.csproj", "Product.Application/"]
COPY ["Product.Infrastructure/Product.Infrastructure.csproj", "Product.Infrastructure/"]

RUN dotnet restore "Product.Api/Product.Api.csproj"

COPY . .
WORKDIR "/src/Product.Api"
RUN dotnet publish "Product.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Product.Api.dll"]