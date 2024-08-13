-- Check if the column 'UserId' exists in the 'HappyNotes.Note' table
SET @column_exists = (SELECT COUNT(*)
                      FROM information_schema.columns
                      WHERE table_schema = 'HappyNotes'
                        AND table_name = 'NoteTag'
                        AND column_name = 'UserId');

-- Prepare and execute the statement only if the column does not exist
SET @stmt = IF(@column_exists = 0, 'ALTER TABLE HappyNotes.NoteTag ADD UserId BIGINT NOT NULL COMMENT "=Note.UserId" AFTER Id',
               'SELECT "Column already exists" AS Result');

PREPARE stmt FROM @stmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

use HappyNotes;
UPDATE NoteTag, Note SET NoteTag.UserId = Note.UserId WHERE NoteTag.NoteId = Note.Id;
