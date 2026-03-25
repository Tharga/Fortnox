# Feature: Add Unit Tests

## Goal
Add a comprehensive unit test project to the Tharga.Fortnox solution and integrate test execution into the Azure DevOps build pipeline.

## Originating Branch
`develop`

## Scope
- Create `Tharga.Fortnox.Tests` xUnit test project
- Add `InternalsVisibleTo` to main project so internal classes can be tested
- Add the test project to the solution
- Write tests for: Result, FortnoxScope, FortnoxConnectionService, DI registration
- Add `dotnet test` step to azure-pipelines.yml

## Acceptance Criteria
- [ ] Test project builds and all tests pass
- [ ] Result and Result<T> behavior is tested
- [ ] FortnoxScope flag values are validated
- [ ] FortnoxConnectionService.BuildConnectUriAsync is tested (validation + URI output)
- [ ] FortnoxConnectionService HTTP methods are tested with mocked HttpClient
- [ ] DI registration is tested
- [ ] Pipeline runs tests as part of the build

## Done Condition
All tests pass, pipeline includes test step, user confirms feature is complete.
