# Database: HappyNotes Table: MastodonUserAccount

 Field       | Type         | Null | Default | Comment
-------------|--------------|------|---------|------------------------------------------------------
 Id          | bigint       | NO   |         |
 UserId      | bigint       | NO   |         |
 InstanceUrl | varchar(255) | NO   |         |
 AccessToken | varchar(255) | NO   |         |
 TokenType   | varchar(50)  | NO   |         |
 Scope       | varchar(255) | NO   |         |
 Status      | int          | NO   |         | Reference MastodonUserAccountStatus enum for details
 SyncType    | int          | NO   | 1       | SyncType 1 Normal 2 Inactivate
 CreatedAt   | bigint       | NO   |         |

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 InstanceUrl | InstanceUrl |            1 |          0 | BTREE      | YES
 InstanceUrl | UserId      |            2 |          0 | BTREE      | YES
 UserId      | UserId      |            1 |          1 | BTREE      | YES
