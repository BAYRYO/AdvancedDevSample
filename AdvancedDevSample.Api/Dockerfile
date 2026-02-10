FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Build.targets ./
COPY AdvancedDevSample.Api/AdvancedDevSample.Api.csproj AdvancedDevSample.Api/
COPY AdvancedDevSample.Application/AdvancedDevSample.Application.csproj AdvancedDevSample.Application/
COPY AdvancedDevSample.Infrastructure/AdvancedDevSample.Infrastructure.csproj AdvancedDevSample.Infrastructure/
COPY AdvancedDevSampleDomain/AdvancedDevSample.Domain.csproj AdvancedDevSampleDomain/
RUN dotnet restore AdvancedDevSample.Api/AdvancedDevSample.Api.csproj

COPY AdvancedDevSample.Api/ AdvancedDevSample.Api/
COPY AdvancedDevSample.Application/ AdvancedDevSample.Application/
COPY AdvancedDevSample.Infrastructure/ AdvancedDevSample.Infrastructure/
COPY AdvancedDevSampleDomain/ AdvancedDevSampleDomain/
RUN dotnet restore AdvancedDevSample.Api/AdvancedDevSample.Api.csproj --force-evaluate
RUN dotnet publish AdvancedDevSample.Api/AdvancedDevSample.Api.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER 65532:65532

ENTRYPOINT ["dotnet", "AdvancedDevSample.Api.dll"]
