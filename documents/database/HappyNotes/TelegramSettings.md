# Database: HappyNotes Table: TelegramSettings

 Field          | Type          | Null | Default | Comment
----------------|---------------|------|---------|---------------------------------------------
 Id             | int           | NO   |         |
 UserId         | bigint        | NO   |         |
 SyncType       | tinyint       | NO   |         |
 SyncValue      | varchar(32)   | NO   |         |
 EncryptedToken | varchar(128)  | NO   |         |
 ChannelId      | varchar(64)   | NO   |         | Telegram channel ID for syncing
 ChannelName    | varchar(64)   | NO   |         |
 TokenRemark    | varchar(64)   | YES  |         |
 Status         | tinyint       | NO   | 1       | See TelegramSettingsStatus enum for details
 LastError      | varchar(1024) | YES  |         |
 CreatedAt      | bigint        | NO   | 0       | A unix timestamp

## Indexes: 

 Key_name | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------|-------------|--------------|------------|------------|---------
 PRIMARY  | Id          |            1 |          0 | BTREE      | YES
 UserId   | UserId      |            1 |          0 | BTREE      | YES
 UserId   | SyncType    |            2 |          0 | BTREE      | YES
 UserId   | SyncValue   |            3 |          0 | BTREE      | YES
 SyncType | SyncType    |            1 |          1 | BTREE      | YES
