FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY *.sln .
COPY BooksCRUD.Web/*.csproj ./BooksCRUD.Web/
COPY BooksCRUD.Data/*.csproj ./BooksCRUD.Data/
COPY BooksCRUD.Image/*.csproj ./BooksCRUD.Image/
COPY BooksCRUD.Function/*.csproj ./BooksCRUD.Function/
RUN dotnet restore
COPY . .
WORKDIR /app/BooksCRUD.Web
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/BooksCRUD.Web/out ./
ENTRYPOINT ["dotnet", "BooksCRUD.Web.dll"]
EXPOSE 8080