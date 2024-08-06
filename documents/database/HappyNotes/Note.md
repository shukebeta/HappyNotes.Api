# Database: HappyNotes Table: Note

 Field         | Type          | Null | Default | Comment
---------------|---------------|------|---------|-----------------------------------------------------
 Id            | bigint        | NO   |         |
 UserId        | bigint        | NO   | 0       |
 Content       | varchar(1024) | NO   |         |
 Tags          | varchar(512)  | YES  |         |
 FavoriteCount | int           | NO   | 0       |
 IsLong        | tinyint       | NO   | 0       |
 IsPrivate     | tinyint       | NO   | 1       |
 IsMarkdown    | tinyint       | NO   | 0       | indicate content field is in markdown format or not
 CreatedAt     | bigint        | NO   | 0       | A unix timestamp
 UpdatedAt     | bigint        | YES  |         | A unix timestamp
 DeletedAt     | bigint        | YES  |         | A unix timestamp

## Indexes: 

 Key_name            | Column_name   | Seq_in_index | Non_unique | Index_type | Visible
---------------------|---------------|--------------|------------|------------|---------
 PRIMARY             | Id            |            1 |          0 | BTREE      | YES
 idx_FavoriteCount   | FavoriteCount |            1 |          1 | BTREE      | YES
 idx_CreateAt        | CreatedAt     |            1 |          1 | BTREE      | YES
 idx_DeleteAt        | DeletedAt     |            1 |          1 | BTREE      | YES
 idx_UserId_DeleteAt | UserId        |            1 |          1 | BTREE      | YES
 idx_UserId_DeleteAt | DeletedAt     |            2 |          1 | BTREE      | YES
