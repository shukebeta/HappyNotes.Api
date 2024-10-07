-- Start of the script
USE HappyNotes;

DELIMITER //

CREATE PROCEDURE AdjustFilesTable()
BEGIN
    -- Change FileExt from char(4) to char(5) if it exists
    IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'Files' AND COLUMN_NAME = 'FileExt') > 0 THEN
        SET @sql = 'ALTER TABLE Files CHANGE COLUMN FileExt FileExt CHAR(5) NOT NULL';
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    -- Rename CreateAt to CreatedAt if it exists
    IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'Files' AND COLUMN_NAME = 'CreateAt') > 0 THEN
        SET @sql = 'ALTER TABLE Files CHANGE COLUMN CreateAt CreatedAt BIGINT DEFAULT NULL';
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    -- Rename UpdateAt to UpdatedAt if it exists
    IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'Files' AND COLUMN_NAME = 'UpdateAt') > 0 THEN
        SET @sql = 'ALTER TABLE Files CHANGE COLUMN UpdateAt UpdatedAt BIGINT DEFAULT NULL';
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //

DELIMITER ;

-- Call the procedure to adjust the table
CALL AdjustFilesTable();

-- Optionally, drop the procedure afterwards
DROP PROCEDURE IF EXISTS AdjustFilesTable;
