# Plan to Refine ManticoreSearch Implementation Structure

## Objective
Refine the structure of the ManticoreSearch implementation in `SearchService.cs` by:
1. Moving search-related model classes into separate files within a `Search` subdirectory in the `HappyNotes.Models` project.
2. Configuring the ManticoreSearch HTTP endpoint in `appsettings.json` and accessing it from `SearchService.cs`.

## Steps to Implement

1. **Create Subdirectory for Search Models**
   - Create a new subdirectory named `Search` in `src/HappyNotes.Models`.
   - Create separate files for each search model class:
     - `ManticoreSearchResult.cs`
     - `ManticoreHits.cs`
     - `ManticoreHit.cs`
     - `ManticoreSource.cs`
   - Extract the respective classes from `SearchService.cs` into these files, ensuring proper namespace (`HappyNotes.Models.Search`).

2. **Update `SearchService.cs`**
   - Remove the model class definitions from `SearchService.cs`.
   - Add using directive for the new namespace `HappyNotes.Models.Search`.
   - Update the code to reference the models from the new namespace.

3. **Configure ManticoreSearch Endpoint in `appsettings.Development.json`**
   - Add a new configuration entry for the ManticoreSearch HTTP endpoint under `ManticoreConnectionOptions` or a new section if appropriate, e.g., `"HttpEndpoint": "http://127.0.0.1:9312"`.
   - Ensure this configuration can be read in different environments if needed.

4. **Modify `SearchService.cs` to Use Configuration**
   - Inject `IConfiguration` into the `SearchService` constructor to access configuration settings.
   - Update the initialization of `HttpClient` to use the endpoint from the configuration instead of hardcoding `http://127.0.0.1:9312`.

5. **Update `ServiceExtensions.cs` if Necessary**
   - Ensure that `IConfiguration` is available for injection into `SearchService` if not already set up.

6. **Testing and Validation**
   - Verify that the application still compiles and runs correctly after these structural changes.
   - Test the search functionality to ensure it continues to work with the new model structure and configured endpoint.

## Mermaid Diagram for Refined Structure

```mermaid
classDiagram
    class SearchService {
        -IDatabaseClient _client
        -HttpClient _httpClient
        +SearchNotesAsync()
        +SyncNoteToIndexAsync()
        +DeleteNoteFromIndexAsync()
        +UndeleteNoteFromIndexAsync()
        +PurgeDeletedNotesFromIndexAsync()
    }
    class ManticoreSearchResult {
        +ManticoreHits hits
    }
    class ManticoreHits {
        +long total
        +List~ManticoreHit~ hits
    }
    class ManticoreHit {
        +long _id
        +long _score
        +ManticoreSource _source
    }
    class ManticoreSource {
        +long Id
        +long userid
        +string content
        +int islong
        +int isprivate
        +int ismarkdown
        +long createdat
        +long updatedat
        +long deletedat
    }
    SearchService --> ManticoreSearchResult : Uses
    ManticoreSearchResult --> ManticoreHits : Contains
    ManticoreHits --> ManticoreHit : Contains List of
    ManticoreHit --> ManticoreSource : Contains
    note left of SearchService
        Located in HappyNotes.Services
    end
    note right of ManticoreSearchResult
        Located in HappyNotes.Models.Search
    end
```

## Considerations
- **Namespace Consistency**: Ensure that the namespace for the new model files (`HappyNotes.Models.Search`) is consistent and properly referenced in `SearchService.cs`.
- **Configuration Flexibility**: Make sure the endpoint configuration in `appsettings.json` can be overridden for different environments (e.g., Development, Staging, Production).
- **Backward Compatibility**: Verify that no other parts of the application are affected by moving the model classes.