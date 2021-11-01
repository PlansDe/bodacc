test:
	dotnet test --configuration Release

release: test
	dotnet build --configuration Release

run: release
	dotnet run --configuration Release --project=app