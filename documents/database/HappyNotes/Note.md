# Database: HappyNotes Table: Note

 Field         | Type          | Null | Default | Comment
---------------|---------------|------|---------|-----------------------------------------------------
 Id            | bigint        | NO   |         |
 UserId        | bigint        | NO   | 0       |
 Content       | varchar(1024) | NO   |         |
 FavoriteCount | int           | NO   | 0       |
 IsLong        | tinyint       | NO   | 0       |
 IsPrivate     | tinyint       | NO   | 1       |
 IsMarkdown    | tinyint       | NO   | 0       | indicate content field is in markdown format or not
 CreateAt      | bigint        | NO   |         |
 UpdateAt      | bigint        | YES  |         |
 DeleteAt      | bigint        | YES  |         |

## Indexes: 

 Key_name            | Column_name   | Seq_in_index | Non_unique | Index_type | Visible
---------------------|---------------|--------------|------------|------------|---------
 PRIMARY             | Id            |            1 |          0 | BTREE      | YES
 idx_FavoriteCount   | FavoriteCount |            1 |          1 | BTREE      | YES
 idx_CreateAt        | CreateAt      |            1 |          1 | BTREE      | YES
 idx_DeleteAt        | DeleteAt      |            1 |          1 | BTREE      | YES
 idx_UserId_DeleteAt | UserId        |            1 |          1 | BTREE      | YES
 idx_UserId_DeleteAt | DeleteAt      |            2 |          1 | BTREE      | YES
