## clean up from previous runs
rm -r -force nupkgs
rm -r -force ./src/Preflight.Backoffice/Preflight.Backoffice/App_Plugins
mkdir nupkgs

## install backoffice dependencies
cd ./src/Preflight.Backoffice
npm install
npm run prod
cd ../../

## pack individually to avoid plumber.site blowing up
dotnet pack ./src/Preflight.Backoffice/Preflight.Backoffice.csproj -c Release -o nupkgs
dotnet pack ./src/Preflight/Preflight.csproj -c Release -o nupkgs
