-- Check if the 'ChannelName' field already exists
SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'HappyNotes'
  AND TABLE_NAME = 'TelegramSettings'
  AND COLUMN_NAME = 'ChannelName';

-- If the 'ChannelName' field does not exist, add it
SET @sql = IF(@exists = 0,
              'ALTER TABLE `HappyNotes`.`TelegramSettings` ADD `ChannelName` VARCHAR(64) NOT NULL DEFAULT \'\' AFTER `ChannelId`;',
              'SELECT ''Column ChannelName already exists.'' AS Result');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if the 'StatusText' field exists
SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'HappyNotes'
  AND TABLE_NAME = 'TelegramSettings'
  AND COLUMN_NAME = 'StatusText';

-- If the 'StatusText' field exists, change it to 'LastError'
SET @sql = IF(@exists = 1,
              'ALTER TABLE `HappyNotes`.`TelegramSettings` CHANGE `StatusText` `LastError` VARCHAR(1024) DEFAULT NULL;',
              'SELECT ''Column StatusText does not exist.'' AS Result');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
