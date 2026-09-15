# ============================
# Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/EnglishLearning.Domain/EnglishLearning.Domain.csproj", \
      "src/EnglishLearning.Domain/"]

COPY ["src/EnglishLearning.Application/EnglishLearning.Application.csproj", \
      "src/EnglishLearning.Application/"]

COPY ["src/EnglishLearning.Infrastructure/EnglishLearning.Infrastructure.csproj", \
      "src/EnglishLearning.Infrastructure/"]

COPY ["src/EnglishLearning.Web/EnglishLearning.Web.csproj", \
      "src/EnglishLearning.Web/"]

RUN dotnet restore \
    "src/EnglishLearning.Web/EnglishLearning.Web.csproj"

COPY . .

WORKDIR "/src/src/EnglishLearning.Web"

RUN dotnet publish \
    "EnglishLearning.Web.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

# ============================
# Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "EnglishLearning.Web.dll"]