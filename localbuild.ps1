dotnet restore Preflight.sln --locked-mode
dotnet build Preflight.sln --configuration Release --no-restore -p:ContinuousIntegrationBuild=true
dotnet pack Preflight.sln --configuration Release --no-build --output /nupkg
