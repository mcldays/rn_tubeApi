FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["./rn_tubeApi.csproj", "."]
RUN dotnet restore "rn_tubeApi.csproj" --use-current-runtime --runtime "linux-x64"
COPY [".", "."]
#WORKDIR "/src/rn_tubeApi"
RUN dotnet build "rn_tubeApi.csproj" -c Release -o /app/build --runtime "linux-x64" --framework "net7.0"

FROM build AS publish
RUN dotnet publish "rn_tubeApi.csproj" -c Release -o /app/publish --runtime "linux-x64" --use-current-runtime --self-contained false --no-restore

FROM base AS final
WORKDIR /app
RUN mkdir videos
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:80
ENTRYPOINT ["dotnet", "rn_tubeApi.dll"]