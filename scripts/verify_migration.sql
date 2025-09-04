-- Data Migration Verification Script
-- Run this after data migration to verify integrity

-- 1. Check total record count matches
SELECT 'Original Count' as source, COUNT(*) as count FROM noteindex_backup
UNION ALL  
SELECT 'New Count' as source, COUNT(*) as count FROM noteindex;

-- 2. Check for any missing records by ID
SELECT 'Missing Records' as check_type, COUNT(*) as count
FROM noteindex_backup b 
WHERE NOT EXISTS (SELECT 1 FROM noteindex n WHERE n.Id = b.Id);

-- 3. Sample a few records to verify content integrity
SELECT Id, UserId, LEFT(Content, 100) as content_preview, Tags 
FROM noteindex 
ORDER BY CreatedAt DESC 
LIMIT 5;

-- 4. Verify bigram tokenization is working
SELECT * FROM noteindex WHERE MATCH('测试');
SELECT * FROM noteindex WHERE MATCH('清结');

-- 5. Show table structure
DESCRIBE noteindex;