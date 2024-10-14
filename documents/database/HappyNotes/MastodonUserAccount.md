# Database: HappyNotes Table: MastodonUserAccount

 Field          | Type          | Null | Default | Comment
----------------|---------------|------|---------|------------------------------------------------------
 Id             | bigint        | NO   |         |
 UserId         | bigint        | NO   |         |
 ApplicationId  | int           | NO   |         |
 MastodonUserId | varchar(255)  | NO   |         |
 InstanceUrl    | varchar(255)  | NO   |         |
 Username       | varchar(255)  | NO   |         |
 DisplayName    | varchar(255)  | YES  |         |
 AvatarUrl      | varchar(255)  | YES  |         |
 AccessToken    | varchar(255)  | NO   |         |
 RefreshToken   | varchar(255)  | YES  |         |
 TokenType      | varchar(50)   | NO   |         |
 Scope          | varchar(255)  | NO   |         |
 Status         | int           | NO   |         | Reference MastodonUserAccountStatus enum for details
 StatusText     | varchar(1024) | YES  |         |
 ExpiresAt      | bigint        | YES  |         |
 CreatedAt      | bigint        | NO   |         |
 UpdatedAt      | bigint        | YES  |         |

## Indexes: 

 Key_name | Column_name   | Seq_in_index | Non_unique | Index_type | Visible
----------|---------------|--------------|------------|------------|---------
 PRIMARY  | Id            |            1 |          0 | BTREE      | YES
 UserId   | UserId        |            1 |          0 | BTREE      | YES
 UserId   | ApplicationId |            2 |          0 | BTREE      | YES
 UserId_2 | UserId        |            1 |          1 | BTREE      | YES
