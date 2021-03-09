FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CheaperResearch/CheaperResearch.csproj", "CheaperResearch/"]
RUN dotnet restore "CheaperResearch/CheaperResearch.csproj"
COPY . .
WORKDIR "/src/CheaperResearch"
RUN dotnet build "CheaperResearch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CheaperResearch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CheaperResearch.dll"]
