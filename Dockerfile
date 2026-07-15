# Restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["ShoppingCart.sln", "./"]
COPY ["src/ShoppingCart.Api/ShoppingCart.Api.csproj", "src/ShoppingCart.Api/"]
COPY ["src/ShoppingCart.Application/ShoppingCart.Application.csproj", "src/ShoppingCart.Application/"]
COPY ["src/ShoppingCart.Domain/ShoppingCart.Domain.csproj", "src/ShoppingCart.Domain/"]
COPY ["src/ShoppingCart.Infrastructure/ShoppingCart.Infrastructure.csproj", "src/ShoppingCart.Infrastructure/"]

RUN dotnet restore "src/ShoppingCart.Api/ShoppingCart.Api.csproj"

# Development image with Hot Reload
FROM restore AS development

COPY . .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

EXPOSE 8080

CMD ["dotnet", "watch", "--project", "src/ShoppingCart.Api/ShoppingCart.Api.csproj", "run", "--no-launch-profile"]

# Build and publish
FROM restore AS build

COPY . .

WORKDIR /src/src/ShoppingCart.Api

RUN dotnet publish "ShoppingCart.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ShoppingCart.Api.dll"]