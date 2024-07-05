# Database: HappyNotes Table: Files

 Field    | Type      | Null | Default | Comment
----------|-----------|------|---------|---------
 Id       | bigint    | NO   |         |
 Md5      | char(32)  | NO   |         |
 Path     | char(20)  | NO   |         |
 FileExt  | char(4)   | NO   |         |
 RefCount | int       | YES  |         |
 CreateAt | bigint    | YES  |         |
 UpdateAt | bigint    | YES  |         |
 FileName | char(128) | YES  |         |

## Indexes: 

 Key_name | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------|-------------|--------------|------------|------------|---------
 PRIMARY  | Id          |            1 |          0 | BTREE      | YES
 Md5      | Md5         |            1 |          0 | BTREE      | YES
