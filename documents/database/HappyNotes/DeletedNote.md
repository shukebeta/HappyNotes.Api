# Database: HappyNotes Table: DeletedNote

 Field         | Type          | Null | Default | Comment
---------------|---------------|------|---------|---------
 Id            | bigint        | NO   |         |
 UserId        | bigint        | NO   | 0       |
 Content       | varchar(1024) | NO   |         |
 FavoriteCount | int           | NO   | 0       |
 IsLong        | tinyint       | NO   | 0       |
 IsPrivate     | tinyint       | NO   | 1       |
 Status        | tinyint       | NO   | 1       |
 CreateAt      | bigint        | NO   |         |
 UpdateAt      | bigint        | YES  |         |
 DeleteAt      | bigint        | YES  |         |

## Indexes: 

 Key_name | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------|-------------|--------------|------------|------------|---------
 PRIMARY  | Id          |            1 |          0 | BTREE      | YES
