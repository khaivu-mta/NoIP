FROM selenium/standalone-chrome:4.2.1 AS base
WORKDIR /app

# build and copy dotnet project
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS publish
WORKDIR /src
COPY ["noip.csproj", "noip/"]
COPY . .
RUN dotnet publish "noip.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# install dotnet
RUN sudo wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN sudo dpkg -i packages-microsoft-prod.deb
RUN sudo apt-get update
RUN sudo apt-get install -y dotnet-runtime-5.0

ENTRYPOINT ["dotnet", "noip.dll"]