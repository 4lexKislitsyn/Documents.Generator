FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17 AS base

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS restore
WORKDIR /src
COPY "src/Documents.Generator/" .
RUN dotnet restore "Documents.Generator.csproj"

FROM restore as build
RUN dotnet build --no-restore "Documents.Generator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore "Documents.Generator.csproj" -c Release -o /app/publish

FROM base as fonts
WORKDIR /fonts
RUN apk --no-cache add curl
# Google fonts
RUN curl -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/"  | \
    grep "a href" | sed 's,.*">,,;s,/.*,,' | grep -v "\.\." | \
    grep "east" | \
    while read d; \
    do curl -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/$d/" | \
    grep "a href" | grep "\.ttf" | sed 's,.*ttf.>,,' | sed 's,</..*,,' | \
    while read f; do curl -g -s "https://sid.ethz.ch/debian/google-fonts/fonts-master/ofl/$d/$f" -o "$f"; done; done;

FROM base as final
WORKDIR /app
RUN apk --no-cache add fontconfig
RUN apk --no-cache add chromium
ENV PUPPETEER_EXECUTABLE_PATH /usr/lib/chromium/chrome
COPY --from=publish /app/publish .
RUN adduser -D nonroot
USER nonroot
EXPOSE 5001
EXPOSE 5002
ENTRYPOINT fc-cache -f && dotnet "Documents.Generator.dll"

FROM base AS standalone
WORKDIR /usr/share/fonts/google-fonts
COPY --from=fonts /fonts .
RUN find . -name "*.ttf" -exec chmod 644 {} \;
WORKDIR /app
RUN apk --no-cache add fontconfig
RUN apk --no-cache add chromium
ENV PUPPETEER_EXECUTABLE_PATH /usr/lib/chromium/chrome
COPY --from=publish /app/publish .
RUN adduser -D nonroot
USER nonroot
EXPOSE 5001
EXPOSE 5002
ENTRYPOINT fc-cache -f && dotnet "Documents.Generator.dll"
