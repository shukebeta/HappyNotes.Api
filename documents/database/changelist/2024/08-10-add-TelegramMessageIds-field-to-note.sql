-- Check if the 'TelegramMessageIds' field exists
SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'HappyNotes'
  AND TABLE_NAME = 'Note'
  AND COLUMN_NAME = 'TelegramMessageIds';

-- If the 'TelegramMessageIds' field does not exist, add it
SET @sql = IF(@exists = 0,
              'ALTER TABLE `HappyNotes`.`Note` ADD `TelegramMessageIds` VARCHAR(512) NULL COMMENT ''Comma-separated telegram MessageId list'' AFTER `Tags`;',
              'SELECT ''Column TelegramMessageIds already exists.'' AS Result');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
