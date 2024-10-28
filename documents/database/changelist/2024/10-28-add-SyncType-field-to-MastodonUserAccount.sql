-- Check if the 'TelegramMessageIds' field exists
SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'HappyNotes'
  AND TABLE_NAME = 'MastodonUserAccount'
  AND COLUMN_NAME = 'SyncType';

-- If the 'TelegramMessageIds' field does not exist, add it
SET @sql = IF(@exists = 0,
              'ALTER TABLE `HappyNotes`.`MastodonUserAccount` ADD `SyncType` INT NOT NULL DEFAULT ''1'' COMMENT ''SyncType 1 Normal 2 Inactivate'' AFTER `Status`;',
              'SELECT ''Column SyncType already exists.'' AS Result');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
