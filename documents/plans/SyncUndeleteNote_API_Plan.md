# Plan for Adding SyncUndeleteNote API to ISyncNoteService

**Goal**: Add a `SyncUndeleteNote` method to the `ISyncNoteService` interface and implement it in relevant services to handle undeleting notes, specifically updating the `deletedAt` field to 0 in ManticoreSearch, while taking no action for Mastodon and Telegram sync services.

## Steps:

1. **Update the `ISyncNoteService` Interface**:
   - Add a new method `SyncUndeleteNote` to the interface with only the `Note` entity as a parameter.
   - Location: `src/HappyNotes.Services/interfaces/ISyncNoteService.cs`

2. **Implement `SyncUndeleteNote` in `ManticoreSyncNoteService`**:
   - Add the method to update the `deletedAt` field to 0 in the Manticore search index using the existing `searchService`.
   - Handle any exceptions with appropriate logging, similar to other methods in this class.
   - Location: `src/HappyNotes.Services/ManticoreSyncNoteService.cs`

3. **Implement `SyncUndeleteNote` in Other Sync Services**:
   - For `MastodonSyncNoteService` and `TelegramSyncNoteService`, implement the method to do nothing (empty method or simple return), as per the user's instruction.
   - Locations: `src/HappyNotes.Services/MastodonSyncNoteService.cs` and `src/HappyNotes.Services/TelegramSyncNoteService.cs`

4. **Update Any Calling Code if Necessary**:
   - Check if there are any places in the application where notes are undeleted, and ensure that the `SyncUndeleteNote` method is called on the appropriate sync services after an undelete operation.
   - Likely locations: `src/HappyNotes.Services/NoteService.cs` or controllers like `src/HappyNotes.Api/Controllers/NoteController.cs`

## Mermaid Diagram for Process Flow

```mermaid
sequenceDiagram
    participant U as User
    participant N as NoteController
    participant NS as NoteService
    participant SS as SyncServices (Manticore, Mastodon, Telegram)
    U->>N: Request to Undelete Note
    N->>NS: Call UndeleteNote
    NS->>NS: Update deletedAt to NULL in MySQL
    NS->>SS: Call SyncUndeleteNote on All Sync Services
    SS-->>NS: Manticore Updates deletedAt to 0, Others Do Nothing
    NS-->>N: Return Success
    N-->>U: Confirm Undelete Successful