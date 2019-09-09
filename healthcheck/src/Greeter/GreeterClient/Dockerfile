FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview9-bionic AS base
EXPOSE 50051
WORKDIR /app
RUN GRPC_HEALTH_PROBE_VERSION=v0.3.0 && \
    curl -L -o /bin/grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-amd64 && \
    chmod +x /bin/grpc_health_probe

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview9-buster AS build
WORKDIR /src
COPY ["GreeterClient/GreeterClient.csproj", "GreeterClient/"]
COPY ["Greeter/Greeter.csproj", "Greeter/"]
RUN dotnet restore "GreeterClient/GreeterClient.csproj"
COPY . .
WORKDIR "/src/GreeterClient"
RUN dotnet build "GreeterClient.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "GreeterClient.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GreeterClient.dll"]