#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MetaExchangeService/MetaExchangeService.csproj", "MetaExchangeService/"]
COPY ["MetaExchange/MetaExchange.csproj", "MetaExchange/"]
RUN dotnet restore "MetaExchangeService/MetaExchangeService.csproj"
COPY . .
WORKDIR "/src/MetaExchangeService"
RUN dotnet build "MetaExchangeService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MetaExchangeService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetaExchangeService.dll"]