FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Build.targets ./
COPY AdvancedDevSample.Frontend/AdvancedDevSample.Frontend.csproj AdvancedDevSample.Frontend/
RUN dotnet restore AdvancedDevSample.Frontend/AdvancedDevSample.Frontend.csproj

ARG API_BASE_URL=http://localhost:5069
COPY AdvancedDevSample.Frontend/ AdvancedDevSample.Frontend/
RUN sed -i "s#\"ApiBaseUrl\": \".*\"#\"ApiBaseUrl\": \"${API_BASE_URL}\"#g" \
    AdvancedDevSample.Frontend/wwwroot/appsettings.json
RUN dotnet publish AdvancedDevSample.Frontend/AdvancedDevSample.Frontend.csproj -c Release -o /app/publish

FROM nginx:stable-alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY AdvancedDevSample.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot ./

RUN chown -R nginx:nginx /usr/share/nginx/html /var/cache/nginx /var/log/nginx /etc/nginx/conf.d

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
