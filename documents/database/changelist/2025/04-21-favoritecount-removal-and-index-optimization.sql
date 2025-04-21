USE HappyNotes;
-- Drop idx_FavoriteCount index if it exists
SET @drop_idx_FavoriteCount = (
    SELECT IF(
        COUNT(*) > 0,
        CONCAT('ALTER TABLE Note DROP INDEX ', INDEX_NAME),
        'SELECT 1' -- Placeholder to avoid syntax error
    )
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Note'
      AND INDEX_NAME = 'idx_FavoriteCount'
    LIMIT 1
);

PREPARE stmt_drop_FavoriteCount FROM @drop_idx_FavoriteCount;
EXECUTE stmt_drop_FavoriteCount;
DEALLOCATE PREPARE stmt_drop_FavoriteCount;

-- Drop FavoriteCount column if it exists
SET @drop_FavoriteCount_column = (
    SELECT IF(
        EXISTS(
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'Note'
              AND COLUMN_NAME = 'FavoriteCount'
        ),
        'ALTER TABLE Note DROP COLUMN FavoriteCount',
        'SELECT 1' -- Placeholder to avoid syntax error
    )
);

PREPARE stmt_drop_column FROM @drop_FavoriteCount_column;
EXECUTE stmt_drop_column;
DEALLOCATE PREPARE stmt_drop_column;

-- Create new composite index if it does not exist
SET @create_new_index = (
    SELECT IF(
        COUNT(*) = 0,
        'CREATE INDEX idx_user_deleted_private ON Note (UserId, DeletedAt, IsPrivate)',
        'SELECT 1' -- Placeholder to avoid syntax error
    )
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Note'
      AND INDEX_NAME = 'idx_user_deleted_private'
);

PREPARE stmt_create_index FROM @create_new_index;
EXECUTE stmt_create_index;
DEALLOCATE PREPARE stmt_create_index;
