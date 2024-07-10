ALTER TABLE `HappyNotes`.`Note`
    ADD INDEX `idx_FavoriteCount` (`FavoriteCount`),
    ADD INDEX `idx_CreateAt` (`CreateAt`),
    ADD INDEX `idx_DeleteAt` (`DeleteAt`),
    ADD INDEX `idx_UserId_DeleteAt` (`UserId`, `DeleteAt`);

ALTER TABLE `HappyNotes`.`Note` DROP COLUMN Status;
