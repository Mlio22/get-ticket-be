FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EventManagement.csproj", "."]
RUN dotnet restore "./EventManagement.csproj"
COPY . .
RUN dotnet build "EventManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventManagement.dll"]
