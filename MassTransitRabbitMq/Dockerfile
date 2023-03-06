FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MassTransitRabbitMq/MassTransitRabbitMq.csproj", "MassTransitRabbitMq/"]
RUN dotnet restore "MassTransitRabbitMq/MassTransitRabbitMq.csproj"
COPY . .
WORKDIR "/src/MassTransitRabbitMq"
RUN dotnet build "MassTransitRabbitMq.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MassTransitRabbitMq.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MassTransitRabbitMq.dll"]
