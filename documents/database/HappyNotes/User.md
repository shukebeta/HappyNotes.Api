# Database: HappyNotes Table: User

 Field         | Type         | Null | Default | Comment
---------------|--------------|------|---------|---------
 Id            | bigint       | NO   |         |
 Username      | varchar(20)  | NO   |         |
 Email         | varchar(128) | NO   |         |
 EmailVerified | tinyint      | NO   | 0       |
 Gravatar      | varchar(512) | YES  |         |
 Password      | varchar(64)  | NO   |         |
 Salt          | varchar(64)  | NO   |         |
 CreateAt      | bigint       | NO   |         |
 UpdateAt      | bigint       | YES  |         |
 DeleteAt      | bigint       | YES  |         |

## Indexes: 

 Key_name | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------|-------------|--------------|------------|------------|---------
 PRIMARY  | Id          |            1 |          0 | BTREE      | YES
 UserName | Username    |            1 |          0 | BTREE      | YES
 Email    | Email       |            1 |          0 | BTREE      | YES
