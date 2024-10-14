-- Check if the column 'Tags' exists in the 'HappyNotes.Note' table
SET @column_exists = (SELECT COUNT(*)
                      FROM information_schema.columns
                      WHERE table_schema = 'HappyNotes'
                        AND table_name = 'Note'
                        AND column_name = 'Tags');

-- Prepare and execute the statement only if the column does not exist
SET @stmt = IF(@column_exists = 0, 'ALTER TABLE HappyNotes.Note ADD Tags VARCHAR(512) NULL COMMENT "space separated tag list" AFTER Content',
               'SELECT ''Column already exists'' AS Result');

PREPARE stmt FROM @stmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if the column 'TagName' exists in the 'HappyNotes.NoteTag' table
SET @column_exists = (SELECT COUNT(*)
                      FROM information_schema.columns
                      WHERE table_schema = 'HappyNotes'
                        AND table_name = 'NoteTag'
                        AND column_name = 'TagName');

-- If 'TagName' exists, rename it to 'Tag'
SET @stmt = IF(@column_exists = 1,
               'ALTER TABLE HappyNotes.NoteTag CHANGE TagName Tag VARCHAR(32) NOT NULL COMMENT "Note tag in lowercase"',
               'SELECT ''Column TagName does not exist" AS Result');
PREPARE stmt FROM @stmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if the column 'Tag' has the desired properties
SET @column_correct = (SELECT COUNT(*)
                       FROM information_schema.columns
                       WHERE table_schema = 'HappyNotes'
                         AND table_name = 'NoteTag'
                         AND column_name = 'Tag'
                         AND column_type = 'varchar(32)'
                         AND is_nullable = 'NO'
                         AND column_comment = 'Note tag, put #tag1 tag2 tag3 in note content');

-- If 'Tag' does not have the desired properties, alter it
SET @stmt = IF(@column_correct = 0,
               'ALTER TABLE HappyNotes.NoteTag CHANGE Tag Tag VARCHAR(32) NOT NULL COMMENT "Note tag in lowercase"',
               'SELECT ''Column Tag already has the correct properties'' AS Result');
PREPARE stmt FROM @stmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
