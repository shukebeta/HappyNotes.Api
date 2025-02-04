# Changelog

All notable changes to the HappyNote.Api project will be documented in this file.

---

## [2025-01-11]
- Added `limit` parameter to `/tag/myTagCloud` API.
- Excluded tags starting with `@` in `/tag/myTagCloud` API.
- Ensured tags remain clickable when syncing long text to Mastodon.

## [2025-01-03]
- Added `PublishDateTime` and `TimeZoneId` support to `PostNoteRequest` for scheduling notes to past/future dates.

## [2024-11-24]
- Added Markdown image support to `MastodonTootService`.

## [2024-10-30]
- Introduced Mastodon sync feature.

## [2024-08-15]

### Added
- Telegram message length limits and Markdown support (August 3).
- File sending for long Telegram messages (August 3).
- `TelegramMessageIds` field to `Note` for message tracking (August 10).
- `TagController` and `TagCount` model for tag cloud functionality (August 13).

### Changed
- Enhanced `TelegramSettingStatus` and related features (August 6).
- Improved Telegram note sending and tag handling (August 10).
- Updated `NoteTag` to include `UserId` for user-specific tag management (August 13).

### Fixed
- Refined tag regex to align with traditional tag definitions (August 15).

## [2024-08-02]

### Added
- Text encryption helper and test suite for secure data handling (August 1).
- Telegram sync feature: settings, controller, and initial service implementation (August 1-2).

### Changed
- Refactored note creation process and improved tag handling (July 28).
- Renamed timestamp fields for consistency (August 2).

## [2024-07-10]

### Added
- Timezone setup support (July 1).
- Tags functionality and API (July 3-5).
- `privateNoteOnlyIsEnabled` setting option (July 7).

### Changed
- Updated database: added indexes to `Note` table and dropped `Status` column (July 10).

### Fixed
- Resolved `IsMarkdown` functionality issue (July 6).
- Fixed missing weeks in memories feature (July 10).

## [2024-06-16]

### Added
- Memories API (June 3).
- Duplicate check mechanism for post/update API (June 12).
- Auto write-note feature in develop/staging environments (June 18).

### Changed
- Optimized memories API and fixed timezone calculations (June 10).
- Adjusted duplicate request response when posting notes (June 13).
- Upgraded SqlSugar to v5.1.4.158 (June 13).

### Fixed
- Prevented users from updating another user's note (June 23).

## [2024-05-28]

### Added
- User settings table and APIs.
- `is_markdown` field to `Note` table.

## [2024-05-19]

### Added
- Initial project setup and basic functionality (May 4-18).
- Deployment workflows for staging and production environments.
- CORS support.
- Domain support for `shukebeta.github.io`.
