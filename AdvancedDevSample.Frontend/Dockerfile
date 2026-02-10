FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore AdvancedDevSample.slnx

ARG API_BASE_URL=http://localhost:5069
RUN sed -i "s#\"ApiBaseUrl\": \".*\"#\"ApiBaseUrl\": \"${API_BASE_URL}\"#g" AdvancedDevSample.Frontend/wwwroot/appsettings.json
RUN dotnet publish AdvancedDevSample.Frontend/AdvancedDevSample.Frontend.csproj -c Release -o /app/publish

FROM nginx:1.27-alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY AdvancedDevSample.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot ./

EXPOSE 80
