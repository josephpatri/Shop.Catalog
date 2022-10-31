FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.sln .
COPY Shop.Catalog.Service/*.csproj ./Shop.Catalog.Service/
COPY Shop.Catalog.UnitTest/*.csproj ./Shop.Catalog.UnitTest/
COPY ./NuGet.Config /nuget.config

RUN dotnet restore
# copy full solution over
COPY . .
RUN dotnet build "./Shop.Catalog.Service/Shop.Catalog.Service.csproj"
RUN dotnet build "./Shop.Catalog.UnitTest/Shop.Catalog.UnitTest.csproj"

FROM build AS testrunner
WORKDIR /app/Shop.Catalog.UnitTest
CMD ["dotnet", "test", "--logger:trx"]

# run the unit tests
FROM build AS test
WORKDIR /app/Shop.Catalog.UnitTest
RUN dotnet test --logger:trx

# publish the API
FROM build AS publish
WORKDIR /app/Shop.Catalog.Service/
RUN dotnet publish -c Release -o out

# run the api
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app

COPY --from=publish /app/Shop.Catalog.Service/out ./
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Shop.Catalog.Service.dll"]