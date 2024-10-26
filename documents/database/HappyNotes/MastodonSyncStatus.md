# Database: HappyNotes Table: MastodonSyncStatus

 Field           | Type          | Null | Default | Comment
-----------------|---------------|------|---------|---------
 Id              | bigint        | NO   |         |
 InstanceUrl     | varchar(255)  | NO   |         |
 NoteId          | bigint        | NO   |         |
 UserId          | bigint        | NO   |         |
 ApplicationId   | int           | NO   |         |
 TootId          | varchar(255)  | YES  |         |
 SyncStatus      | int           | NO   |         |
 LastSyncAttempt | bigint        | YES  |         |
 ErrorMessage    | varchar(1024) | YES  |         |
 CreatedAt       | bigint        | NO   |         |
 UpdatedAt       | bigint        | NO   |         |

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 InstanceUrl | InstanceUrl |            1 |          0 | BTREE      | YES
 InstanceUrl | NoteId      |            2 |          0 | BTREE      | YES
 SyncStatus  | SyncStatus  |            1 |          1 | BTREE      | YES
 NoteId      | NoteId      |            1 |          1 | BTREE      | YES
