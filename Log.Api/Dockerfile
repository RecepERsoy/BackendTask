FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Log.Api/Log.Api.csproj", "Log.Api/"]

RUN dotnet restore "Log.Api/Log.Api.csproj"

COPY . .
WORKDIR "/src/Log.Api"
RUN dotnet publish "Log.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Log.Api.dll"]