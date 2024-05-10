# Database: HappyNotes Table: Tag

 Field        | Type            | Null | Default | Comment
--------------|-----------------|------|---------|----------
 Id           | bigint unsigned | NO   |         |
 Name         | varchar(128)    | NO   |         | Tag name
 PublicCount  | int             | NO   | 0       |
 PrivateCount | int             | NO   | 0       |
 TotalCount   | int             | NO   | 0       |
 CreateAt     | bigint          | NO   |         |
 UpdateAt     | bigint          | YES  |         |
 DeleteAt     | bigint          | YES  |         |
 CreateBy     | bigint          | NO   | 0       |
 UpdateBy     | bigint          | YES  |         |

## Indexes: 

 Key_name       | Column_name  | Seq_in_index | Non_unique | Index_type | Visible
----------------|--------------|--------------|------------|------------|---------
 PRIMARY        | Id           |            1 |          0 | BTREE      | YES
 I_Name         | Name         |            1 |          0 | BTREE      | YES
 UI_PublicCount | PublicCount  |            1 |          1 | BTREE      | YES
 I_PrivateCount | PrivateCount |            1 |          1 | BTREE      | YES
 I_TotalCount   | TotalCount   |            1 |          1 | BTREE      | YES
