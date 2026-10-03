FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Tacdent.slnx ./
COPY src/Tacdent.Core/Tacdent.Core.csproj src/Tacdent.Core/
COPY src/Tacdent.Data/Tacdent.Data.csproj src/Tacdent.Data/
COPY src/Tacdent.Application/Tacdent.Application.csproj src/Tacdent.Application/
COPY src/Tacdent.Api/Tacdent.Api.csproj src/Tacdent.Api/
RUN dotnet restore src/Tacdent.Api/Tacdent.Api.csproj
COPY src/ src/
RUN dotnet publish src/Tacdent.Api/Tacdent.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG GIT_SHA=unknown
LABEL org.opencontainers.image.revision=$GIT_SHA
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Tacdent.Api.dll"]
