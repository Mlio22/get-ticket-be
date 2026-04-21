FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Auth.csproj", "."]
RUN dotnet restore "./Auth.csproj"
COPY . .
RUN dotnet build "Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.dll"]
