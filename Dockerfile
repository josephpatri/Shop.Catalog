FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.sln .
COPY Shop.Catalog.Service/*.csproj ./Shop.Catalog.Service/
COPY Shop.Catalog.UnitTest/*.csproj ./Shop.Catalog.UnitTest/

ARG PAT

# Get and install the Artifact Credential provider
RUN wget -O - https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | bash

# Set environment variables
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS '{"endpointCredentials":[{"endpoint":"https://pkgs.dev.azure.com/josephville12/_packaging/Commons/nuget/v3/index.json","username":"josephville12","password":"'${nuget_pat}'"}]}'
RUN dotnet restore . -s "https://pkgs.dev.azure.com/josephville12/_packaging/Commons/nuget/v3/index.json" -s "https://api.nuget.org/v3/index.json"

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