FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY "src/Documents.Generator/" "Documents.Generator/"
WORKDIR "/src/Documents.Generator"
RUN dotnet restore "Documents.Generator.csproj"
RUN dotnet build "Documents.Generator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Documents.Generator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17 AS final
RUN apk add chromium
RUN adduser -D nonroot
USER nonroot
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Production
ENV PUPPETEER_EXECUTABLE_PATH /usr/lib/chromium/chrome
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Documents.Generator.dll"]
