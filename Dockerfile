FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17 AS base
WORKDIR /fonts
RUN apk --no-cache add curl
# Google fonts
RUN curl -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/"  | \
    grep "a href" | sed 's,.*">,,;s,/.*,,' | grep -v "\.\." | \
    while read d; \
    do curl -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/$d/" | \
    grep "a href" | grep "\.ttf" | sed 's,.*ttf.>,,' | sed 's,</..*,,' | \
    while read f; do curl -g -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/$d/$f" -o "$f"; done; done;

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS restore
WORKDIR /src
COPY "src/Documents.Generator/" "Documents.Generator/"
WORKDIR "/src/Documents.Generator"
RUN dotnet restore "Documents.Generator.csproj"

FROM restore as build
RUN dotnet build "Documents.Generator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Documents.Generator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /usr/share/fonts/google-fonts
COPY --from=base /fonts .
RUN find . -name "*.ttf" -exec chmod 644 {} \;
RUN apk --no-cache add fontconfig
RUN fc-cache -f && rm -rf /var/cache/*
WORKDIR /app
RUN apk --no-cache add chromium
ENV PUPPETEER_EXECUTABLE_PATH /usr/lib/chromium/chrome
RUN adduser -D nonroot
USER nonroot
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Documents.Generator.dll"]
