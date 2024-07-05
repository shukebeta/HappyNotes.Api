DROP TABLE IF EXISTS HappyNotes.Tag;
DROP TABLE IF EXISTS HappyNotes.NoteTag;
CREATE TABLE HappyNotes.NoteTag
(
    Id       BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    NoteId   BIGINT UNSIGNED NOT NULL,
    TagName  VARCHAR(128)    NOT NULL,
    CreateAt BIGINT          NOT NULL,
    INDEX idx_TagName(TagName),
    UNIQUE (NoteId, TagName) -- Ensure a note cannot have the same tag more than once
);
