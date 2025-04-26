-- Manticore Index Synchronization Script for HappyNotes
-- This script synchronizes existing notes data to the RT index 'idx_notes'

-- Connect to the HappyNotes database to fetch notes data
-- You may need to adjust the connection parameters based on your environment
USE HappyNotes;

-- Generate REPLACE INTO statements for Manticore RT index
-- This query fetches all notes and prepares data for insertion into idx_notes
SELECT
    CONCAT(
        'REPLACE INTO noteindex (Id, UserId, IsLong, IsPrivate, IsMarkdown, Content, CreatedAt, UpdatedAt, DeletedAt) VALUES (',
        n.Id, ', ',
        n.UserId, ', ',
        n.IsLong, ', ',
        n.IsPrivate, ', ',
        n.IsMarkdown, ', ',
        '''',
        REPLACE(IF(n.IsLong, l.Content, n.Content), '''', '\\'''),
        '''', ', ',
        n.CreatedAt, ', ',
        IFNULL(n.UpdatedAt, n.CreatedAt), ', ',
        IFNULL(n.DeletedAt, 0), ');'
    ) AS query
FROM Note n
LEFT JOIN LongNote l ON n.Id = l.Id;

-- Instructions for execution:
-- 1. Run this script in your MySQL client within Docker to generate the REPLACE INTO statements.
--    Use the following command to execute the script and redirect output to a file:
--    docker-compose -f docker/docker-compose.yml exec mysql mysql -u your_username -p your_password HappyNotes < bin/sync-manticore-index.sql > manticore_sync.sql
--    Replace 'your_username' and 'your_password' with your MySQL credentials.
-- 2. Connect to the Manticore server using a MySQL client.
--    You can do this by running:
--    docker-compose -f docker/docker-compose.yml exec manticore mysql -h 127.0.0.1 -P 9306
-- 3. Execute the generated SQL file in the Manticore client:
--    source manticore_sync.sql
--    This will update all existing notes to the RT index.

-- Note: Ensure you adjust the file path for 'manticore_sync.sql' if needed based on your environment.
